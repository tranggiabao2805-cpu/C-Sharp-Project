using FastFoodOnline.Models;
using FastFoodOnline.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FastFoodOnline.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager)
        {
            _signIn = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(vm);

            var result = await _signIn.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                HttpContext.Response.Cookies.Delete("GuestId");
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Tài khoản của bạn tạm thời bị khóa.");
            else
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");

            return View(vm);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/Account")
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var existingUser = await _userManager.FindByEmailAsync(vm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                DateOfBirth = vm.DateOfBirth
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signIn.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Cập nhật thông tin cá nhân
            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Address = vm.Address;
            user.DateOfBirth = vm.DateOfBirth;

            // Cập nhật email nếu thay đổi
            if (user.Email != vm.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, vm.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError("Email", error.Description);
                    return View(vm);
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, vm.Email);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                        ModelState.AddModelError("Email", error.Description);
                    return View(vm);
                }
            }

            // Kiểm tra và đổi mật khẩu nếu có nhập
            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                // Kiểm tra mật khẩu hiện tại có nhập không
                if (string.IsNullOrEmpty(vm.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại.");
                    return View(vm);
                }

                // Kiểm tra xác nhận mật khẩu
                if (vm.NewPassword != vm.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                    return View(vm);
                }

                // Kiểm tra mật khẩu hiện tại có đúng không
                var checkPassword = await _userManager.CheckPasswordAsync(user, vm.CurrentPassword);
                if (!checkPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(vm);
                }

                // Thực hiện đổi mật khẩu
                var passwordResult = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("NewPassword", error.Description);
                    }
                    return View(vm);
                }

                // Làm mới đăng nhập để cập nhật thông tin bảo mật
                await _signIn.RefreshSignInAsync(user);
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["Success"] = "Thông tin đã được cập nhật.";
                return RedirectToAction("EditUser");
            }

            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }
    }
}