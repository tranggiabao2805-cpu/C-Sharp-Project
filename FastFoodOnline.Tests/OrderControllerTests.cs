using FastFoodOnline.Controllers;
using FastFoodOnline.Data;
using FastFoodOnline.Enums;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class OrderControllerTests
    {
        private FastFoodDbContext _context;
        private OrderController _controller;
        private Guid _userId;
        private ClaimsPrincipal _user;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastFoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new FastFoodDbContext(options);

            _userId = Guid.NewGuid();
            _user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _controller = new OrderController(_context);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            SeedData();
        }

        private void SeedData()
        {
            var food = new FoodItem
            {
                Id = 1,
                Name = "Burger",
                Description = "Beef burger",
                ImageUrl = "burger.jpg",
                Status = ItemStatus.Available,
                IsDeleted = false
            };
            _context.FoodItems.Add(food);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        // ✅ Test 1: PlaceOrder() thêm đơn hàng thành công
        [Test]
        public async Task PlaceOrder_Valid_ShouldCreateOrder()
        {
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { FoodItemId = 1, Quantity = 2, UnitPrice = 50 }
            };

            var result = await _controller.PlaceOrder(orderDetails) as RedirectToActionResult;

            var order = _context.Orders.Include(o => o.OrderDetails).FirstOrDefault();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _context.Orders.Count());
            Assert.AreEqual(100, order.TotalPrice);
            Assert.AreEqual("Details", result.ActionName);
        }

        // ✅ Test 2: PlaceOrder() không có món -> BadRequest
        [Test]
        public async Task PlaceOrder_Empty_ShouldReturnBadRequest()
        {
            var result = await _controller.PlaceOrder(new List<OrderDetail>());
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // ✅ Test 3: MyOrders() trả đúng đơn của user hiện tại
        [Test]
        public async Task MyOrders_ShouldReturnOrdersOfCurrentUser()
        {
            _context.Orders.Add(new Order
            {
                UserId = _userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.Now,
                TotalPrice = 100
            });
            _context.SaveChanges();

            var result = await _controller.MyOrders() as ViewResult;
            var orders = result?.Model as List<Order>;

            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count);
            Assert.AreEqual(_userId, orders[0].UserId);
        }

        // ✅ Test 4: Details() sai id -> NotFound
        [Test]
        public async Task Details_InvalidId_ShouldReturnNotFound()
        {
            var result = await _controller.Details(99);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // ✅ Test 5: Details() trả đúng View khi đơn thuộc user
        [Test]
        public async Task Details_ValidId_ShouldReturnView()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "user1@example.com",
                Email = "user1@example.com",
                FullName = "Test User"
            };

            _context.Users.Add(user);

            var order = new Order
            {
                Id = 1,
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>() // cần để tránh null khi Include
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            var claimsUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsUser }
            };

            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Expected a ViewResult, but got null (maybe NotFound or Forbid).");
            Assert.IsInstanceOf<Order>(result.Model);
            var model = (Order)result.Model;
            Assert.AreEqual(order.Id, model.Id);
        }

        // ✅ Test 6: UpdateStatus() cập nhật trạng thái (Admin)
        [Test]
        public async Task UpdateStatus_Admin_ShouldChangeStatus()
        {
            var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser }
            };

            var order = new Order
            {
                Id = 5,
                UserId = _userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.Now
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var result = await _controller.UpdateStatus(5, OrderStatus.Delivered) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(OrderStatus.Delivered, _context.Orders.Find(5).Status);
        }
    }
}
