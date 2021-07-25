using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

        public IEnumerable<Consumer> Consumers => this.consumers;


        public void AddConsumer(int size, int messagesPerSecond)
        {
            if (size > 0)
                for (var i = 1; i <= size; i++)
                {
                    var connection = serviceProvider.GetRequiredService<IConnection>();
                    var model = connection.CreateModel();

                    consumers.Enqueue(new Consumer(model, connection, "test_queue", messagesPerSecond).Start());
                }
        }




        public void RemoveConsumer(string id)
        {
            if (consumers.Count > 0)
            {
                var consumer = consumers.SingleOrDefault(it => it.Id == id);
                if (consumer != null)
                {
                    var otherConsumers = consumers.Where(it => it.Id != id).ToList();
                    consumers.Clear();
                    otherConsumers.ForEach(consumers.Enqueue);
                    consumer.Stop();
                }
            }
        }
    }
}
