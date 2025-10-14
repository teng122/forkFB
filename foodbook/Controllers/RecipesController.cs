using Microsoft.AspNetCore.Mvc;

namespace foodbook.Controllers
{
    public class RecipesController : Controller
    {
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}


