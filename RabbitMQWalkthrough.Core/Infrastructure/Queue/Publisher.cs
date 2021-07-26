
using Dapper;
using Microsoft.Extensions.Logging;
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
    public class Publisher
    {
        private readonly IModel model;
        private readonly IConnection connection;
        private readonly SqlConnection sqlConnection;
        private readonly MessageDataService messageDataService;
        private readonly ILogger<Publisher> logger;
        private  string exchange;
        private Thread runThread;
        private volatile bool isRunning;


        public int MessagesPerSecond { get; private set; }

        private bool isInitialized;

        public string Id { get; }

        public Publisher(IModel model, IConnection connection, SqlConnection sqlConnection, MessageDataService messageDataService, ILogger<Publisher> logger)
        {
            this.model = model;
            this.connection = connection;
            this.sqlConnection = sqlConnection;
            this.messageDataService = messageDataService;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");

            model.ConfirmSelect();

            this.runThread = new Thread(this.HandlePublish);
        }

        public void Initialize(string exchange, int messagesPerSecond)
        {
            if (this.isInitialized) throw new InvalidOperationException("Initialize só pode ser chamado uma vez");
            this.exchange = exchange;
            this.MessagesPerSecond = messagesPerSecond;
            this.isInitialized = true;
        }

        private void HandlePublish()
        {
            long count = 0;
            while (this.isRunning)
            {
                this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

                count++;

                //Esse controle transacional deveria ser abstraído
                using var transaction = this.sqlConnection.BeginTransaction();
                try
                {
                    /*Aqui deveria chamar alguma camada de negócio*/
                    Message message = this.messageDataService.CreateMessage(transaction, this.sqlConnection);
                    //fim

                    this.model.BasicPublish(
                        exchange: this.exchange,
                        routingKey: string.Empty,
                        mandatory: true,
                        basicProperties: this.model.CreatePersistentBasicProperties(),
                        body: message.Serialize().ToByteArray().ToReadOnlyMemory());


                    this.model.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    this.logger.LogError(ex, "Erro ao publicar mensagem. Transação com banco foi abortada.");
                }
            }

            this.model.Close();

            this.model.Dispose();

            this.connection.Close();

            this.connection.Dispose();

            this.sqlConnection.Close();

        }



        public Publisher Start()
        {
            if (this.isInitialized == false) throw new InvalidOperationException("Instancia não inicializada");

            this.isRunning = true;
            this.runThread.Start();
            return this;
        }

        public Publisher Stop()
        {
            this.isRunning = false;

            return this;
        }

    }
}
