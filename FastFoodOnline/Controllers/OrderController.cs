using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFoodOnline.Data;
using FastFoodOnline.Models;
using System.Security.Claims;

namespace FastFoodOnline.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _db;

        public OrderController(FastFoodDbContext db)
        {
            _db = db;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
        }

        // ✅ Đặt hàng (giả sử từ giỏ hàng)
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(List<OrderDetail> orderDetails)
        {
            if (orderDetails == null || !orderDetails.Any())
                return BadRequest("Không có món nào để đặt.");

            var userId = GetUserId();

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.Now,
                OrderDetails = orderDetails,
                TotalPrice = orderDetails.Sum(od => od.Quantity * od.UnitPrice)
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = order.Id });
        }

        // ✅ Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FoodItem)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Combo)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && order.UserId != userId)
                return Forbid();

            return View(order);
        }

        // ✅ Người dùng: xem đơn hàng của mình
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            var orders = _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.FoodItem)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Combo)
                .ToList();

            return View(orders);
        }

        // ✅ Admin: xem tất cả đơn hàng
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ✅ Admin: cập nhật trạng thái đơn hàng
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
