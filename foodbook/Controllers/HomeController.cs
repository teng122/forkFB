using foodbook.Attributes;
using foodbook.Models;
using foodbook.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace foodbook.Controllers
{
    [LoginRequired]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly SupabaseService _supabaseService;

            

        public HomeController(ILogger<HomeController> logger, SupabaseService supabaseService)
        {
            _logger = logger;
            _supabaseService = supabaseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddRecipe()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddRecipe(AddRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Xử lý lưu recipe vào database
            // Chuyển đổi IFormFile thành byte[] nếu cần
            // Lưu vào Supabase

            TempData["Success"] = "Đã thêm công thức thành công!";
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       

        public async Task<IActionResult> Newsfeed()
        {
            try
            {
                var recipes = await _supabaseService.GetNewsfeedRecipesAsync();
                return View(recipes);
            }
            catch (Exception ex)
            {
                // Log the error (e.g., using ILogger)
                Console.WriteLine($"Error fetching newsfeed recipes: {ex.Message}");
                // Return an empty list of RecipeViewModel to the view
                return View(new List<NewfeedViewModel>());
            }
        }
    }
}
