using EVDealerSales.Models.DTOs.AuthDTOs;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", loginDto);
            }

            try
            {
                var response = await _authService.LoginAsync(loginDto, _configuration);
                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View("Login", loginDto);
                }

                // Lưu token vào session hoặc cookie nếu cần
                //HttpContext.Session.SetString("AuthToken", response.Token);

                return RedirectToAction("LandingPage", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", loginDto);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registrationDto);
            }

            try
            {
                var user = await _authService.RegisterUserAsync(registrationDto);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Registration failed.");
                    return View(registrationDto);
                }

                // Đăng ký thành công, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(registrationDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout(Guid userId)
        {
            await _authService.LogoutAsync(userId);
            // Xóa token nếu lưu ở session/cookie
            //HttpContext.Session.Remove("AuthToken");
            return RedirectToAction("LandingPage");
        }
    }
}
