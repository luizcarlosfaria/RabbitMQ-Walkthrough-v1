
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
    public class Publisher
    {
        private readonly IModel model;
        private readonly IConnection connection;
        private readonly SqlConnection sqlConnection;
        private readonly ILogger<Publisher> logger;
        private  string exchange;
        private Thread runThread;
        private volatile bool isRunning;


        public int MessagesPerSecond { get; private set; }

        private bool isInitialized;

        public string Id { get; }

        public Publisher(IModel model, IConnection connection, SqlConnection sqlConnection, ILogger<Publisher> logger)
        {
            this.model = model;
            this.connection = connection;
            this.sqlConnection = sqlConnection;
            this.logger = logger;
            this.Id = Guid.NewGuid().ToString("D");

            model.ConfirmSelect();

            this.runThread = new Thread(() =>
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
                        Message message = this.CallAgnosticService(transaction);
                        //fim

                        model.BasicPublish(
                            exchange: this.exchange,
                            routingKey: string.Empty,
                            mandatory: true,
                            basicProperties: model.CreatePersistentBasicProperties(),
                            body: message.Serialize().ToByteArray().ToReadOnlyMemory());


                        model.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        logger.LogError(ex, "Erro ao publicar mensagem. Transação com banco foi abortada.");
                    }
                }

                this.model.Close();

                this.model.Dispose();

                this.connection.Close();

                this.connection.Dispose();

                this.sqlConnection.Close();
            });
        }

        public void Initialize(string exchange, int messagesPerSecond)
        {
            if (this.isInitialized) throw new InvalidOperationException("Initialize só pode ser chamado uma vez");
            this.exchange = exchange;
            this.MessagesPerSecond = messagesPerSecond;
            this.isInitialized = true;
        }

        private Message CallAgnosticService(SqlTransaction transaction)
        {
            string sql = @"
                            INSERT INTO [dbo].[Messages] ([Stored],[Num])  VALUES (GETUTCDATE(),0); 
                            SELECT * from [dbo].[Messages] where MessageId = SCOPE_IDENTITY();
                        ";
            Message message = this.sqlConnection.QuerySingle<Message>(sql, new { Created = DateTime.Now }, transaction);
            return message;
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
