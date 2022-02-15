using Microsoft.AspNetCore.Mvc;

namespace User_Management.Areas.User.Controllers
{
    [Area("User")]
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
