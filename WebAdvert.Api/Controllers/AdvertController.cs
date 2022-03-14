using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
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

        public AdvertController(IAdvertStorageService advertStorageService)
        {
            _advertStorageService = advertStorageService;
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

        [HttpPost]
        [Route("Confirm")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm([FromBody] ConfirmAdvertModel model)
        {
            try
            {
                 await _advertStorageService.Confirm(model);
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
    }
}
