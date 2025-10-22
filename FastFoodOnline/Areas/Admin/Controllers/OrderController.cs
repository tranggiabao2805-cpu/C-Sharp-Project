using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _db;

        public OrderController(FastFoodDbContext db)
        {
            _db = db;
        }

        public IActionResult AllOrders(OrderStatus? status)
        {
            // Đếm tổng số đơn hàng
            ViewBag.OrderCount = _db.Orders.Count();

            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(i => i.FoodItem)
                .Include(o => o.OrderDetails)
                    .ThenInclude(i => i.Combo)   // ✅ thêm Include Combo
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return View(query.ToList());
        }

        [HttpPost]
        public IActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            var order = _db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _db.SaveChanges();
            }
            return RedirectToAction("AllOrders");
        }
    }
}
