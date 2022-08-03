using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RabbitMQ.Client;
using RabbitMQWalkthrough.Core.Infrastructure;
using RabbitMQ.Client.Exceptions;
using RabbitMQWalkthrough.Core.Infrastructure.Metrics;
using RabbitMQWalkthrough.Core.Infrastructure.Metrics.Collectors;
using RabbitMQWalkthrough.Core.Infrastructure.Queue;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationEntrypoint.Workers;
using System.Data.Common;
using RabbitMQWalkthrough.Core.Infrastructure.Data;
using RestSharp.Authenticators;
using RestSharp;
using Npgsql;
using Microsoft.AspNetCore.Hosting.Server;

namespace WebApplicationEntrypoint
{

    /// <remarks>
    /// As connectionstrings não estão, intencionalmente em arquivos de configuração.
    /// São diversos componentes que dependem da exata mesma conexão, portanto 
    /// trocar a senha do Banco, fará o grafana parar.
    /// Trocar a senha do RabbitMQ fará o coletor de métricas parar.
    /// </remarks>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Facilitador: Passe o host a ser usado quando executado com docker-compose, caso esteja no windows, retornará localhost.
        /// </summary>
        /// <param name="defaultHost"></param>
        /// <returns></returns>
        private static string GetHost(string defaultHost) => System.Environment.OSVersion.Platform == PlatformID.Unix ? defaultHost : "localhost";



        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri($"amqp://WalkthroughUser:WalkthroughPassword@{GetHost("rabbitmq")}/Walkthrough"),
                DispatchConsumersAsync = false,
                ConsumerDispatchConcurrency = 1,
                UseBackgroundThreadsForIO = false
            });


            //TODO: Atenção
            services.AddTransientWithRetry<IConnection, BrokerUnreachableException>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());

            //TODO: Atenção
            services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());


            //TODO: Atenção
            services.AddTransientWithRetry<NpgsqlConnection, NpgsqlException>(sp =>
            {
                NpgsqlConnection connection = new ($"Server={GetHost("postgres")};Port=5432;Database=Walkthrough;User Id=WalkthroughUser;Password=WalkthroughPass;");
                connection.Open();
                return connection;
            });


            services.AddSingleton<ConsumerManager>();
            services.AddSingleton<PublisherManager>();
            services.AddSingleton<MetricsService>();
            services.AddSingleton<RestClient>(sp => new($"http://{GetHost("rabbitmq")}:15672/api")
            {
                Authenticator = new HttpBasicAuthenticator("WalkthroughUser", "WalkthroughPassword")
            });

            services.AddSingleton<IMetricCollector, QueueMetricCollector>();
            services.AddSingleton<IMetricCollector, PublisherMetricCollector>();
            services.AddSingleton<IMetricCollector, ConsumerMetricCollector>();
            services.AddSingleton<IMetricCollector, CollectorFixer>();
            services.AddHostedService<MetricsWorker>();


            services.AddSingleton<MessageDataService>();

            //TODO: Atenção Precisa chamar Initialize()
            services.AddTransient<Publisher>();

            //TODO: Atenção Precisa chamar Initialize()
            services.AddTransient<Consumer>();

            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitRabbitMQ(app.ApplicationServices);

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

        private static void InitRabbitMQ(IServiceProvider applicationServices)
        {
            using var rabbitMQChannel = applicationServices.GetRequiredService<IModel>();

            rabbitMQChannel.QueueDeclare("test_queue", true, false, false);
            rabbitMQChannel.ExchangeDeclare("test_exchange", "fanout", true, false);
            rabbitMQChannel.QueueBind("test_queue", "test_exchange", string.Empty);
        }
    }
}
