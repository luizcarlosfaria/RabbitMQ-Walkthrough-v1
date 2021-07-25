using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Model
{
    public class Message
    {
        public string MessageId { get; set; }


        /// <summary>
        /// Quando foi processada
        /// </summary>
        public DateTimeOffset Processed { get; set; }


        /// <summary>
        /// Data do banco
        /// </summary>
        public DateTimeOffset Stored { get; set; }

        
        public int TimeSpent { get; set; }


    }
}
