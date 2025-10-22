using FastFoodOnline.Controllers;
using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class CartControllerTests
    {
        private FastFoodDbContext _context;
        private CartController _controller;
        private string _userId;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastFoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new FastFoodDbContext(options);
            _userId = Guid.NewGuid().ToString();

            _controller = new CartController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new[] { new Claim(ClaimTypes.NameIdentifier, _userId) },
                                "TestAuth"
                            )
                        )
                    }
                }
            };
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        [Test]
        public async Task Index_WhenCartEmpty_ShouldReturnEmptyList()
        {
            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as List<CartItem>;

            Assert.That(result, Is.Not.Null);
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(0));
            Assert.That(_controller.ViewBag.Total, Is.EqualTo(0));
        }

        [Test]
        public async Task Add_NewFoodItem_ShouldAddToCart()
        {
            // Arrange: tạo FoodItem đầy đủ dữ liệu required
            var food = new FoodItem
            {
                Id = 1,
                Name = "Burger",
                Description = "Tasty burger",
                ImageUrl = "burger.jpg",
                Price = 50
            };
            _context.FoodItems.Add(food);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Add(food.Id) as RedirectToActionResult;

            // Assert
            Assert.That(result?.ActionName, Is.EqualTo("Index"));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == _userId);

            Assert.That(cart, Is.Not.Null);
            Assert.That(cart.Items.Count, Is.EqualTo(1));
            var item = cart.Items.First();
            Assert.That(item.FoodItemId, Is.EqualTo(food.Id));
            Assert.That(item.Quantity, Is.EqualTo(1));
            Assert.That(item.Price, Is.EqualTo(food.Price));
        }

        [Test]
        public async Task Add_SameItem_IncreasesQuantity()
        {
            var food = new FoodItem
            {
                Id = 2,
                Name = "Pizza",
                Description = "Cheese pizza",
                ImageUrl = "pizza.jpg",
                Price = 100
            };
            _context.FoodItems.Add(food);
            await _context.SaveChangesAsync();

            await _controller.Add(food.Id);
            await _controller.Add(food.Id);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == _userId);

            Assert.That(cart.Items.First().Quantity, Is.EqualTo(2));
        }

        [Test]
        public async Task Update_QuantityZero_ShouldRemoveItem()
        {
            var food = new FoodItem
            {
                Id = 3,
                Name = "Fries",
                Description = "Crispy fries",
                ImageUrl = "fries.jpg",
                Price = 30
            };
            _context.FoodItems.Add(food);

            var cart = new Cart
            {
                UserId = _userId,
                Items = new List<CartItem>
                {
                    new CartItem { FoodItemId = food.Id, Quantity = 2, Price = food.Price }
                }
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var itemId = cart.Items.First().Id;
            var result = await _controller.Update(itemId, 0) as RedirectToActionResult;

            Assert.That(result?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.CartItems.FindAsync(itemId), Is.Null);
        }

        [Test]
        public async Task Remove_Item_ShouldRemoveFromCart()
        {
            var food = new FoodItem
            {
                Id = 4,
                Name = "Soda",
                Description = "Refreshing soda",
                ImageUrl = "soda.jpg",
                Price = 10
            };
            _context.FoodItems.Add(food);

            var cart = new Cart
            {
                UserId = _userId,
                Items = new List<CartItem>
                {
                    new CartItem { FoodItemId = food.Id, Quantity = 1, Price = food.Price }
                }
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var itemId = cart.Items.First().Id;
            var result = await _controller.Remove(itemId) as RedirectToActionResult;

            Assert.That(result?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.CartItems.FindAsync(itemId), Is.Null);
        }

        [Test]
        public async Task Clear_ShouldRemoveAllItems()
        {
            var cart = new Cart
            {
                UserId = _userId,
                Items = new List<CartItem>
                {
                    new CartItem { FoodItemId = 1, Quantity = 1, Price = 10 },
                    new CartItem { FoodItemId = 2, Quantity = 2, Price = 20 }
                }
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _controller.Clear() as RedirectToActionResult;

            Assert.That(result?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.CartItems.CountAsync(), Is.EqualTo(0));
        }
    }
}
