using Azure.Core;
using Cinema.Utilty;
using Cinema.Utilty.DBInitializer;
using Cinema.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Threading.Tasks;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationUserOtpRepository _applicationUserOtpRepository;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IApplicationUserOtpRepository applicationUserOtpRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _applicationUserOtpRepository = applicationUserOtpRepository;
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
                await _userManager.AddToRoleAsync(user, SD.Customer);
                TempData["success-notification"] = "User Created successfully!";
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ComfirmEmil), "Account", new
                {
                    userId = user.Id,
                    token = token,
                    area = "Identity"

                }, Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email, "Comfirm your Account", $"<h1>Comfirm your Account by Chilking <a href='{link}'> Here</a></h1>");

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
            var user = await _userManager.FindByEmailAsync(loginVm.UserNameOrEmail);
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
                    await _signInManager.SignInAsync(user, loginVm.RememberMe);

                    TempData["success-notification"] = "Login successfully!";
                    return RedirectToAction("Index", "Home", new { area = "customer" });
                }

            }
            ModelState.AddModelError("UserNameOrEmail", "Invaild UserName Or Email");
            ModelState.AddModelError("Password", "Invaild Password");

            return View(loginVm);

        }


        public async Task<IActionResult> ComfirmEmil(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    TempData["error-notification"] = $"{String.Join(",", result.Errors)}";
                }
                else
                {
                    TempData["success-notification"] = "Email Comfirmation successfully!";
                }
                return RedirectToAction("Index", "Home", new { area = "customer" });
            }
            return NotFound();

        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success-notification"] = "Logout successfully!";
            return RedirectToAction("Index", "Home", new { area = "customer" });
        }
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resendEmailConfirmationVM);
            }
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.EmailOrUserName);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.EmailOrUserName);
            }
            if (user is not null)
            {
                // Send Confirmation Email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ComfirmEmil), "Account", new { userId = user.Id, token = token, area = "Identity" }, Request.Scheme);
                await _emailSender.SendEmailAsync(user!.Email ?? "", "Confirm Your Account Again!", $"<h1>Confirm Your Account By Clicking <a href='{link}'>here</a></h1>");
                TempData["success-notification"] = "Send Email Successfully";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            return View(resendEmailConfirmationVM);


        }
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPasswordVM);
            }
            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.EmailOrUserName);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(forgetPasswordVM.EmailOrUserName);
            }
            if (user is not null)
            {
                var otpNumber = new Random().Next(100000, 999999);
                var TotalNumber =( await _applicationUserOtpRepository.GetAsync(x => x.ApplicationUserId == user.Id && DateTime.Now.Day== x.SendDate.Day)).Count();
                if (TotalNumber >= 20)
                {
                    TempData["error-notification"] = "You have reached the maximum number of OTP requests for today. Please try again tomorrow.";
                    return View(forgetPasswordVM);
                }
                await _applicationUserOtpRepository.CreateAsync(new ApplicationUserOtp
                {
                    ApplicationUserId = user.Id,
                    OTPNumber = otpNumber,
                    SendDate= DateTime.UtcNow,
                    Status=false,
                    ValidTo = DateTime.UtcNow.AddMinutes(30),
                    Reason = "Forget Password"


                });

                await _emailSender.SendEmailAsync(user.Email ?? "", "Reset Password", $"<h1>Reset Your Password By Clicking otp{otpNumber}</h1>");
                TempData["success-notification"] = "Send  otp Email Successfully";
                return RedirectToAction("ResetPassword", "Account", new { area = "Identity",userId =user.Id });
            }
            ModelState.AddModelError("EmailOrUserName", "Invaild UserName Or Email");
            return View(forgetPasswordVM);
        }
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user=await _userManager.FindByIdAsync(userId);  
            if(user is not null)
            { 
                return View(new ResetPasswordVM { UserId=userId});
            }
            return NotFound();
            
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if(!ModelState.IsValid)
            {
                return View(resetPasswordVM);
            }
            var user = await _userManager.FindByIdAsync(resetPasswordVM.UserId);
            if (user is not null)
            {
                var lastOTP = (await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == resetPasswordVM.UserId)).OrderBy(e => e.Id).LastOrDefault();

                if (lastOTP is not null)
                {
                    if (lastOTP.OTPNumber == resetPasswordVM.OTP && (lastOTP.ValidTo - DateTime.UtcNow).TotalMinutes < 30 && !lastOTP.Status)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.Password);

                        if (result.Succeeded)
                        {
                            TempData["success-notification"] = "Reset Password Successfully";
                        }
                        else
                        {
                            TempData["error-notification"] = $"{String.Join(",", result.Errors)}";
                        }

                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                }

                TempData["error-notification"] = "Invalid OR Expired OTP";
                return View(resetPasswordVM);
            }
            return NotFound();
        }

        public async Task<IActionResult> Profile()
        {
            var user= await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                
                return View(new ProfileVm
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,

                });

            }
            return RedirectToAction(actionName: "Login", "Account", new { area = "Identity" });

            
        }

        public  async Task<IActionResult> EditProfile()
        {
            var user =await  _userManager.GetUserAsync(User);
            if (user is not null)
            {
                return View(new ProfileVm
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                });
            }
            return RedirectToAction(actionName: "Login", "Account", new { area = "Identity" });
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileVm profileVm)
        {
            if (!ModelState.IsValid)
            {
                return View(profileVm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (user.UserName != profileVm.UserName)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, profileVm.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                    {
                        ModelState.AddModelError("", $"Username: {error.Description}");
                    }
                    return View(profileVm);
                }
            }

            if (user.Email != profileVm.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, profileVm.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError("", $"Email: {error.Description}");
                    }
                    return View(profileVm);
                }
            }

            user.FirstName = profileVm.FirstName;
            user.LastName = profileVm.LastName;
            user.Address = profileVm.Address;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["success-notification"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(profileVm);
        }

        //public async Task<IActionResult> EditProfile(ProfileVm profileVm)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(profileVm);
        //    }
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user is not null)
        //    {
        //        user.UserName = profileVm.UserName;
        //        user.Email = profileVm.Email;
        //        user.FirstName = profileVm.FirstName;
        //        user.LastName = profileVm.LastName;
        //        user.Address = profileVm.Address;
        //        var result = await _userManager.UpdateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            TempData["success-notification"] = "Profile Updated Successfully!";
        //            return RedirectToAction("Index", "Home", new { area = "customer" });
        //        }
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }
        //    }
        //    return View(profileVm);
        //}
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm changePasswordVm)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordVm);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var result = await _userManager.ChangePasswordAsync(user, changePasswordVm.CurrentPassword, changePasswordVm.NewPassword);
                if (result.Succeeded)
                {
                    TempData["success-notification"] = "Change Password Successfully!";
                    return RedirectToAction("Index", "Home", new { area = "customer" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(changePasswordVm);
        }

        //public IActionResult DelecteAccount()
        //{
        //    return View();
        //}   
        //[HttpPost]
        //public async Task<IActionResult> DelecteAccount(string password)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user is not null)
        //    {
        //        var result = await _userManager.CheckPasswordAsync(user, password);
        //        if (result)
        //        {
        //            var deleteResult = await _userManager.DeleteAsync(user);
        //            if (deleteResult.Succeeded)
        //            {
        //                TempData["success-notification"] = "Account Deleted Successfully!";
        //                return RedirectToAction("Index", "Home", new { area = "customer" });
        //            }
        //            foreach (var error in deleteResult.Errors)
        //            {
        //                ModelState.AddModelError("", error.Description);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("password", "Invalid Password");
        //        }
        //    }
        //    return View();
        //}
        public IActionResult AccessDenied()
            { return View(); }
     

    }



    
}

