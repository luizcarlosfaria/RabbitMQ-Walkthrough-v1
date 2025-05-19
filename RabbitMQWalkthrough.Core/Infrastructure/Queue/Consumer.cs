using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWalkthrough.Core.Infrastructure;
using RabbitMQWalkthrough.Core.Infrastructure.Data;
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
        private readonly IChannel channel;
        private readonly IConnection connection;
        private readonly ILogger<Consumer> logger;
        private readonly NpgsqlConnection sqlConnection;
        private readonly MessageDataService messageDataService;
        private string queue;
        private AsyncEventingBasicConsumer eventingBasicConsumer;
        private volatile bool isRunning;
        private bool isInitialized;

        public string ConsumerTag { get; private set; }
        public int MessagesPerSecond { get; private set; }

        public string Id { get; }

        public Consumer(IChannel channel, IConnection connection, ILogger<Consumer> logger, NpgsqlConnection sqlConnection, MessageDataService messageDataService)
        {
            this.channel = channel;
            this.connection = connection;
            this.logger = logger;
            this.sqlConnection = sqlConnection;
            this.messageDataService = messageDataService;
            this.Id = Guid.NewGuid().ToString("D");
            this.eventingBasicConsumer = new AsyncEventingBasicConsumer(channel);
            this.eventingBasicConsumer.ReceivedAsync += this.OnMessage;
        }

        public void Initialize(string queue, int messagesPerSecond)
        {
            if (this.isInitialized) throw new InvalidOperationException("Initialize só pode ser chamado uma vez");
            this.queue = queue;
            this.MessagesPerSecond = messagesPerSecond;
            this.isInitialized = true;
        }


        private async Task OnMessage(object sender, BasicDeliverEventArgs eventArgs)
        {
            //Thread.Sleep(TimeSpan.FromSeconds(2));

            if (this.isRunning == false)
            {
                if (this.channel.IsOpen)
                {
                    await this.channel.BasicRejectAsync(eventArgs.DeliveryTag, true);
                    this.logger.LogInformation("Mensagem sofreu rejeição leve em função do desligamento do consumidor");
                }
                return;
            }
            if (this.MessagesPerSecond != 0)
                this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

            Message message = null;

            try
            {
                message = eventArgs.Body.ToArray().ToUTF8String().Deserialize<Message>();
            }
            catch (Exception ex)
            {
                if (this.channel.IsOpen)
                {
                    await this.channel.BasicRejectAsync(eventArgs.DeliveryTag, false);
                    this.logger.LogError(ex, "Mensagem sofreu uma rejeição grave em função de um erro na desserialização. A mensagem será descartada");
                }
                return;
            }


            if (this.isRunning)
            {
                using NpgsqlTransaction sqlTransaction = this.sqlConnection.BeginTransaction();
                try
                {
                    if (this.isRunning)
                    {
                        this.messageDataService.MarkAsProcessed(message, this.sqlConnection, sqlTransaction);

                        if (this.isRunning)
                        {
                            sqlTransaction.Commit();
                            await this.channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                        }
                        else
                        {
                            this.logger.LogInformation("Abordando procesamento sem ack e sem commit");
                        }
                    }
                    else
                    {
                        if (this.channel.IsOpen)
                        {
                            await this.channel.BasicRejectAsync(eventArgs.DeliveryTag, true);
                            this.logger.LogInformation("Mensagem sofreu rejeição leve em função do desligamento do consumidor");
                        }
                        sqlTransaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    if (this.channel.IsOpen)
                    {
                        await this.channel.BasicNackAsync(eventArgs.DeliveryTag, false, true);
                        this.logger.LogError(ex, "Mensagem foi reenfileirada para processamento futuro, o consumidor atual não conseguiu processá-la.");
                    }

                    if (sqlTransaction.Connection != null)
                        sqlTransaction.Rollback();
                }
            }
            else
            {
                if (this.channel.IsOpen) await this.channel.BasicRejectAsync(eventArgs.DeliveryTag, true);
            }
        }


        public async Task<Consumer> StartAsync()
        {
            if (this.isInitialized == false) throw new InvalidOperationException("Instancia não inicializada");



            if (this.MessagesPerSecond > 0)
                await this.channel.SetPrefetchCountAsync((ushort)(this.MessagesPerSecond * 5));
            else
                await this.channel.SetPrefetchCountAsync((ushort)(1000));

            this.isRunning = true;

            this.ConsumerTag = await this.channel.BasicConsumeAsync(this.queue, false, this.eventingBasicConsumer);

            return this;
        }

        public async Task<Consumer> Stop()
        {
            this.isRunning = false;

            if (!string.IsNullOrWhiteSpace(this.ConsumerTag))
            {
                await this.channel.BasicCancelAsync(this.ConsumerTag);
            }


            await this.channel.CloseAsync();

            this.channel.Dispose();

            await this.connection.CloseAsync();

            this.connection.Dispose();

            this.sqlConnection.Close();

            return this;
        }

    }
}
