using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESTFUL.Entities
{
    // Schema was needed with Postgres otherwise "relation not found" errors thrown
    [Table("items", Schema = "RESTFUL")]
    public record Item
    {
        // init is used when a value is only allowed to be set during initialization
        // set would allow it to be changed at any time
        public Guid Id { get; init; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
