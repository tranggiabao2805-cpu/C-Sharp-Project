using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFoodOnline.Data;
using FastFoodOnline.Models;

namespace FastFoodOnline.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly FastFoodDbContext _context;

        public FoodItemController(FastFoodDbContext context)
        {
            _context = context;
        }

        // GET: FoodItems
        public async Task<IActionResult> Index(string keyword, int? categoryId, decimal? min, decimal? max)
        {
            var query = _context.FoodItems
                .AsNoTracking()
                .Include(f => f.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(f => f.Name.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(f => f.CategoryId == categoryId.Value);

            if (min.HasValue)
                query = query.Where(f => f.Price >= min.Value);

            if (max.HasValue)
                query = query.Where(f => f.Price <= max.Value);

            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .AsNoTracking()
                    .ToListAsync(),
                "Id", "Name", categoryId
            );

            ViewBag.Keyword = keyword;
            ViewBag.Min = min;
            ViewBag.Max = max;

            var items = await query.ToListAsync();
            return View(items);
        }

        // GET: FoodItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var foodItem = await _context.FoodItems
                .AsNoTracking()
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (foodItem == null) return NotFound();

            return View(foodItem);
        }

        // GET: FoodItems/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                _context.FoodItems.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadCategoriesAsync(foodItem.CategoryId);
            return View(foodItem);
        }

        // GET: FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null) return NotFound();

            await LoadCategoriesAsync(foodItem.CategoryId);
            return View(foodItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem foodItem)
        {
            if (id != foodItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foodItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodItemExists(foodItem.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadCategoriesAsync(foodItem.CategoryId);
            return View(foodItem);
        }

        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var foodItem = await _context.FoodItems
                .AsNoTracking()
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (foodItem == null) return NotFound();

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
                _context.FoodItems.Remove(foodItem);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.Id == id);
        }

        private async Task LoadCategoriesAsync(object selected = null)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .AsNoTracking()
                    .ToListAsync(),
                "Id", "Name", selected
            );
        }
    }
}
