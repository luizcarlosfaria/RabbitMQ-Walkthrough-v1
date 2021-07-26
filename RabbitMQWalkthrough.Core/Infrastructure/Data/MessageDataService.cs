using Dapper;
using RabbitMQWalkthrough.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Infrastructure.Data
{
    public class MessageDataService
    {

        public MessageDataService()
        {
            
        }

        public Message CreateMessage(SqlTransaction transaction, SqlConnection sqlConnection)
        {
            string sql = @"
                            INSERT INTO [dbo].[Messages] ([Stored],[Num])  VALUES (GETUTCDATE(),0); 
                            
                            SELECT 
                                [MessageId], 
                                [Stored], 
                                [Processed], 
                                [Num] 
                            FROM
                                [dbo].[Messages] 
                            WHERE 
                                MessageId = SCOPE_IDENTITY();
                        ";
            Message message = sqlConnection.QuerySingle<Message>(sql, null, transaction);
            return message;
        }

        internal void MarkAsProcessed(Message message, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {
            string sql = @"UPDATE [dbo].[Messages] 
                            SET 
                                [Processed] = GETUTCDATE(), 
                                [Num] = [Num]+1 
                            WHERE 
                                [MessageId] = @MessageId;";
            
            sqlConnection.Execute(sql, message, sqlTransaction);

        }
    }
}
