using EVDealerSales.Models.DTOs.AuthDTOs;
using EVDealerSales.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.WebMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPage(LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginPage", loginDto);
            }

            try
            {
                var response = await _authService.LoginAsync(loginDto, _configuration);
                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View("LoginPage", loginDto);
                }
                _logger.LogInformation($"User {loginDto.Email} logged in successfully.");

                // Lưu token vào session hoặc cookie nếu cần
                //HttpContext.Session.SetString("AuthToken", response.Token);

                return RedirectToAction("Employees", "Manager");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error for {loginDto.Email}: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("LoginPage", loginDto);
            }
        }

        [HttpGet]
        public IActionResult RegisterPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPage(UserRegistrationDto registrationDto)
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
                return RedirectToAction("LoginPage");
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
