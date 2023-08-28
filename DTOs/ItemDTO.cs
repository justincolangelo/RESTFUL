using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFUL.DTOs
{
    public record ItemDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
