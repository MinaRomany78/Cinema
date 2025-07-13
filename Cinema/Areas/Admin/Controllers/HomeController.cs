using Cinema.Data;
using Cinema.Data.Enums;
using Cinema.Utilty.DBInitializer;
using Cinema.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =$"{SD.SuperAdmin},{SD.Admin},{SD.Campany},{SD.Employee}")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context = new();
        public IActionResult Index()
        {
            var countMovies = _context.Movies.Count();
            var countCinema = _context.Cinemas.Count();
            var countCategory = _context.Categories.Count();
            var countActors = _context.Actors.Count();

            CountsModelVM count = new()
            {
                CountMovies = countMovies,
                CountCinema = countCinema,
                CountActors = countActors,
                CountCategories = countCategory



            };
            var moviesPerCategory = _context.Categories
                                    .Select(c => new
                                    {
                                        c.Name,
                                        Count = c.Movies.Count()
                                    }).ToList();

            ViewBag.CategoryLabels = moviesPerCategory.Select(c => c.Name).ToList();
            ViewBag.CategoryCounts = moviesPerCategory.Select(c => c.Count).ToList();
            ViewBag.CategoryCounts = moviesPerCategory.Select(c => c.Count).ToList();
            var totalMovies = _context.Movies.Count();

var upcomingCount = _context.Movies.Count(m => m.MovieStatus == MovieStatus.Upcoming);
var availableCount = _context.Movies.Count(m => m.MovieStatus == MovieStatus.Available);
var expiredCount = _context.Movies.Count(m => m.MovieStatus == MovieStatus.Expired);

ViewBag.UpcomingPercent = totalMovies > 0 ? (upcomingCount * 100) / totalMovies : 0;
ViewBag.AvailablePercent = totalMovies > 0 ? (availableCount * 100) / totalMovies : 0;
ViewBag.ExpiredPercent = totalMovies > 0 ? (expiredCount * 100) / totalMovies : 0;

            return View(count);
        }
    }
}
