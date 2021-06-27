using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQWalkthrough.Core.Queue;
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
        public void AddConsumer()
        {
            consumerManager.AddConsumer();
        }

        [HttpDelete]
        public void RemoveConsumer()
        {
            consumerManager.RemoveConsumer();
        }
    }
}
