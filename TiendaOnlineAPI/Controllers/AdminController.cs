using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnlineAPI.Data;
using TiendaOnlineAPI.DTOs;
using TiendaOnlineAPI.Models.Enums;

namespace TiendaOnlineAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Ordenes por estado
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var paidOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Paid);
            var shippedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipped);
            var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

            // Ventas totales (solo pagadas o enviadas)
            var totalSales = await _context.Orders
                .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped)
                .SumAsync(o => o.TotalAmount);

            // Productos
            var totalProducts = await _context.Products.CountAsync();
            var lowStockProducts = await _context.Products.CountAsync(p => p.Stock <= 5 && p.IsActive);

            // Usuarios
            var totalUsers = await _context.Users.CountAsync();

            // Últimas órdenes (5 más recientes)
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.TotalAmount,
                    Status = o.Status.ToString(),
                    UserEmail = o.User.Email
                })
                .ToListAsync();

            // Últimos usuarios registrados (5 más recientes)
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .ToListAsync();

            var dashboard = new
            {
                Orders = new
                {
                    Pending = pendingOrders,
                    Paid = paidOrders,
                    Shipped = shippedOrders,
                    Cancelled = cancelledOrders,
                    TotalSales = totalSales
                },
                Products = new
                {
                    Total = totalProducts,
                    LowStock = lowStockProducts
                },
                Users = new
                {
                    Total = totalUsers
                },
                RecentOrders = recentOrders,
                RecentUsers = recentUsers
            };

            return Ok(dashboard);
        }
    }

    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminOrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/orders?status=Paid
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.TotalAmount,
                    Status = o.Status.ToString(),
                    UserEmail = o.User.Email,
                    Items = o.Items.Select(i => new
                    {
                        i.ProductId,
                        ProductName = i.Product.Name,
                        i.Quantity,
                        i.Price
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.TotalAmount,
                Status = o.Status.ToString(),
                UserEmail = o.User.Email,
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.Price
                })
            });

            return Ok(result);
        }

        // POST: api/admin/orders/{id}/ship
        [HttpPost("{id}/ship")]
        public async Task<IActionResult> ShipOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound("Orden no encontrada");

            if (order.Status != OrderStatus.Paid)
                return BadRequest("Solo órdenes pagadas pueden enviarse");

            order.Status = OrderStatus.Shipped;
            await _context.SaveChangesAsync();

            return Ok("Orden enviada correctamente");
        }

        // POST: api/admin/orders/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Orden no encontrada");

            if (order.Status == OrderStatus.Cancelled)
                return BadRequest("La orden ya está cancelada");

            // Devolver stock
            foreach (var item in order.Items)
            {
                item.Product.Stock += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok("Orden cancelada por el administrador");
        }
    }
}
