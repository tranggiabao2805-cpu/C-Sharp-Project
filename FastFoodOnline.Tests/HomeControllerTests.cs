using FastFoodOnline.Controllers;
using FastFoodOnline.Data;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private FastFoodDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            // ✅ Dùng InMemoryDatabase cho mỗi test
            var options = new DbContextOptionsBuilder<FastFoodDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new FastFoodDbContext(options);

            // ✅ UserStore hỗ trợ IdentityUser<Guid>
            var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, FastFoodDbContext, Guid>(_context);

            _userManager = new UserManager<ApplicationUser>(
                userStore,
                null,
                new PasswordHasher<ApplicationUser>(),
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                null, null, null, null
            );

            _loggerMock = new Mock<ILogger<HomeController>>();

            _controller = new HomeController(_loggerMock.Object, _context, _userManager);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnCorrectCountsInViewBag()
        {
            // Arrange
            await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "user1",
                Email = "user1@mail.com",
                FullName = "User One"
            });
            await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "user2",
                Email = "user2@mail.com",
                FullName = "User Two"
            });

            _context.FoodItems.AddRange(
                new FoodItem { Name = "Burger", Description = "Tasty", ImageUrl = "a.jpg", Price = 50 },
                new FoodItem { Name = "Pizza", Description = "Cheesy", ImageUrl = "b.jpg", Price = 100 }
            );

            _context.Combos.Add(new Combo
            {
                Name = "Combo 1",
                Description = "Test combo",
                Price = 120
            });

            _context.Orders.Add(new Order
            {
                UserId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            });

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.ViewData["UserCount"]);
            Assert.AreEqual(2, result.ViewData["FoodCount"]);
            Assert.AreEqual(1, result.ViewData["ComboCount"]);
            Assert.AreEqual(1, result.ViewData["OrderCount"]);
        }

        [Test]
        public void Privacy_ShouldReturnView()
        {
            var result = _controller.Privacy();
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Error_ShouldReturnErrorViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.IsNotNull(model);
            Assert.IsFalse(string.IsNullOrEmpty(model.RequestId));
        }
    }
}
