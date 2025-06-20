using Cinema.Data;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context= new();
        public IActionResult Index()
        {
            var Cinemas=_context.Cinemas;

            return View(Cinemas);
        }
    }
}
