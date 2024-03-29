﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;
using RESTFUL.Interfaces;
using RESTFUL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RESTFUL.Controllers
{
    [ApiController]
    [Route("[controller]")] // could use "items" also
    public class ItemsController : ControllerBase
    {
        // use dependency injection instead of creating new instances each time the endpoints are hit
        private readonly IItemsRepository _repository;

        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDTO>> GetItems()
        {
            var items = (await _repository.GetItemsAsync())
                .Select(item => item.AsDTO());

            _logger.LogInformation($"Found {items.Count()} items.");

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
    }
}
