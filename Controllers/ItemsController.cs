using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;
using RESTFUL.Repositories;
using RESTFUL.Interfaces;
using RESTFUL.DTOs;
using RESTFUL;
using Microsoft.AspNetCore.Mvc;

namespace RESTFUL.Controllers
{
    [ApiController]
    [Route("[controller]")] // could use "items" also
    public class ItemsController : ControllerBase
    {
        // use dependency injection instead of creating new instances each time the endpoints are hit
        private readonly IInMemoryItemsRepository _repository;
        public ItemsController(IInMemoryItemsRepository repository)
        {
            _repository = repository;
        }

        // GET /items
        [HttpGet]
        public IEnumerable<ItemDTO> GetItems()
        {
            return _repository.GetItems().Select(item => item.AsDTO());
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        // return http response or the Item type
        public ActionResult<ItemDTO> GetItem(Guid id)
        {
            var item = _repository.GetItem(id);

            if (item == null)
            {
                return NotFound();
            }

            return item.AsDTO();
        }

        // POST /items
        [HttpPost]
        public ActionResult<ItemDTO> CreateItem(CreateItemDTO item)
        {
            Item newItem = new()
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Price = item.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            _repository.CreateItem(newItem);

            return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, newItem.AsDTO());
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateItem(Guid id, UpdateItemDTO item)
        {
            var toUpdate = _repository.GetItem(id);

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

            _repository.UpdateItem(updatedItem);

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteItem(Guid id)
        {
            var item = _repository.GetItem(id);

            if (item == null)
            {
                return NotFound();
            }

            _repository.DeleteItem(id);

            return NoContent();
        }
    }
}
