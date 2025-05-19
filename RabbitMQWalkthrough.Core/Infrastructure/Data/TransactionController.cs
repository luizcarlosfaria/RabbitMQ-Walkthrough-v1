using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Data
{
    public class TransactionController
    {
        private readonly ILogger<TransactionController> logger;
        private readonly IServiceProvider serviceProvider;

        public TransactionController(ILogger<TransactionController> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public void RunUnderTransaction(Action<IServiceProvider> action)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            
            using SqlTransaction transaction = scope.ServiceProvider.GetRequiredService<SqlTransaction>();
            try
            {
                action(scope.ServiceProvider);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                this.logger.LogError(ex, "Erro ao publicar mensagem. Transação com banco foi abortada.");
                
                throw;
            }
        }
    }
}
