using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{ 
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _Context = new();
        
        public IActionResult Index()
        {
            var Movies = _Context.Movies.Include(e=>e.Cinema).Include(e=>e.Category);

            return View(Movies.ToList());
        }
    }
}
