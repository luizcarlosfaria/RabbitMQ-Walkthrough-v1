
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWalkthrough.Core.Architecture;
using RabbitMQWalkthrough.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Queue
{
    public class Publisher
    {
        private readonly IModel model;
        private readonly string exchange;
        private Thread runThread;
        private volatile bool isRunning;

        public Publisher(IModel model, string exchange, int messagesPerSecond)
        {
            this.model = model;
            this.exchange = exchange;
            this.MessagesPerSecond = messagesPerSecond;
            this.Id = Guid.NewGuid().ToString("D");

            //model.ConfirmSelect();

            this.runThread = new Thread(() =>
            {
                long count = 0;
                while (this.isRunning)
                {
                    count++;

                    this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();


                    var message = new Message()
                    {
                        Created = DateTime.Now,
                        MessageId = $"{this.Id}-{count}"
                    };

                    model.ReliablePublish(message, this.exchange, string.Empty);

                }

                this.model.Close();

                this.model.Dispose();
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
