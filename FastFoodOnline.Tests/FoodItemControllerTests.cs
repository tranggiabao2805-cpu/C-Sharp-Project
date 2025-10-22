using FastFoodOnline.Controllers;
using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class FoodItemControllerTests
    {
        private FastFoodDbContext _context;
        private FoodItemController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastFoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new FastFoodDbContext(options);
            _controller = new FoodItemController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // 🔹 Test: Index trả về danh sách
        [Test]
        public async Task Index_ShouldReturnListOfFoodItems()
        {
            // Arrange
            var category = new Category { Name = "Fast Food" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _context.FoodItems.AddRange(
                new FoodItem { Name = "Burger", Description = "Beef burger", ImageUrl = "burger.jpg", Price = 50, CategoryId = category.Id },
                new FoodItem { Name = "Pizza", Description = "Cheese pizza", ImageUrl = "pizza.jpg", Price = 100, CategoryId = category.Id }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index(null, null, null, null) as ViewResult;
            var model = result?.Model as List<FoodItem>;

            // Assert
            Assert.IsNotNull(result, "Expected a ViewResult but got null");
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(2, model.Count, "Expected 2 items but found a different count");
        }

        [Test]
        public async Task Details_ValidId_ShouldReturnFoodItem()
        {
            var category = new Category { Name = "Snacks" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var item = new FoodItem
            {
                Name = "Fries",
                Price = 30,
                Description = "Crispy golden fries",
                ImageUrl = "/images/fries.jpg",
                CategoryId = category.Id
            };

            _context.FoodItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(item.Id) as ViewResult;
            var model = result?.Model as FoodItem;

            // Assert
            Assert.IsNotNull(result, "Expected a ViewResult, but got null or NotFound.");
            Assert.IsNotNull(model, "Model should not be null for valid item.");
            Assert.AreEqual("Fries", model.Name);
        }

        // 🔹 Test: Details với ID không tồn tại
        [Test]
        public async Task Details_InvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.Details(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // 🔹 Test: Create với model hợp lệ
        [Test]
        public async Task Create_ValidModel_ShouldRedirectToIndex()
        {
            var item = new FoodItem
            {
                Name = "Soda",
                Description = "Refreshing drink",
                ImageUrl = "soda.jpg",
                Price = 15
            };

            var result = await _controller.Create(item) as RedirectToActionResult;

            Assert.AreEqual("Index", result?.ActionName);
            Assert.AreEqual(1, await _context.FoodItems.CountAsync());
        }

        // 🔹 Test: Create với model không hợp lệ
        [Test]
        public async Task Create_InvalidModel_ShouldReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var item = new FoodItem();

            var result = await _controller.Create(item) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        // 🔹 Test: Edit hợp lệ
        [Test]
        public async Task Edit_ValidModel_ShouldUpdateFoodItem()
        {
            var item = new FoodItem
            {
                Name = "Nuggets",
                Description = "Chicken pieces",
                ImageUrl = "nuggets.jpg",
                Price = 40
            };
            _context.FoodItems.Add(item);
            await _context.SaveChangesAsync();

            item.Name = "Chicken Nuggets";
            var result = await _controller.Edit(item.Id, item) as RedirectToActionResult;

            Assert.AreEqual("Index", result?.ActionName);
            var updated = await _context.FoodItems.FindAsync(item.Id);
            Assert.AreEqual("Chicken Nuggets", updated.Name);
        }

        // 🔹 Test: Edit với ID không trùng khớp
        [Test]
        public async Task Edit_IdMismatch_ShouldReturnNotFound()
        {
            var item = new FoodItem { Id = 1, Name = "Burger", Description = "Test", ImageUrl = "b.jpg", Price = 10 };
            var result = await _controller.Edit(2, item);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // 🔹 Test: DeleteConfirmed
        [Test]
        public async Task DeleteConfirmed_ShouldRemoveFoodItem()
        {
            var item = new FoodItem { Name = "Ice Cream", Description = "Cold dessert", ImageUrl = "ice.jpg", Price = 25 };
            _context.FoodItems.Add(item);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteConfirmed(item.Id) as RedirectToActionResult;

            Assert.AreEqual("Index", result?.ActionName);
            Assert.IsNull(await _context.FoodItems.FindAsync(item.Id));
        }

        // 🔹 Test: Delete với ID không hợp lệ
        [Test]
        public async Task Delete_InvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.Delete(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
