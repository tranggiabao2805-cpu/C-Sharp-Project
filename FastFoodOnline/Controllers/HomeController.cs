using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FastFoodOnline.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FastFoodDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(
            ILogger<HomeController> logger,
            FastFoodDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userCount = await _userManager.Users.CountAsync();
            var foodCount = await _context.FoodItems
                .CountAsync(f => !f.IsDeleted);
            var comboCount = await _context.Combos.CountAsync();
            var orderCount = await _context.Orders.CountAsync(); // 👈 Đếm tất cả đơn hàng

            ViewBag.UserCount = userCount;
            ViewBag.FoodCount = foodCount;
            ViewBag.ComboCount = comboCount;
            ViewBag.OrderCount = orderCount;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
