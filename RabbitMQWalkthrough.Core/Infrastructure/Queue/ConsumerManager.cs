using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Queue
{
    public class ConsumerManager
    {
        private readonly IServiceProvider serviceProvider;
        private List<Consumer> consumers = new List<Consumer>();

        public ConsumerManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<Consumer> Consumers => this.consumers.ToArray();


        private object syncLock = new();

        public async Task AddConsumerAsync(int size, int messagesPerSecond)
        {
            if (size > 0)
                for (int i = 1; i <= size; i++)
                {
                    Consumer consumer = this.serviceProvider.GetRequiredService<Consumer>();
                    consumer.Initialize("test_queue", messagesPerSecond);
                    lock (this.syncLock)
                    {
                        this.consumers.Add(consumer);
                    }
                    await consumer.StartAsync();
                }
        }

        public void RemoveConsumer(string id)
        {
            if (this.consumers.Count > 0)
            {
                lock (this.syncLock)
                {
                    Consumer consumer = this.consumers.SingleOrDefault(it => it.Id == id);
                    if (consumer != null)
                    {
                        this.consumers.Remove(consumer);
                        consumer.Stop();
                    }
                }
            }
        }
    }
}
