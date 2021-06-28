using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Architecture
{
    public static partial class Extensions
    {

        public static void ReliablePublish<T>(this IModel model, T objectToPublish, string exchange, string routingKey)
        {
            if (model.IsOpen == false) return;

            var body = objectToPublish.Serialize().ToByteArray().ToReadOnlyMemory();

            var basicProperties = model.CreateBasicProperties();

            basicProperties.DeliveryMode = 2;

            model.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: basicProperties,
                body: body);

        }

        public static IModel SetPrefetchCount(this IModel model, ushort prefetchCount)
        {
            model.BasicQos(0, prefetchCount, false); 
            return model;
        }

    }
}
