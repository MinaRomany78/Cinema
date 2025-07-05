using Cinema.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,IEmailSender emailSender,SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm registerVm)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVm);
            }
            ApplicationUser user = new()
            {
                UserName = registerVm.UserName,
                Email = registerVm.Email,
                FirstName = registerVm.FirstName,
                LastName = registerVm.LastName,
                Address = registerVm.Address
            };
            var result = await _userManager.CreateAsync(user, registerVm.Password);
            if (result.Succeeded)
            {
                TempData["success-notification"] = "User Created successfully!";
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
               var link= Url.Action(nameof(ComfirmEmil), "Account",new
                {
                    userId = user.Id,
                    token = token,
                    area = "Identity"
                    
                }, Request.Scheme);
               await _emailSender.SendEmailAsync(user.Email,"Comfirm your Account", $"<h1>Comfirm your Account by Chilking <a href='{link}'> Here</a></h1>");

                return RedirectToAction("Index", "Home", new { area = "customer" });

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(registerVm);



        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }
           var user=  await  _userManager.FindByEmailAsync(loginVm.UserNameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(loginVm.UserNameOrEmail);
            }
            if (user is not null)
            {
                var result = await _userManager.CheckPasswordAsync(user, loginVm.Password);
                if (result)
                {
                    if (!user.EmailConfirmed)
                    {
                        TempData["error-notification"] = "Please comfirm your email before login!";
                        return View(loginVm);
                    }

                    if (!user.LockoutEnabled)
                    {
                        TempData["error-notification"] = $"you have block till {user.LockoutEnd}";
                        return View(loginVm);
                    }
                    await _signInManager.SignInAsync(user,loginVm.RememberMe);
                    
                    TempData["success-notification"] = "Login successfully!";
                    return RedirectToAction("Index", "Home", new { area = "customer" });
                }

            }
            ModelState.AddModelError("UserNameOrEmail", "Invaild UserName Or Email");
            ModelState.AddModelError("Password", "Invaild Password");

            return View(loginVm);

            }


        public async Task<IActionResult> ComfirmEmil(string userId,string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user is not null)
            {
               var result =await _userManager.ConfirmEmailAsync(user, token);
                if(!result.Succeeded)
                {
                    TempData["error-notification"] = $"{String.Join(",",result.Errors)}";
                }else
                {
                    TempData["success-notification"] = "Email Comfirmation successfully!";
                }
                return RedirectToAction("Index", "Home", new { area = "customer" });
            }
            return NotFound();

        }
    }
}
