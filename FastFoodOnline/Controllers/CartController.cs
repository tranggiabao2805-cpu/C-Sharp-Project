using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FastFoodOnline.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly FastFoodDbContext _db;

        public CartController(FastFoodDbContext db)
        {
            _db = db;
        }

        private string GetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.FoodItem)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Combo)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            ViewBag.Total = cart?.Items.Sum(i => i.Price * i.Quantity) ?? 0;
            return View(cart?.Items.ToList() ?? new List<CartItem>());
        }

        public async Task<IActionResult> Add(int foodItemId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(); 

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var food = await _db.FoodItems.FindAsync(foodItemId);
            if (food == null) return NotFound();

            var item = cart.Items.FirstOrDefault(i => i.FoodItemId == foodItemId);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    FoodItemId = foodItemId,
                    Quantity = 1,
                    Price = food.Price
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> AddCombo(int comboId)
        {
            var userId = GetUserId();
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var combo = await _db.Combos.FindAsync(comboId);
            if (combo == null) return NotFound();

            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ComboId = combo.Id,
                Quantity = 1,
                Price = combo.Price
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm combo vào giỏ hàng!";
            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var item = await _db.CartItems.FindAsync(id);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            var item = await _db.CartItems.FindAsync(id);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _db.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _db.CartItems.RemoveRange(cart.Items);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.FoodItem)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Combo)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index");

            // ✅ Tạo đơn hàng trước
            var order = new Order
            {
                UserId = Guid.Parse(userId),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>()
            };

            // ✅ Thêm OrderDetail có liên kết trực tiếp với Order
            foreach (var item in cart.Items)
            {
                var detail = new OrderDetail
                {
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Note = "",
                    OrderDate = DateTimeOffset.UtcNow,
                    Order = order // liên kết tới Order cha
                };

                if (item.FoodItemId.HasValue)
                    detail.FoodItemId = item.FoodItemId.Value;
                else if (item.ComboId.HasValue)
                    detail.ComboId = item.ComboId.Value;

                order.OrderDetails.Add(detail);
            }

            // ✅ Tính tổng tiền
            order.TotalPrice = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

            // ✅ Lưu Order + OrderDetails
            _db.Orders.Add(order);

            // ✅ Xóa giỏ hàng
            _db.CartItems.RemoveRange(cart.Items);

            await _db.SaveChangesAsync();

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }
    }
}