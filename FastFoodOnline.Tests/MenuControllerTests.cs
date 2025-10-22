using FastFoodOnline.Controllers;
using FastFoodOnline.Data;
using FastFoodOnline.Enums;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class MenuControllerTests
    {
        private FastFoodDbContext _context;
        private MenuController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastFoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ✅ tạo database mới mỗi test
                .Options;

            _context = new FastFoodDbContext(options);
            _controller = new MenuController(_context);

            SeedData();
        }

        private void SeedData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var foods = new List<FoodItem>
            {
                new FoodItem { Id = 1, Name = "Burger", Description = "Juicy beef burger", ImageUrl = "burger.jpg", Status = ItemStatus.Available, IsDeleted = false },
                new FoodItem { Id = 2, Name = "Pizza", Description = "Cheesy pizza", ImageUrl = "pizza.jpg", Status = ItemStatus.Available, IsDeleted = false },
                new FoodItem { Id = 3, Name = "Old Food", Description = "Expired", ImageUrl = "old.jpg", Status = ItemStatus.Unavailable, IsDeleted = false }
            };

            _context.FoodItems.AddRange(foods);

            var combo = new Combo
            {
                Id = 1,
                Name = "Combo 1",
                Description = "Test combo",
                Price = 100,
                Status = Combo.ComboStatus.Active,
                ComboItems = new List<ComboItem>
                {
                    new ComboItem { FoodItemId = 2 } // chỉ liên kết tới Pizza
                }
            };

            _context.Combos.Add(combo);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        // ---------------------------------------------------
        // ✅ Test 1: Food() không có search -> trả về 2 món
        // ---------------------------------------------------
        [Test]
        public async Task Food_NoSearch_ShouldReturnAllAvailableFoods()
        {
            var result = await _controller.Food(null) as ViewResult;
            var model = result?.Model as List<FoodItem>;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, model.Count);
        }

        // ---------------------------------------------------
        // ✅ Test 2: Food() có search -> lọc đúng kết quả
        // ---------------------------------------------------
        [Test]
        public async Task Food_WithSearch_ShouldReturnFilteredFoods()
        {
            var result = await _controller.Food("Pizza") as ViewResult;
            var model = result?.Model as List<FoodItem>;

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Pizza", model.First().Name);
        }

        // ---------------------------------------------------
        // ✅ Test 3: Combos() -> trả về danh sách combo
        // ---------------------------------------------------
        [Test]
        public async Task Combos_ShouldReturnActiveCombos()
        {
            var result = await _controller.Combos() as ViewResult;
            var model = result?.Model as List<Combo>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Combo 1", model[0].Name);
        }

        // ---------------------------------------------------
        // ✅ Test 4: ComboDetails() hợp lệ -> trả về View
        // ---------------------------------------------------
        [Test]
        public async Task ComboDetails_ValidId_ShouldReturnComboView()
        {
            var result = await _controller.ComboDetails(1) as ViewResult;
            var combo = result?.Model as Combo;

            Assert.IsNotNull(combo);
            Assert.AreEqual("Combo 1", combo.Name);
        }

        // ---------------------------------------------------
        // ✅ Test 5: ComboDetails() sai ID -> NotFound()
        // ---------------------------------------------------
        [Test]
        public async Task ComboDetails_InvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.ComboDetails(99);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // ---------------------------------------------------
        // ✅ Test 6: FoodDetails() hợp lệ
        // ---------------------------------------------------
        [Test]
        public async Task FoodDetails_ValidId_ShouldReturnView()
        {
            var result = await _controller.FoodDetails(1) as ViewResult;
            var item = result?.Model as FoodItem;

            Assert.IsNotNull(item);
            Assert.AreEqual("Burger", item.Name);
        }

        // ---------------------------------------------------
        // ✅ Test 7: FoodDetails() sai ID
        // ---------------------------------------------------
        [Test]
        public async Task FoodDetails_InvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.FoodDetails(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
