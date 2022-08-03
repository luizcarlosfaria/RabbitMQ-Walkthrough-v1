using Dapper;
using Npgsql;
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


        public Message CreateMessage(NpgsqlTransaction transaction, NpgsqlConnection sqlConnection)
        {
            string sql = @"
                INSERT INTO app.""Messages"" 
                    (""Stored"",""Num"") 
                VALUES 
                    (now(),0) 
                RETURNING *; 
                        ";
            Message message = sqlConnection.QuerySingle<Message>(sql, null, transaction);
            return message;
        }

        public void MarkAsProcessed(Message message, NpgsqlConnection sqlConnection, NpgsqlTransaction sqlTransaction)
        {
            string sql = @"UPDATE app.""Messages""
                            SET 
                                ""Processed"" = now(), 
                                ""TimeSpent"" = now() - ""Stored"",
                                ""Num"" = ""Num"" + 1 
                            WHERE 
                                ""MessageId"" = @MessageId ; ";
            
            sqlConnection.Execute(sql, message, sqlTransaction);

        }
    }
}
