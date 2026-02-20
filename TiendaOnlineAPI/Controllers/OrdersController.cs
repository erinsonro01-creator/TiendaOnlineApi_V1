using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnlineAPI.Data;
using TiendaOnlineAPI.DTOs;
using TiendaOnlineAPI.Models;
using TiendaOnlineAPI.Models.Enums;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Carrito vacío");

            decimal total = 0;

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem>()
            };


            foreach (var item in cart.Items)
            {
                if (item.Product.Stock < item.Quantity)
                    return BadRequest($"No hay suficiente stock para {item.Product.Name}");

                item.Product.Stock -= item.Quantity;
                total += item.Product.Price * item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            order.TotalAmount = total;

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new OrderResponseDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            return Ok(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict("El producto fue modificado por otro usuario. Intenta nuevamente.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error en el proceso de checkout");
        }

    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("all")]
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


    [Authorize(Roles = "Customer,Admin")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
            return NotFound("Orden no encontrada");

        if (order.Status != OrderStatus.Pending)
            return BadRequest("Solo órdenes pendientes pueden cancelarse");

        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            product.Stock += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();

        return Ok("Orden cancelada correctamente");
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayOrder(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Orden no encontrada");

            if (order.Status != OrderStatus.Pending)
                return BadRequest("La orden no está pendiente de pago");

            // 🔹 Simulación de pago exitoso
            var paymentSuccess = true;

            if (!paymentSuccess)
            {
                foreach (var item in order.Items)
                {
                    item.Product.Stock += item.Quantity;
                }

                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return BadRequest("Pago fallido");
            }

            order.Status = OrderStatus.Paid;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Pago realizado con éxito");
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error procesando el pago");
        }
    }

    [Authorize(Roles = "Admin")]
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


}