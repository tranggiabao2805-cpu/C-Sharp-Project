using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace YourApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FoodItemController : Controller
    {
        private readonly FastFoodDbContext _context;

        public FoodItemController(FastFoodDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var foodItems = _context.FoodItems
                .Include(f => f.Category)
                .Where(f => !f.IsDeleted) // ✅ Chỉ lấy món chưa bị xóa
                .ToList();

            return View(foodItems);
        }

        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                _context.FoodItems.Add(foodItem);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
            return View(foodItem);
        }

        public IActionResult Edit(int id)
        {
            var foodItem = _context.FoodItems.Find(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
            return View(foodItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                _context.FoodItems.Update(foodItem);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", foodItem.CategoryId);
            return View(foodItem);
        }
        public IActionResult Delete(int id)
        {
            var foodItem = _context.FoodItems
                .Include(f => f.Category)
                .FirstOrDefault(f => f.Id == id);

            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var foodItem = _context.FoodItems.Find(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            foodItem.IsDeleted = true; // ✅ Soft delete
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}