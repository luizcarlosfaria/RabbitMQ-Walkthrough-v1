using Microsoft.Extensions.DependencyInjection;
using Polly;
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

        public static TimeSpan AsMessageRateToSleepTimeSpan(this int messagesPerSecond)
        {
            if (messagesPerSecond < 1)
                throw new ArgumentOutOfRangeException(nameof(messagesPerSecond));


            var sleepTimer = 1000 / messagesPerSecond;


            return TimeSpan.FromMilliseconds(sleepTimer);
        }

        public static IServiceCollection AddTransientWithRetry<TService, TKnowException>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TKnowException : Exception
            where TService : class
        {
            return services.AddTransient(sp =>
            {
                TService returnValue = default;

                var policy = Policy
                    .Handle<TKnowException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

                policy.Execute(() =>
                {
                    returnValue = implementationFactory(sp);

                });

                return returnValue;

            });
        }



    }
}
