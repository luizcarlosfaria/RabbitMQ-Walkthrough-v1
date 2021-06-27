using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Architecture
{
    public static partial class Extensions
    {

        public static TimeSpan AsMessageRateToSleepTimeSpan(this int messagesPerSecond)
        {
            if ((messagesPerSecond < 1) || (messagesPerSecond > 500))
                throw new ArgumentOutOfRangeException("messageRate");


            var sleepTimer = 1000 / messagesPerSecond;


            return TimeSpan.FromMilliseconds(sleepTimer);
        }

    }
}
