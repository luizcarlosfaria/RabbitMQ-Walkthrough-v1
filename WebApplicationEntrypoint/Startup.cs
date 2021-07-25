using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQWalkthrough.Core;
using RabbitMQWalkthrough.Core.Architecture;
using RabbitMQWalkthrough.Core.Metrics;
using RabbitMQWalkthrough.Core.Metrics.Collectors;
using RabbitMQWalkthrough.Core.Queue;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationEntrypoint.Workers;

namespace WebApplicationEntrypoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton(sp => new ConnectionFactory() {
                Uri = new Uri("amqp://WalkthroughUser:WalkthroughPassword@rabbitmq/Walkthrough"),
                DispatchConsumersAsync = false,
                ConsumerDispatchConcurrency = 1,
                //UseBackgroundThreadsForIO = true
            });

            services.AddSingleton(sp =>
            {
                IConnection connection = null;

                var policy = Policy
                    .Handle<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

                policy.Execute(() =>
                {
                    connection = sp.GetRequiredService<ConnectionFactory>().CreateConnection();

                });

                return connection;

            });



            services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

            services.AddSingleton<ConsumerManager>();
            services.AddSingleton<PublisherManager>();
            services.AddSingleton<MetricsService>();
            services.AddSingleton<IMetricCollector, QueueMetricCollector>();
            services.AddSingleton<IMetricCollector, PublisherMetricCollector>();
            services.AddSingleton<IMetricCollector, ConsumerMetricCollector>();
            //services.AddHostedService<MetricsWorker>();


            services.AddSingleton(sp => {

                SqlConnection connection = null;
                
                var policy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

                policy.Execute(() =>
                {
                    connection = new SqlConnection("Server=sql,1433;Database=Walkthrough;User Id=WalkthroughUser;Password=WalkthroughPass");
                    connection.Open();
                });

                return connection;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitRabbitMQ(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitRabbitMQ(IApplicationBuilder app)
        {
            using var rabbitMQChannel = app.ApplicationServices.GetRequiredService<IModel>();

            rabbitMQChannel.QueueDeclare("test_queue", true, false, false);
            rabbitMQChannel.ExchangeDeclare("test_exchange", "fanout", true, false);
            rabbitMQChannel.QueueBind("test_queue", "test_exchange", string.Empty);
        }
    }
}
