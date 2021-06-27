using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQWalkthrough.Core.Architecture;
using RabbitMQWalkthrough.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationEntrypoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackController : ControllerBase
    {
        public void Post([FromServices]IModel model)
        {
            var message = new Message()
            {
                Created = DateTime.Now
            };

            model.ReliablePublish(message, "test_exchange", string.Empty);
        }

    }
}
