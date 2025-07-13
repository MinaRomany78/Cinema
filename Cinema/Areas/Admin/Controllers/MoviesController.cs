
using Cinema.Data.Enums;

using Cinema.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        //  private readonly ApplicationDbContext _Context = new();
        private readonly IMovieRepository _moviesRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IActorRepository _actorRepository;
        private readonly IActorMoviesRepository _actorMoviesRepository;

        public MoviesController(
            IMovieRepository moviesRepository,
            ICategoryRepository categoryRepository,
            ICinemaRepository cinemaRepository,
            IActorRepository actorRepository,
            IActorMoviesRepository actorMoviesRepository)
        {
            _moviesRepository = moviesRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _actorRepository = actorRepository;
            _actorMoviesRepository = actorMoviesRepository; // Assuming you have an implementation for this
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _moviesRepository.GetAsync(
                include: new Expression<Func<Movie, object>>[]
                {
                    e => e.Cinema,
                    e => e.Category,
                }
            );

            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var Cinemas = (await _cinemaRepository.GetAsync()).ToList();
            var Categories = (await _categoryRepository.GetAsync()).ToList();
            var Actors = (await _actorRepository.GetAsync()).ToList();
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

                await _moviesRepository.CreateAsync(vm.Movie);

                // ربط الممثلين
                if (vm.SelectedActorIds != null && vm.SelectedActorIds.Any())
                {
                    foreach (var actorId in vm.SelectedActorIds)
                    {
                        await _actorMoviesRepository.CreateAsync(new ActorMovie
                        {
                            MovieId = vm.Movie.Id,
                            ActorId = actorId
                        });
                    }
                    await _actorMoviesRepository.Commit();
                }

                TempData["success-notification"] = "Movie added successfully.";
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل البيانات في حال وجود خطأ(
            vm.Cinemas =( await _cinemaRepository.GetAsync()).ToList();
            vm.Categories = (await _categoryRepository.GetAsync()).ToList();
            vm.ActorList = (await _actorRepository.GetAsync()).ToList();
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
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _moviesRepository.GetOneAsync(
               e => e.Id == id,
               include: new Expression<Func<Movie, object>>[]
               {
                    e => e.Cinema,
                    e => e.Category,
                    e => e.ActorMovies
               });

            if (movie is null)
                return NotFound();

            var Cinemas =(await _cinemaRepository.GetAsync()).ToList();
            var Categories = (await _categoryRepository.GetAsync()).ToList();
            var Actors = (await _actorRepository.GetAsync()).ToList();

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

            var existingMovie = await _moviesRepository.GetOneAsync(e => e.Id == vm.Movie.Id, tracked: false);
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
                vm.Cinemas = (await _cinemaRepository.GetAsync()).ToList();
                vm.Categories = (await _categoryRepository.GetAsync()).ToList();
                vm.ActorList = (await _actorRepository.GetAsync()).ToList();
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
            await _moviesRepository.UpdateAsync(vm.Movie);


            // Update actor relations
            var existingActors = await  _actorMoviesRepository.GetAsync(am => am.MovieId == vm.Movie.Id);
            foreach (var am in existingActors)
            {
                await _actorMoviesRepository.DeleteAsync(am);
            }
            if (vm.SelectedActorIds != null && vm.SelectedActorIds.Any())
            {
                foreach (var actorId in vm.SelectedActorIds)
                {
                    await _actorMoviesRepository.CreateAsync(new ActorMovie
                    {
                        MovieId = vm.Movie.Id,
                        ActorId = actorId
                    });
                }
                await _actorMoviesRepository.Commit();
            }

            TempData["success-notification"] = "Movie updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {

            var movie = await _moviesRepository.GetOneAsync(
                            e => e.Id == id,
                            include: new Expression<Func<Movie, object>>[]
                            {
                                    e => e.Cinema,
                                    e => e.Category
                            });
            if (movie is not null)
            {
                await _moviesRepository.DeleteAsync(movie);
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\movies", movie.ImgUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
               await  _moviesRepository.Commit();
                var actorMovies = await _actorMoviesRepository.GetAsync(am => am.MovieId == id);
                foreach (var am in actorMovies)
                {
                    await _actorMoviesRepository.DeleteAsync(am);
                }

                TempData["success-notification"] = "Movie deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();

        }
}

}
