using System;
using System.ComponentModel.DataAnnotations;

namespace TiendaOnlineAPI.DTOs
{
	public class ProductCreateDto
	{
		[Required]

        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}

