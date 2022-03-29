using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using WebAdvert.Api.Domain.Enums;
using WebAdvert.Api.Domain.Models;
using WebAdvert.Api.Models;

namespace WebAdvert.Api.Services
{
    public class AdvertStorageService : IAdvertStorageService
    {
        private readonly IMapper _mapper;

        public AdvertStorageService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map(model, new AdvertDbModel());
            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatusEnum.Pending;

            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);
            await context.SaveAsync(dbModel);

            return dbModel.Id;
        }

        public async Task Confirm(ConfirmAdvertModel model)
        {
            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);

            var record = await context.LoadAsync<AdvertDbModel>(model.Id);

            if (record == null)
                throw new KeyNotFoundException($"A record with ID={model.Id} was not found.");

            if (model.Status == AdvertStatusEnum.Active)
            {
                record.Status = model.Status;
                await context.SaveAsync(record);
            }
        }

        public async Task<AdvertModel> GetById(string id)
        {
            using var client = new AmazonDynamoDBClient();
            using var context = new DynamoDBContext(client);

            var record = await context.LoadAsync<AdvertDbModel>(id);

            if (record == null)
                throw new KeyNotFoundException($"A record with ID={id} was not found.");

            var model = new AdvertModel()
            {
                Id = record.Id,
                Title = record.Title,
                Description = record.Description,
                Price = record.Price
            };

            return model;
        }

        public async Task<bool> CheckHealth()
        {
            using var client = new AmazonDynamoDBClient();
            var describeTableResponse = await client.DescribeTableAsync("Adverts");
            return string.Compare(describeTableResponse.Table.TableStatus, "active", true) == 0;
        }
    }
}
