using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize(Roles ="admin")]
    public class AdminController : Controller
    { 
        public IActionResult Index()
        {
            return View();
        }
    }
}
