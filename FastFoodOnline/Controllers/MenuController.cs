using FastFoodOnline.Data;
using FastFoodOnline.Enums;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFoodOnline.Controllers
{
    public class MenuController : Controller
    {
        private readonly FastFoodDbContext _db;

        public MenuController(FastFoodDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Food(string search)
        {
            var query = _db.FoodItems
                .Where(f => f.Status == ItemStatus.Available && !f.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            var items = await query.ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Combos()
        {
            var combos = await _db.Combos
                .Where(c => c.Status == Combo.ComboStatus.Active)
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.FoodItem)
                .ToListAsync();

            return View(combos);
        }

        public async Task<IActionResult> ComboDetails(int id)
        {
            var combo = await _db.Combos
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.Id == id);

            return combo == null ? NotFound() : View(combo);
        }

        public async Task<IActionResult> FoodDetails(int id)
        {
            var food = await _db.FoodItems
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (food == null)
                return NotFound();

            return View(food);
        }
    }

}
