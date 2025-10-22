using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FastFoodOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly FastFoodDbContext _db;

        public CategoryController(FastFoodDbContext db)
        {
            _db = db;
        }

        // 📋 Hiển thị danh sách loại
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                                      .AsNoTracking()
                                      .ToListAsync();
            return View(categories);
        }

        // ➕ Hiển thị form thêm loại
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Xử lý thêm loại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.Categories.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✏️ Hiển thị form sửa loại
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ Xử lý cập nhật loại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _db.Categories.AnyAsync(c => c.Id == model.Id);
            if (!exists)
                return NotFound();

            _db.Categories.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 🗑 Hiển thị xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // ✅ Xử lý xóa loại
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}