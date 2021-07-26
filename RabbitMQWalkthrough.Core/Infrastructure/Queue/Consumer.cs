using Dapper;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWalkthrough.Core.Infrastructure;
using RabbitMQWalkthrough.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Queue
{
    public class Consumer
    {
        private readonly IModel model;
        private readonly IConnection connection;
        private readonly ILogger<Consumer> logger;
        private readonly SqlConnection sqlConnection;
        private string queue;
        private EventingBasicConsumer eventingBasicConsumer;
        private volatile bool isRunning;
        private bool isInitialized;

        public string ConsumerTag { get; private set; }
        public int MessagesPerSecond { get; private set; }

        public string Id { get; }

        public Consumer(IModel model, IConnection connection, ILogger<Consumer> logger, SqlConnection sqlConnection)
        {
            this.model = model;
            this.connection = connection;
            this.logger = logger;
            this.sqlConnection = sqlConnection;
            this.Id = Guid.NewGuid().ToString("D");
            this.eventingBasicConsumer = new EventingBasicConsumer(model);
            this.eventingBasicConsumer.Received += this.OnMessage;
        }

        public void Initialize(string queue, int messagesPerSecond)
        {
            if (this.isInitialized) throw new InvalidOperationException("Initialize só pode ser chamado uma vez");
            this.queue = queue;
            this.MessagesPerSecond = messagesPerSecond;
            this.isInitialized = true;
        }


        private void OnMessage(object sender, BasicDeliverEventArgs e)
        {

            if (this.isRunning == false)
            {
                if (this.model.IsOpen)
                    this.model.BasicReject(e.DeliveryTag, true);

                this.logger.LogInformation("Mensagem sofreu rejeição leve em função do desligamento do consumidor");

                return;
            }

            this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

            Message message = null;

            try
            {
                message = e.Body.ToArray().ToUTF8String().Deserialize<Message>();
            }
            catch (Exception ex)
            {
                if (this.model.IsOpen)
                    this.model.BasicReject(e.DeliveryTag, false);

                this.logger.LogError(ex, "Mensagem sofreu uma rejeição grave em função de um erro na desserialização. A mensagem será descartada");

                return;
            }


            try
            {
                this.CallAgnosticService(message);

                this.model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                if (this.model.IsOpen)
                    this.model.BasicNack(e.DeliveryTag, false, true);

                this.logger.LogError(ex, "Mensagem foi reenfileirada para processamento futuro, o consumidor atual não conseguiu processá-la.");

                return;
            }

        }

        private void CallAgnosticService(Message message)
        {
            string sql = @"update [dbo].[Messages] set [Processed] = GETUTCDATE(), [Num] = [Num]+1 where [MessageId] = @MessageId;";
            this.sqlConnection.Execute(sql, message);
        }

        public Consumer Start()
        {
            if (this.isInitialized == false) throw new InvalidOperationException("Instancia não inicializada");

            this.model.SetPrefetchCount((ushort)(this.MessagesPerSecond * 5));

            this.isRunning = true;

            this.ConsumerTag = this.model.BasicConsume(this.queue, false, this.eventingBasicConsumer);

            return this;
        }

        public Consumer Stop()
        {
            this.isRunning = false;

            if (!string.IsNullOrWhiteSpace(this.ConsumerTag))
            {
                this.model.BasicCancel(this.ConsumerTag);
            }

            this.model.Close();

            this.model.Dispose();

            this.connection.Close();

            this.connection.Dispose();

            this.sqlConnection.Close();

            return this;
        }

    }
}
