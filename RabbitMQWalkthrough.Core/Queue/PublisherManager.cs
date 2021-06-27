using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
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

        public int PublisherCount => this.publishers.Count;


        public void AddPublisher()
        {
            publishers.Enqueue(new Publisher(serviceProvider.GetRequiredService<IModel>(), "test_exchange").Start());
        }


        public void RemovePublisher()
        {
            if (publishers.Count > 0)
                publishers.Dequeue().Stop();
        }
    }
}
