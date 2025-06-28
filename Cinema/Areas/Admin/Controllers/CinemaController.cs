using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var cinemas = _context.Cinemas.ToList();
            return View(cinemas);
        }
        public IActionResult Create()
        {
            return View(new Models.Cinema());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Models.Cinema cinema,IFormFile cinemaLogo)
        {
            if (cinemaLogo is not null &&cinemaLogo.Length>0)
            {
                var fileName=Guid.NewGuid().ToString() + Path.GetExtension(cinemaLogo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images", fileName);
                using(var stream=System.IO.File.Create(filePath))
                {
                   await cinemaLogo.CopyToAsync(stream);
                }
                cinema.CinemaLogo = fileName;
                _context.SaveChanges();


            }
            ModelState.Remove("CinemaLogo"); // Remove the CinemaLogo from ModelState to avoid validation errors
            if (ModelState.IsValid)
            {
                _context.Cinemas.Add(cinema);
                _context.SaveChanges();
                TempData["success-notification"] = "Cinema created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }
        public IActionResult Edit(int id)
        {
            var cinema = _context.Cinemas.Find(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(Models.Cinema cinema, IFormFile cinemaLogo)
        {
            var existingCinema = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == cinema.Id);
            if (existingCinema is null)
            {
                return NotFound();
            }

            if (cinemaLogo is not null && cinemaLogo.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(cinemaLogo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await cinemaLogo.CopyToAsync(stream);
                }

                // حذف الصورة القديمة إن وجدت
                if (!string.IsNullOrEmpty(existingCinema.CinemaLogo))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images", existingCinema.CinemaLogo);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                cinema.CinemaLogo = fileName;
            }
            else
            {
                cinema.CinemaLogo = existingCinema.CinemaLogo; // احتفظ بالصورة القديمة
            }

            ModelState.Remove("CinemaLogo"); // مهم جدًا

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            _context.Cinemas.Update(cinema);
            await _context.SaveChangesAsync();
            TempData["success-notification"] = "Cinema updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas.Find(id);

            if (cinema is not null)
            {
               
                _context.Cinemas.Remove(cinema);
                    _context.SaveChanges();
                 var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images", cinema.CinemaLogo);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                TempData["success-notification"] = "Cinema deleted successfully!";
                    return RedirectToAction(nameof(Index));
                
            }
            return NotFound();
            

        }
    }
}
