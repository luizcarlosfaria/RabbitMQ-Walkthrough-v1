using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace RabbitMQWalkthrough.Core.Infrastructure.Queue
{
    public class PublisherManager
    {
        private readonly IServiceProvider serviceProvider;
        private List<Publisher> publishers = new List<Publisher>();

        public PublisherManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<Publisher> Publishers => this.publishers.ToArray();


        private object syncLock = new Object();

        public void AddPublisher(int size, int messagesPerSecond)
        {
            if (size > 0 && messagesPerSecond > 0) 
                for (var i = 1; i <= size; i++)
                {
                    var publisher = this.serviceProvider.GetRequiredService<Publisher>();

                    publisher.Initialize("test_exchange", messagesPerSecond);

                    lock (this.syncLock)
                    {
                        this.publishers.Add(publisher);
                    }

                    publisher.Start();
                }
        }



        public void RemovePublisher(string id)
        {
            if (this.publishers.Count > 0)
            {
                lock (this.syncLock)
                {
                    var publisher = this.publishers.SingleOrDefault(it => it.Id == id);
                    if (publisher != null)
                    {

                        this.publishers.Remove(publisher);
                    }
                    publisher.Stop();
                }
            }
        }
    }
}
