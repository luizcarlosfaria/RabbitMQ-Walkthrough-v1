using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQWalkthrough.Core.Queue
{
    public class ConsumerManager
    {
        private readonly IServiceProvider serviceProvider;
        private Queue<Consumer> consumers = new Queue<Consumer>();

        public ConsumerManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public int ConsumerCount => this.consumers.Count;


        public void AddConsumer()
        {
            consumers.Enqueue(new Consumer(serviceProvider.GetRequiredService<IModel>(), "test_queue").Start());
        }


        public void RemoveConsumer()
        {
            if (consumers.Count > 0)
                consumers.Dequeue().Stop();
        }
    }
}
