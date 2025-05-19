using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure
{
    public static partial class Extensions
    {
        public static BasicProperties CreatePersistentBasicProperties(this IChannel channel) => new BasicProperties().SetDeliveryMode(DeliveryModes.Persistent);

        public static BasicProperties SetMessageId(this BasicProperties prop, string messageId)
        {
            prop.MessageId = messageId;
            return prop;
        }

        public static BasicProperties SetCorrelationId(this BasicProperties prop, string correlationId)
        {
            prop.CorrelationId = correlationId;
            return prop;
        }


        public static BasicProperties SetDeliveryMode(this BasicProperties prop, DeliveryModes deliveryMode)
        {
            prop.DeliveryMode = deliveryMode;

            return prop;
        }

        public static async Task<IChannel> SetPrefetchCountAsync(this IChannel channel, ushort prefetchCount)
        {
            await channel.BasicQosAsync(0, prefetchCount, false);
            return channel;
        }


    }
}
