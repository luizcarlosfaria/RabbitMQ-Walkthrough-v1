
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

    /// <summary>
    /// Esse publisher foi desenhado para representar uma carga de trabalho que envia X mensagens por segundo.
    /// Diversas decisões foram tomadas com base nessa característica.    
    /// </summary>
    public class Publisher
    {
        private readonly IChannel channel;
        private readonly IConnection rabbitMqConnection;
        private readonly NpgsqlConnection sqlConnection;
        private readonly MessageDataService messageDataService;
        private readonly ILogger<Publisher> logger;
        private string exchange;
        private readonly Thread runThread;
        private volatile bool isRunning;


        public int MessagesPerSecond { get; private set; }

        private bool isInitialized;

        public string Id { get; }

        public Publisher(IChannel channel, IConnection rabbitMqConnection, NpgsqlConnection sqlConnection, MessageDataService messageDataService, ILogger<Publisher> logger)
        {
            this.channel = channel;
            this.rabbitMqConnection = rabbitMqConnection;
            this.sqlConnection = sqlConnection;
            this.messageDataService = messageDataService;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");

            this.runThread = new Thread(this.HandlePublishAsync);
        }

        public void Initialize(string exchange, int messagesPerSecond)
        {
            if (this.isInitialized) throw new InvalidOperationException("Initialize só pode ser chamado uma vez");
            this.exchange = exchange;
            this.MessagesPerSecond = messagesPerSecond;
            this.isInitialized = true;
        }

        private void HandlePublishAsync()
        {
            //this.channel.ConfirmSelect(); //Ack na publicação.

            long count = 0;
            while (this.isRunning)
            {
                if (this.MessagesPerSecond != 0)
                    this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

                count++;

                //Esse controle transacional deveria ser abstraído
                using NpgsqlTransaction transaction = this.sqlConnection.BeginTransaction();
                try
                {
                    //Thread.Sleep(TimeSpan.FromMilliseconds(100));

                    /*Aqui deveria chamar alguma camada de negócio*/
                    Message message = this.messageDataService.CreateMessage(transaction, this.sqlConnection);
                    //fim

                    this.channel.BasicPublishAsync(
                        exchange: this.exchange,
                        routingKey: string.Empty,
                        mandatory: true,
                        basicProperties: this.channel.CreatePersistentBasicProperties().SetMessageId(Guid.NewGuid().ToString("D")), //Extension Method para criar um basic properties com persistência
                        body: message.Serialize().ToByteArray().ToReadOnlyMemory()).GetAwaiter().GetResult(); //Extension Method para simplificar a publicação

                    //this.channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5)); //Ack na publicação.

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    this.logger.LogError(ex, "Erro ao publicar mensagem. Transação com banco foi abortada.");
                }
            }

            //se chegamos aqui, nosso worker foi parado.

            this.channel.CloseAsync().GetAwaiter().GetResult();

            this.channel.Dispose();

            this.rabbitMqConnection.CloseAsync().GetAwaiter().GetResult();

            this.rabbitMqConnection.Dispose();

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
