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
    public class Consumer
    {
        private readonly IModel model;
        private readonly string queue;
        private EventingBasicConsumer eventingBasicConsumer;
        public string ConsumerTag { get; private set; }


        public Consumer(IModel model, string queue, int messagesPerSecond)
        {
            this.model = model;
            this.queue = queue;
            this.MessagesPerSecond = messagesPerSecond;
            this.Id = Guid.NewGuid().ToString("D");

            this.eventingBasicConsumer = new EventingBasicConsumer(model);
            this.eventingBasicConsumer.Received += this.OnMessage;
        }

        public int MessagesPerSecond { get; }

        public string Id { get; }

        private void OnMessage(object sender, BasicDeliverEventArgs e)
        {
            this.MessagesPerSecond.AsMessageRateToSleepTimeSpan().Wait();

            Message message = e.Body.ToArray().ToUTF8String().Deserialize<Message>();

            message.Processed = DateTime.Now;

            //Console.WriteLine(message.Serialize());

            if (this.model.IsOpen)
                this.model.BasicAck(e.DeliveryTag, false);

            //return Task.CompletedTask;
        }

        public Consumer Start()
        {
            this.model.SetPrefetchCount((ushort)(this.MessagesPerSecond*4));

            this.ConsumerTag = this.model.BasicConsume(this.queue, false, this.eventingBasicConsumer);

            return this;
        }

        public Consumer Stop()
        {
            if (!string.IsNullOrWhiteSpace(this.ConsumerTag))
            {
                this.model.BasicCancel(this.ConsumerTag);

                this.model.Close();

                this.model.Dispose();
            }

            return this;
        }

    }
}
