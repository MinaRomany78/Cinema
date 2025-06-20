using Cinema.Data;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Customer.Controllers
{
    
         [Area("Customer")]
    public class CategoriesController : Controller
    {
        
        private readonly ApplicationDbContext _context=new();
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();

            return View(categories);
        }
    }
}
