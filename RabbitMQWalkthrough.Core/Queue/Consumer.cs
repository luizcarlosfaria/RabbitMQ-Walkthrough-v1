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
        private AsyncEventingBasicConsumer eventingBasicConsumer;
        public string ConsumerTag { get; private set; }


        public Consumer(IModel model, string queue)
        {
            this.model = model;
            this.queue = queue;

            this.eventingBasicConsumer = new AsyncEventingBasicConsumer(model);
            this.eventingBasicConsumer.Received += this.OnMessage;
        }

        private Task OnMessage(object sender, BasicDeliverEventArgs e)
        {
            TimeSpan.FromMilliseconds(500).Wait();

            Message message = e.Body.ToArray().ToUTF8String().Deserialize<Message>();

            message.Processed = DateTime.Now;

            //Console.WriteLine(message.Serialize());

            this.model.BasicAck(e.DeliveryTag, false);
            return Task.CompletedTask;
        }

        public Consumer Start()
        {
            this.ConsumerTag = this.model.BasicConsume(this.queue, false, this.eventingBasicConsumer);

            return this;
        }

        public Consumer Stop()
        {
            if (!string.IsNullOrWhiteSpace(this.ConsumerTag))
                this.model.BasicCancel(this.ConsumerTag);

            return this;
        }

    }
}
