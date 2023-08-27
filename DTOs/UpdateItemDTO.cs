using System;
using System.ComponentModel.DataAnnotations;

namespace RESTFUL.DTOs
{
    public record UpdateItemDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(1, 10000)]
        public decimal Price { get; set; }
    }
}
