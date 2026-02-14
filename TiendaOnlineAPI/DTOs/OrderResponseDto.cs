using System;
namespace TiendaOnlineAPI.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public List<OrderItemResponseDto> Items { get; set; }
    }

}

