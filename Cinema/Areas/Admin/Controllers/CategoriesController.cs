
using Cinema.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private ICategoryRepository _categoryRepository;
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this._categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Models.Category category)
        {
            if (ModelState.IsValid)
            {
               await _categoryRepository.CreateAsync(category);
                TempData["success-notification"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        } 
        [HttpPost]
        public async Task<IActionResult> Edit(Models.Category category)
        {
            if (ModelState.IsValid)
            {
              await _categoryRepository.UpdateAsync(category);
                TempData["success-notification"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            await _categoryRepository.DeleteAsync(category);
            TempData["success-notification"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
