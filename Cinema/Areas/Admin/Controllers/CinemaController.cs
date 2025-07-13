
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        //private readonly ApplicationDbContext _context;
        private ICinemaRepository _cinemaRepository;
        public CinemaController(ICinemaRepository cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }
        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaRepository.GetAsync();
            return View(cinemas);
        }
        public IActionResult Create()
        {
            return View(new Models.Cinema());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Models.Cinema cinema, IFormFile cinemaLogo)
        {
            ModelState.Remove("CinemaLogo");

            if (cinemaLogo == null || cinemaLogo.Length == 0)
            {
                ModelState.AddModelError("cinemaLogo", "Please upload a cinema logo.");
            }

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            // Fix for CS8602: Ensure cinemaLogo is not null before accessing FileName
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cinemaLogo?.FileName ?? string.Empty);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await cinemaLogo.CopyToAsync(stream);
            }

            cinema.CinemaLogo = fileName;

            await _cinemaRepository.CreateAsync(cinema);

            TempData["success-notification"] = "Cinema created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.Cinema cinema, IFormFile cinemaLogo)
        {
            //var existingCinema = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == cinema.Id);
            var existingCinema = await _cinemaRepository.GetOneAsync(c => c.Id == cinema.Id,tracked:false);
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
                cinema.CinemaLogo = existingCinema.CinemaLogo;
            }

            ModelState.Remove("CinemaLogo"); 

            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

          await  _cinemaRepository.UpdateAsync(cinema);
            TempData["success-notification"] = "Cinema updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);

            if (cinema is not null)
            {
               
               await _cinemaRepository.DeleteAsync(cinema);
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
