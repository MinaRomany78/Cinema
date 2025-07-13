using Cinema.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Threading.Tasks;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
           _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var result = _userManager.Users.ToList();

            Dictionary<ApplicationUser, string> keyValuePairs = new();

            foreach (var item in result)
            {
                keyValuePairs.Add(item, String.Join(",", await _userManager.GetRolesAsync(item)));
            }

            return View(keyValuePairs.ToDictionary());
        }
        [HttpGet]

        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var model = new AdminRegisterVm
            {
                Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminRegisterVm vm)
        {
            ModelState.Remove("userId"); 
            if (!ModelState.IsValid)
            {
                // إعادة تحميل قائمة الرولز لو فشل التحقق
                vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Address = vm.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, "Mina123+");
            if (result.Succeeded)
            {
                // 🟢 نضيف المستخدم للرول المحدد
                await _userManager.AddToRoleAsync(user, vm.Role);

                TempData["success-notification"] = "User created successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // إعادة تحميل الرولز في حالة الخطأ
            vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new AdminRegisterVm
            {
                userId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Role = userRoles.FirstOrDefault(),
                Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(AdminRegisterVm vm)
        {
           // ModelState.Remove("Role"); 
            ModelState.Remove("Id"); // إزالة هذه الخاصية من التحقق

            if (!ModelState.IsValid)
            {
                // إعادة تحميل قائمة الرولز لو فشل التحقق
                vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();
                return View(vm);
            }
            var user = await _userManager.FindByIdAsync(vm.userId);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = vm.UserName;
            user.Email = vm.Email;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Address = vm.Address;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // 🟢 نحدث الرول للمستخدم
                var currentRoles = await _userManager.GetRolesAsync(user);
                
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                await _userManager.AddToRoleAsync(user, vm.Role);
                TempData["success-notification"] = "User updated successfully!";
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            // إعادة تحميل الرولز في حالة الخطأ
            vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
            return View(vm);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user =await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["success-notification"] = "User deleted successfully!";
              await  _userManager.RemoveFromRoleAsync(user, SD.Customer);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return RedirectToAction("index");
        }

    }
}
