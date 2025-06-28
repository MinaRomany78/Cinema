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
        private readonly ApplicationDbContext _context = new();
        public IActionResult Index()
        {
            var actors = _context.Actors.ToList();
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
                _context.SaveChanges();
            }
            ModelState.Remove("ProfilePicture");
            if (ModelState.IsValid)
            {
                _context.Actors.Add(actor);
                _context.SaveChanges();
                TempData["success-notification"] = "Actor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        public IActionResult Edit(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.Id == id);
            if (actor != null)
            {
                return View(actor);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Actor actor,IFormFile ProfilePicture)
        {
            var actorFile = _context.Actors. AsNoTracking().FirstOrDefault(a => a.Id == actor.Id);
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
                _context.Actors.Update(actor);
                _context.SaveChanges();
                TempData["success-notification"] = "Actor updated successfully!";
                return RedirectToAction(nameof(Index));

            }
            return NotFound();
            
            
        }
        
        public IActionResult Delete(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.Id == id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                _context.SaveChanges();
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
