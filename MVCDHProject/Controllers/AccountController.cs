using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCDHProject.Models;

namespace MVCDHProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        public ViewResult Register()
        {
            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserViewModel userModel)
        {
            if(ModelState.IsValid)
            {
                //IdentityUser represents a new user or (Model class) with a given set if attributes
                IdentityUser identityUser = new IdentityUser
                {
                    UserName=userModel.Name,
                    Email=userModel.Email,
                    PhoneNumber=userModel.Mobile
                };

                //Creates a new user and returns a result which tell about success or failure
                var result = await userManager.CreateAsync(identityUser, userModel.Password);
                if(result.Succeeded)
                {
                    //Performing a Sign-In into the application
                    await signInManager.SignInAsync(identityUser, false);  //Here signed in from login page so stay signed value is false
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var Error in result.Errors)
                    {
                        //Displaying error details to the user
                        ModelState.AddModelError("",Error.Description);
                    }
                }
            }
            return View(userModel);
        }

        [HttpGet]
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if(ModelState.IsValid)
            {
                var result=await signInManager.PasswordSignInAsync(loginModel.Name,loginModel.Password,loginModel.RememberMe,false);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View(loginModel);
        }
    }
}
