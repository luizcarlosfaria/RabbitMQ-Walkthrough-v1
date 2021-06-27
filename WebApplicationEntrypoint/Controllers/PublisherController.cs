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
    public class PublisherController : ControllerBase
    {
        private readonly PublisherManager publisherManager;

        public PublisherController(PublisherManager publisherManager)
        {
            this.publisherManager = publisherManager;
        }

        [HttpPut]
        public void AddPublisher()
        {
            publisherManager.AddPublisher();
        }

        [HttpDelete]
        public void RemovePublisher()
        {
            publisherManager.RemovePublisher();
        }
    }
}
