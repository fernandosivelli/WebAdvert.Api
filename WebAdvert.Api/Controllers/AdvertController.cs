using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebAdvert.Api.Domain.Messages;
using WebAdvert.Api.Domain.Models;
using WebAdvert.Api.Models;
using WebAdvert.Api.Services;

namespace WebAdvert.Api.Controllers
{
    [ApiController]
    [Route("adverts/v1")]
    public class AdvertController : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;
        private readonly IConfiguration _configuration;

        public AdvertController(IAdvertStorageService advertStorageService, IConfiguration configuration)
        {
            _advertStorageService = advertStorageService;
            _configuration = configuration;
        }

        [HttpGet]
        public string Get()
        {
            return $"Hello World from inside a Lambda {DateTime.Now}";
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(CreateAdvertResponse))]
        public async Task<IActionResult> Create([FromBody] AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await _advertStorageService.Add(model);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return StatusCode(201, new CreateAdvertResponse() { Id = recordId });
        }

        [HttpPut]
        [Route("Confirm")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm([FromBody] ConfirmAdvertModel model)
        {
            try
            {
                 await _advertStorageService.Confirm(model);
                 await RaiseAdvertConfirmedMessage(model);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return new OkResult();
        }

        private async Task RaiseAdvertConfirmedMessage(ConfirmAdvertModel model)
        {
            var topicArn = _configuration.GetValue<string>("WebAdvertApiSnsArn");

            var advertModel = await _advertStorageService.GetById(model.Id);

            using var client = new AmazonSimpleNotificationServiceClient(RegionEndpoint.EUWest1);

            var message = new AdvertConfirmedMessageModel()
            {
                Id = model.Id,
                Title = advertModel.Title
            };

            var messageJson = JsonConvert.SerializeObject(message);

            var publishRequest = new PublishRequest(topicArn, messageJson);

            await client.PublishAsync(publishRequest);
        }
    }
}
