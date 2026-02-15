using System;
using System.ComponentModel.DataAnnotations;
namespace TiendaOnlineAPI.Models
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public int Stock { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }


        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
		public Category Category { get; set; }
	}
}

