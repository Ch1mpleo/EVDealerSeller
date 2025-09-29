using Microsoft.AspNetCore.Mvc;

namespace EVDealerSales.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult LandingPage()
        {
            return View();
        }
    }
}
