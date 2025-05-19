using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
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


            int sleepTimer = 1000 / messagesPerSecond;


            return TimeSpan.FromMilliseconds(sleepTimer);
        }

        public static IServiceCollection AddTransientWithRetry<TService, TKnowException>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TKnowException : Exception
            where TService : class
        {
            return services.AddTransient(sp =>
            {
                TService returnValue = default;

                RetryPolicy policy = BuildPolicy<TKnowException>();

                policy.Execute(() =>
                {
                    returnValue = implementationFactory(sp);

                });

                return returnValue;

            });
        }

        public static IServiceCollection AddSingletonWithRetry<TService, TKnowException>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TKnowException : Exception
            where TService : class
        {
            return services.AddSingleton(sp =>
            {
                TService returnValue = default;

                BuildPolicy<TKnowException>().Execute(() => { returnValue = implementationFactory(sp); });

                return returnValue;

            });
        }

        public static IServiceCollection AddScopedWithRetry<TService, TKnowException>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
           where TKnowException : Exception
           where TService : class
        {
            return services.AddScoped(sp =>
            {
                TService returnValue = default;

                BuildPolicy<TKnowException>().Execute(() => { returnValue = implementationFactory(sp); });

                return returnValue;

            });
        }


        private static RetryPolicy BuildPolicy<TKnowException>(int retryCount = 5) where TKnowException : Exception
        {
            return Policy
                .Handle<TKnowException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );
        }
    }
}
