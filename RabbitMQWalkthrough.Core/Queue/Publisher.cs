
using Dapper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWalkthrough.Core.Architecture;
using RabbitMQWalkthrough.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Queue
{
    public class Publisher
    {
        private readonly IModel model;
        private readonly IConnection connection;
        private readonly SqlConnection sqlConnection;
        private readonly string exchange;
        private Thread runThread;
        private volatile bool isRunning;

        public Publisher(IModel model, IConnection connection, SqlConnection sqlConnection, string exchange, int messagesPerSecond)
        {
            this.model = model;
            this.connection = connection;
            this.sqlConnection = sqlConnection;
            this.exchange = exchange;
            this.MessagesPerSecond = messagesPerSecond;
            this.Id = Guid.NewGuid().ToString("D");

            //model.ConfirmSelect();

            this.runThread = new Thread(() =>
            {
                long count = 0;
                while (this.isRunning)
                {
                    this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

                    count++;

                    using (var transaction = this.sqlConnection.BeginTransaction())
                    {

                        string sql = @"
                    INSERT INTO [dbo].[Messages] ([Stored],[Num])  VALUES (GETUTCDATE(),0); 
                    
                    SELECT * from [dbo].[Messages] where MessageId = SCOPE_IDENTITY();
                    ";
                        Message message = this.sqlConnection.QuerySingle<Message>(sql, new { Created = DateTime.Now }, transaction);

                        model.ReliablePublish(message, this.exchange, string.Empty);

                        transaction.Commit();
                    }
                }

                this.model.Close();

                this.model.Dispose();

                this.connection.Close();

                this.connection.Dispose();

                this.sqlConnection.Close();
            });
        }

        public int MessagesPerSecond { get; }

        public string Id { get; }

        public Publisher Start()
        {
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
