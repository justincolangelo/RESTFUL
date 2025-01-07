using RESTFUL.DTOs;
using RESTFUL.Entities;
using RESTFUL.Interfaces;
using RESTFUL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace RESTFUL.Controllers
{
    [ApiController]
    [Route("[controller]")] // could use "items" also
    public class ItemsController : ControllerBase
    {
        // use dependency injection instead of creating new instances each time the endpoints are hit
        private readonly IItemsRepository _repository;

        private readonly ILogger<ItemsController> _logger;

        private readonly ServiceBusService _serviceBus;

        public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger, ServiceBusService serviceBus)
        {
            _repository = repository;
            _logger = logger;
            _serviceBus = serviceBus;
        }

        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDTO>> GetItems()
        {
            var items = (await _repository.GetItemsAsync())
                .Select(item => item.AsDTO());

            var msg = $"Found {items.Count()} items.";

            _logger.LogInformation(msg);

            await _serviceBus.PublishMessageToDefaultQueue(msg);

            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        // return http response or the Item type
        public async Task<ActionResult<ItemDTO>> GetItem(Guid id)
        {
            var item = await _repository.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item.AsDTO();
        }

        // POST /items
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItem(CreateItemDTO item)
        {
            Item newItem = new()
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Price = item.Price,
                CreatedDate = DateTime.UtcNow
            };

            await _repository.CreateItemAsync(newItem);

            return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, newItem.AsDTO());
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItem(Guid id, UpdateItemDTO item)
        {
            var toUpdate = await _repository.GetItemAsync(id);

            if (toUpdate == null)
            {
                return NotFound();
            }

            // record types allow us to use "with" creates a copy of it with specified properties modified
            Item updatedItem = toUpdate with
            {
                Name = item.Name,
                Price = item.Price
            };

            await _repository.UpdateItemAsync(updatedItem);

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            var item = await _repository.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await _repository.DeleteItemAsync(id);

            return NoContent();
        }

        // GET /items/messages
        [HttpGet("messages")]
        public async Task<List<string>> GetItemsMessages()
        {
            var messages = await _serviceBus.ConsumeMessageFromDefaultQueue();

            return messages;
        }
    }
}
