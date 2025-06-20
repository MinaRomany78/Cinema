using Azure;
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context = new();
        public IActionResult Index(int page = 1)
        {
            decimal totalNumberOfProductInPages = 9; // You can change this value to set the number of products per page
            var actors = _context.Actors.ToList();
            var totalNumberOfPages = Math.Ceiling(actors.Count() / totalNumberOfProductInPages);
            ViewBag.totalNumberOfPages = totalNumberOfPages;
            if (totalNumberOfPages < page || page < 1)
            {

                ViewBag.currentPage = 1;
                ViewBag.totalNumberOfPages = 1;
                return View(model: actors);


            }
            actors = actors.Skip((page - 1) * (int)totalNumberOfProductInPages).Take((int)totalNumberOfProductInPages).ToList();
            ViewBag.currentPage = page;
            return View(actors);
        }
        public IActionResult Details(int id)
        {
            var actor = _context.Actors
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null) return NotFound();

            return View(actor);
        }

    }
}
