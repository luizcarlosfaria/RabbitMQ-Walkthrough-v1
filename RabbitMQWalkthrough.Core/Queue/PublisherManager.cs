using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace RabbitMQWalkthrough.Core.Queue
{
    public class PublisherManager
    {
        private readonly IServiceProvider serviceProvider;
        private Queue<Publisher> publishers = new Queue<Publisher>();

        public PublisherManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<Publisher> Publishers => this.publishers;


        public void AddPublisher(int size, int messagesPerSecond)
        {
            

            if (size > 0)
                for (var i = 1; i <= size; i++)
                {
                    var connection = serviceProvider.GetRequiredService<IConnection>();
                    var model = connection.CreateModel();
                    var sqlConnection = serviceProvider.GetRequiredService<SqlConnection>();

                    publishers.Enqueue(new Publisher(model, connection, sqlConnection, "test_exchange", messagesPerSecond).Start());
                }
        }



        public void RemovePublisher(string id)
        {
            if (publishers.Count > 0)
            {
                var publisher = publishers.SingleOrDefault(it => it.Id == id);
                if (publisher != null)
                {
                    var otherPublishers = publishers.Where(it => it.Id != id).ToList();
                    publishers.Clear();
                    otherPublishers.ForEach(publishers.Enqueue);
                    publisher.Stop();
                }
            }
        }
    }
}
