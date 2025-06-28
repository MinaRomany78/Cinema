using Cinema.Data;
using Cinema.Data.Enums;
using Cinema.Models;
using Cinema.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _Context = new();

        public IActionResult Index()
        {
            var Movies = _Context.Movies.Include(e => e.Cinema).Include(e => e.Category);

            return View(Movies.ToList());
        }
        public IActionResult Create()
        {
            var Cinemas = _Context.Cinemas.ToList();
            var Categories = _Context.Categories.ToList();
            var Actors = _Context .Actors.ToList();
            CinemawithCategoryVm vm = new()
            {
                Cinemas = Cinemas,
                Categories = Categories,
                ActorList = Actors,
                Movie = new Movie()
            };


            return View(vm);
        }
        [HttpPost]


        public async Task<IActionResult> Create(CinemawithCategoryVm vm, IFormFile ImgUrl)
        {
            ModelState.Remove("ImgUrl");
            ModelState.Remove("Cinema");
            ModelState.Remove("Category");
            ModelState.Remove("Categories");
            ModelState.Remove("Cinemas");
            ModelState.Remove("ActorList");
            ModelState.Remove("Movie.Cinema");
            ModelState.Remove("Movie.Category");

            // تحقق من رفع الصورة
            if (ImgUrl == null || ImgUrl.Length == 0)
            {
                ModelState.AddModelError(nameof(ImgUrl), "Please upload a movie image.");
            }

            // التحقق من التواريخ
            if (vm.Movie.EndDate < vm.Movie.StartDate)
            {
                ModelState.AddModelError(nameof(vm.Movie.EndDate), "End date must be after the start date.");
            }

            if (ModelState.IsValid)
            {
                // حفظ الصورة
                var fileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\movies", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await ImgUrl.CopyToAsync(stream);
                }
                vm.Movie.ImgUrl = fileName;

                vm.Movie.MovieStatus = check(vm.Movie.StartDate, vm.Movie.EndDate);

                _Context.Movies.Add(vm.Movie);
                await _Context.SaveChangesAsync();

                // ربط الممثلين
                if (vm.SelectedActorIds != null && vm.SelectedActorIds.Any())
                {
                    foreach (var actorId in vm.SelectedActorIds)
                    {
                        _Context.ActorMovies.Add(new ActorMovie
                        {
                            MovieId = vm.Movie.Id,
                            ActorId = actorId
                        });
                    }
                    await _Context.SaveChangesAsync();
                }

                TempData["success-notification"] = "Movie added successfully.";
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل البيانات في حال وجود خطأ
            vm.Cinemas = _Context.Cinemas.ToList();
            vm.Categories = _Context.Categories.ToList();
            vm.ActorList = _Context.Actors.ToList();

            return View(vm);
        }



        public MovieStatus check(DateTime StartDate, DateTime EndDate)
        {


            var currentDate = DateTime.Now;
            if (currentDate < StartDate)
                return MovieStatus.Upcoming;
            else if (currentDate >= StartDate && currentDate <= EndDate)
                return MovieStatus.Available;
            else
                return MovieStatus.Expired;

        }
        public IActionResult Edit(int id)
        {
            var movie = _Context.Movies
                .Include(e => e.Cinema)
                .Include(e => e.Category)
                .Include(e => e.ActorMovies) 
                .FirstOrDefault(e => e.Id == id);

            if (movie is null)
                return NotFound();

            var Cinemas = _Context.Cinemas.ToList();
            var Categories = _Context.Categories.ToList();
            var Actors = _Context.Actors.ToList();

            CinemawithCategoryVm vm = new()
            {
                Cinemas = Cinemas,
                Categories = Categories,
                ActorList = Actors,
                Movie = movie,
                SelectedActorIds = movie.ActorMovies.Select(ma => ma.ActorId).ToList() 
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CinemawithCategoryVm vm, IFormFile ImgUrl)
        {
            // Remove validation for properties not posted from the form
            ModelState.Remove("ImgUrl");
            ModelState.Remove("Cinemas");
            ModelState.Remove("Categories");
            ModelState.Remove("ActorList");
            ModelState.Remove("Movie.Cinema");
            ModelState.Remove("Movie.Category");

            var existingMovie = _Context.Movies.AsNoTracking().FirstOrDefault(e => e.Id == vm.Movie.Id);
            if (existingMovie == null)
            {
                return NotFound();
            }

            // Validation: EndDate must be after StartDate
            if (vm.Movie.EndDate < vm.Movie.StartDate)
            {
                ModelState.AddModelError("Movie.EndDate", "End date must be after the start date.");
            }

            // Validation: Image format (only if new image is uploaded)
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(ImgUrl.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImgUrl", "Only JPG, PNG, or WEBP image formats are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Cinemas = _Context.Cinemas.ToList();
                vm.Categories = _Context.Categories.ToList();
                vm.ActorList = _Context.Actors.ToList();
                return View(vm);
            }

            // Save image if new one is uploaded
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\movies", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await ImgUrl.CopyToAsync(stream);
                }

                // Delete old image
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\movies", existingMovie.ImgUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                vm.Movie.ImgUrl = fileName;
            }
            else
            {
                vm.Movie.ImgUrl = existingMovie.ImgUrl;
            }

            // Update movie
            vm.Movie.MovieStatus = check(vm.Movie.StartDate, vm.Movie.EndDate);
            _Context.Movies.Update(vm.Movie);
            await _Context.SaveChangesAsync();

            // Update actor relations
            var existingActorMovies = _Context.ActorMovies.Where(am => am.MovieId == vm.Movie.Id).ToList();
            _Context.ActorMovies.RemoveRange(existingActorMovies);
            await _Context.SaveChangesAsync();

            if (vm.SelectedActorIds != null && vm.SelectedActorIds.Any())
            {
                foreach (var actorId in vm.SelectedActorIds)
                {
                    _Context.ActorMovies.Add(new ActorMovie
                    {
                        MovieId = vm.Movie.Id,
                        ActorId = actorId
                    });
                }
                await _Context.SaveChangesAsync();
            }

            TempData["success-notification"] = "Movie updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {

            var movie = _Context.Movies.Include(e => e.Cinema).Include(e => e.Category).FirstOrDefault(e => e.Id == id);
            if (movie is not null)
            {
                _Context.Movies.Remove(movie);
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\movies", movie.ImgUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                _Context.SaveChanges();
                var actorMovies = _Context.ActorMovies.Where(am => am.MovieId == id).ToList();
                if (actorMovies.Any())
                {
                    _Context.ActorMovies.RemoveRange(actorMovies);
                    _Context.SaveChanges();
                    TempData["success-notification"] = "Movie deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }
}

}
