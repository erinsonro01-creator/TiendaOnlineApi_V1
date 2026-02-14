using System;
namespace TiendaOnlineAPI.DTOs
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}

