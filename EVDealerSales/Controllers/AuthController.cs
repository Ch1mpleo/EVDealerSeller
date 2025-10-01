using EVDealerSales.BO.DTOs.AuthDTOs;
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
        public async Task<IActionResult> Login(LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginDto);
            }

            try
            {
                var response = await _authService.LoginAsync(loginDto, _configuration);
                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View("Login", loginDto);
                }

                _logger.LogInformation($"User {loginDto.Email} logged in successfully.");

                // Store token in session for later use
                HttpContext.Session.SetString("AuthToken", response.Token);

                // Always go to LandingPage
                return RedirectToAction("LandingPage", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error for {loginDto.Email}: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Login", loginDto);
            }
        }



        [HttpGet]
        public IActionResult RegisterPage()
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
                return RedirectToAction("LoginPage");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(registrationDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AuthToken");
            return RedirectToAction("LandingPage", "Home");
        }

    }
}
