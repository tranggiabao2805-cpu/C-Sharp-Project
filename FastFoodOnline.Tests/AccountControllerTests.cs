using FastFoodOnline.Controllers;
using FastFoodOnline.Models;
using FastFoodOnline.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace FastFoodOnline.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // Mock SignInManager dependencies
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null
            );

            // Tạo controller với HttpContext thật
            _controller = new AccountController(_signInManagerMock.Object, _userManagerMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // ---------- TESTS ----------

        [Test]
        public async Task Logout_ShouldRedirectToHome()
        {
            _signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            var result = await _controller.Logout() as RedirectToActionResult;

            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
            Assert.That(result?.ActionName, Is.EqualTo("Index"));
            Assert.That(result?.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Register_InvalidModel_ReturnsView()
        {
            var vm = new RegisterViewModel();
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.Register(vm);

            Assert.IsInstanceOf<ViewResult>(result);
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Register_ValidUser_Success()
        {
            var vm = new RegisterViewModel
            {
                Email = "a@a.com",
                Password = "Abcd1234@",
                ConfirmPassword = "Abcd1234@"
            };

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password))
                            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                            .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(vm);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"), Times.Once);
        }

        [Test]
        public async Task Register_DuplicateEmail_ShouldFail()
        {
            var vm = new RegisterViewModel
            {
                Email = "exists@example.com",
                Password = "Abcd1234@",
                ConfirmPassword = "Abcd1234@"
            };

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError
                            {
                                Code = "DuplicateEmail",
                                Description = "Email already exists"
                            }));

            var result = await _controller.Register(vm);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsFalse(_controller.ModelState.IsValid);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Login_ValidUser_RedirectsHome()
        {
            var vm = new LoginViewModel
            {
                Email = "a@a.com",
                Password = "Abcd1234@",
                RememberMe = true
            };

            // ⚠️ lockoutOnFailure = true → phải match đúng với controller
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, true))
                              .ReturnsAsync(SignInResult.Success);

            var result = await _controller.Login(vm);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect?.ActionName, Is.EqualTo("Index"));
            Assert.That(redirect?.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_InvalidPassword_ReturnsView()
        {
            var vm = new LoginViewModel { Email = "a@a.com", Password = "Wrong" };

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, true))
                              .ReturnsAsync(SignInResult.Failed);

            var result = await _controller.Login(vm);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }
    }
}
