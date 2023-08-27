using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTFUL.Entities
{
    // immutable record type
    public record Item
    {
        // init is used when a value is only allowed to be set during initialization
        // set would allow it to be changed at any time
        public Guid Id { get; init; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}
