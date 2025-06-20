using Azure;
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context = new();
        double totalNumberOfProductInPages = 8;
        public IActionResult Index(string? MoviesName, int? CategoryId,int? CinemaId,int page=1)
        {
            var movies = _context.Movies.Include(m => m.Cinema)
                                        .Include(m => m.Category)
                                        .ToList();

            if (!string.IsNullOrEmpty(MoviesName))
            {
                movies = movies.Where(m => m.Name.Contains(MoviesName)).ToList();
            }
            if (CategoryId > 0)
            {
                movies = movies.Where(m => m.CategoryId == CategoryId).ToList();
            }
            ViewBag.Categories = _context.Categories.ToList();
            if (CinemaId > 0)
            {
                movies = movies.Where(m => m.CinemaId == CinemaId).ToList();
            }


            var totalNumberOfPages = Math.Ceiling(movies.Count() / totalNumberOfProductInPages);
            ViewBag.totalNumberOfPages = totalNumberOfPages;
            if (totalNumberOfPages < page || page < 1)
            {

                ViewBag.currentPage = 1;
                ViewBag.totalNumberOfPages = 1;
                return View(model: movies);


            }
            movies = movies.Skip((page - 1) * (int)totalNumberOfProductInPages).Take((int)totalNumberOfProductInPages).ToList();
            ViewBag.currentPage = page;

            
            return View(movies);
        }

        public IActionResult Details(int id)
        {
            var movie = _context.Movies
                .Include(m => m.Cinema)
                .Include(m => m.Category)
                .Include(m => m.ActorMovies)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }


    }
}
