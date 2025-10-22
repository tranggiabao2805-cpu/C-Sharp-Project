using FastFoodOnline.Data;
using FastFoodOnline.Enums;
using FastFoodOnline.Models;
using FastFoodOnline.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ComboController : Controller
    {
        private readonly FastFoodDbContext _db;
        public ComboController(FastFoodDbContext db) => _db = db;

        // GET: Admin/Combo
        public async Task<IActionResult> Index()
            => View(await _db.Combos
                            .AsNoTracking()
                            .ToListAsync());

        // GET: Admin/Combo/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var combo = await _db.Combos
                .AsNoTracking()
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.Id == id);

            return combo == null ? NotFound() : View(combo);
        }
        public IActionResult Create()
        {
            var foodItems = _db.FoodItems
                .Where(f => !f.IsDeleted && f.Status == ItemStatus.Available)
                .ToList();

            var vm = new ComboCreateViewModel
            {
                Items = foodItems.Select(f => new ComboFoodItem
                {
                    FoodItemId = f.Id,
                    Name = f.Name,
                    Quantity = 1
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ComboCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var combo = new Combo
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                ImageUrl = vm.ImageUrl,
                Status = vm.Status
            };

            _db.Combos.Add(combo);
            _db.SaveChanges();

            var selectedItems = vm.Items
                .Where(i => i.Selected && i.Quantity > 0)
                .ToList();

            foreach (var item in selectedItems)
            {
                _db.ComboItems.Add(new ComboItem
                {
                    ComboId = combo.Id,
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity
                });
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Combo/Edit/5
        public IActionResult Edit(int id)
        {
            var combo = _db.Combos
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefault(c => c.Id == id);

            if (combo == null) return NotFound();

            var selectedIds = combo.ComboItems.Select(ci => ci.FoodItemId).ToList();

            var vm = new ComboCreateViewModel
            {
                Id = combo.Id,
                Name = combo.Name,
                Description = combo.Description,
                Price = combo.Price,
                ImageUrl = combo.ImageUrl,
                Status = combo.Status,
                Items = _db.FoodItems
                    .Where(f => !f.IsDeleted && f.Status == ItemStatus.Available)
                    .AsEnumerable() // Chuyển về LINQ to Objects
                    .Select(fi => new ComboFoodItem
                    {
                        FoodItemId = fi.Id,
                        Name = fi.Name,
                        Selected = selectedIds.Contains(fi.Id),
                        Quantity = combo.ComboItems.FirstOrDefault(ci => ci.FoodItemId == fi.Id)?.Quantity ?? 1
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComboCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var existingCombo = _db.Combos
                .Include(c => c.ComboItems)
                .FirstOrDefault(c => c.Id == vm.Id);

            if (existingCombo == null) return NotFound();

            // Cập nhật thông tin combo
            existingCombo.Name = vm.Name;
            existingCombo.Description = vm.Description;
            existingCombo.Price = vm.Price;
            existingCombo.ImageUrl = vm.ImageUrl;
            existingCombo.Status = vm.Status;

            // Cập nhật món ăn trong combo
            existingCombo.ComboItems.Clear();
            var selectedItems = vm.Items.Where(i => i.Selected && i.Quantity > 0).ToList();

            foreach (var item in selectedItems)
            {
                existingCombo.ComboItems.Add(new ComboItem
                {
                    ComboId = existingCombo.Id,
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity
                });
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Combo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var combo = await _db.Combos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return combo == null ? NotFound() : View(combo);
        }

        // POST: Admin/Combo/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _db.Combos.FindAsync(id);
            _db.Combos.Remove(combo);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
