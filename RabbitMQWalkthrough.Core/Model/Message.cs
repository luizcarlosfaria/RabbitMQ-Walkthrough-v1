using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQWalkthrough.Core.Model
{
    public class Message
    {
        /// <summary>
        /// Quando foi criada na API
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Quando foi processada
        /// </summary>
        public DateTime Processed { get; set; }


        /// <summary>
        /// Data do banco
        /// </summary>
        public DateTime Stored { get; set; }

        
        public TimeSpan TimeSpentInQueue { get; set; }
        public TimeSpan TimeSpentProcessing { get; set; }
        public TimeSpan TimeSpent { get; set; }


    }
}
