using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorsController : Controller
    {
        //private readonly ApplicationDbContext _context = new();
        private IActorRepository _actorRepository;
        public ActorsController( IActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
        }
        public async Task<IActionResult> Index()
        {
            var actors =await _actorRepository.GetAsync();
            return View(actors);
        }
        public IActionResult Create()
        {

            return View(new Actor());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Actor actor,IFormFile ProfilePicture)
        {
            if (ProfilePicture is not null&& ProfilePicture.Length>0 )
            { 
                var fileName=Guid.NewGuid().ToString() + Path.GetExtension(ProfilePicture.FileName);
                var filePath=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\cast", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                   await ProfilePicture.CopyToAsync(stream);
                }
                actor.ProfilePicture = fileName;
              await   _actorRepository.Commit();

            }
            ModelState.Remove("ProfilePicture");
            if (ModelState.IsValid)
            {
              await   _actorRepository.CreateAsync(actor);
                TempData["success-notification"] = "Actor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorRepository.GetOneAsync(c => c.Id == id);
            if (actor != null)
            {
                return View(actor);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Actor actor,IFormFile ProfilePicture)
        {
            var actorFile =await _actorRepository.GetOneAsync(c => c.Id == actor.Id,tracked:false);
            if (actorFile is not null)
            {
                if (ProfilePicture is not null && ProfilePicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePicture.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\cast", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await ProfilePicture.CopyToAsync(stream);
                    }
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\cast", actorFile.ProfilePicture);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    actor.ProfilePicture = fileName;

                }
                else
                {
                    actor.ProfilePicture = actorFile.ProfilePicture;
                   
                }
            }
            ModelState.Remove("ProfilePicture");
            if (ModelState.IsValid)
            {
                await _actorRepository.UpdateAsync(actor);
                TempData["success-notification"] = "Actor updated successfully!";
                return RedirectToAction(nameof(Index));

            }
            return NotFound();
            
            
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(c => c.Id == id);
            if (actor != null)
            {
                await _actorRepository.DeleteAsync(actor);
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets\\images\\cast", actor.ProfilePicture);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                TempData["success-notification"] = "Actor deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
