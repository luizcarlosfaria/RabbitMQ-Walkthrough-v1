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
        public static IBasicProperties CreatePersistentBasicProperties(this IModel model) => model.CreateBasicProperties().SetDeliveryMode(2);


        public static IBasicProperties SetDeliveryMode(this IBasicProperties prop, byte deliveryMode)
        {
            prop.DeliveryMode = deliveryMode;

            return prop;
        }

        public static IModel SetPrefetchCount(this IModel model, ushort prefetchCount)
        {
            model.BasicQos(0, prefetchCount, false);
            return model;
        }


    }
}
