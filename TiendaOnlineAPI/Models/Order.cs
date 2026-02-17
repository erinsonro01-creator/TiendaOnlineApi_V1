using System;
using TiendaOnlineAPI.Models.Enums;
namespace TiendaOnlineAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<OrderItem> Items { get; set; }
    }

}

