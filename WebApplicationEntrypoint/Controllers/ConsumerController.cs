using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQWalkthrough.Core.Infrastructure.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationEntrypoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        private readonly ConsumerManager consumerManager;

        public ConsumerController(ConsumerManager consumerManager)
        {
            this.consumerManager = consumerManager;
        }

        [HttpPut]
        public async Task AddConsumerAsync([FromQuery] int size, [FromQuery] int messagesPerSecond)
        {
            await this.consumerManager.AddConsumerAsync(size, messagesPerSecond);
        }

       

        [HttpDelete("{id}")]
        public void RemoveConsumer(string id)
        {
            this.consumerManager.RemoveConsumer(id);
        }

        [HttpGet()]
        public IEnumerable<Consumer> GetConsumer()
        {
            return this.consumerManager.Consumers;
        }
    }
}
