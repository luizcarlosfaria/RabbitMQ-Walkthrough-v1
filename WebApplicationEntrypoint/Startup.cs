using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQWalkthrough.Core;
using RabbitMQWalkthrough.Core.Architecture;
using RabbitMQWalkthrough.Core.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                Uri = new Uri("amqp://WalkthroughUser:WalkthroughPassword@localhost/Walkthrough"), 
                DispatchConsumersAsync = false, 
                ConsumerDispatchConcurrency = 10,
                UseBackgroundThreadsForIO = true
            });

            services.AddTransient(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());

            services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

            services.AddSingleton<ConsumerManager>();
            services.AddSingleton<PublisherManager>();

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
