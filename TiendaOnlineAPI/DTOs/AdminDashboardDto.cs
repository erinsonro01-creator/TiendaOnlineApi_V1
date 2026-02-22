namespace TiendaOnlineAPI.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int CancelledOrders { get; set; }

        public decimal TotalSales { get; set; }

        public int TotalUsers { get; set; }

        public List<ProductStockDto> LowStockProducts { get; set; } = new List<ProductStockDto>();
    }

    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
