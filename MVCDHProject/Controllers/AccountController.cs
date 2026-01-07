using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using MVCDHProject.Models;
using System.Text;

namespace MVCDHProject.Controllers
{
    public class AccountController : Controller
    {
        #region Fields
        //Fields
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        #endregion

        #region Constructor
        //Constructor
        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        #endregion

        #region Register
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
                    //Implementing logic for sending a mail to confirm the Email
                    var token=await userManager.GenerateEmailConfirmationTokenAsync(identityUser);    //Generate token for confirm password for a user
                    var confirmationUrlLink=Url.Action("ConfirmEmail", "Account", new { UserId = identityUser.Id, Token = token }, Request.Scheme);     //Building an url link for sending
                    SendMail(identityUser,confirmationUrlLink, "Email Confirmation Link");  //Calling Send Mail Method
                    TempData["Title"] = "Email Confirmation Link";
                    TempData["Message"] = "A confirm email link has been sent to your registered mail, click on it to confirm.";
                    return View("DisplayMessages");
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
        #endregion

        #region Sending Email's & Confirm Mail
        public void SendMail(IdentityUser identityUser,string requestLink, string subject)
        {
            StringBuilder mailBody = new StringBuilder();
            mailBody.Append("Hello " + identityUser.UserName + "<br/><br/>");
            if(subject=="Email Confirmation Link")
                mailBody.Append("Click on the link below to confirm your email.");
            else if(subject=="Change Password Link")
                mailBody.Append("Click on the link below to reset your password.");

            mailBody.Append("<br/>");
            mailBody.Append(requestLink);
            mailBody.Append("<br/><br/>");
            mailBody.Append("Customer Support");

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = mailBody.ToString();

            MailboxAddress fromAddress = new MailboxAddress("Customer Support", "gaurav.coder127@gmail.com");
            MailboxAddress toAddress = new MailboxAddress(identityUser.UserName,identityUser.Email);

            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(fromAddress);
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = bodyBuilder.ToMessageBody();

            //used to send the mail
            SmtpClient smtpClient = new SmtpClient();   //using MailKit.Net.Smtp namespace
            smtpClient.Connect("smtp.gmail.com", 465, true);    //465 is a port number os Google Smtp Server, true indicate SSL(https) enables, false means no https,
            smtpClient.Authenticate("gaurav.coder127@gmail.com", "acnn ostg madu rynv");    //provide sender mail id & generate an app password
            smtpClient.Send(mailMessage);   
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId!=null&&token!=null)
            {
               var User=await userManager.FindByIdAsync(userId);
                if(User!=null)
                {
                    var result = await userManager.ConfirmEmailAsync(User, token);
                    if(result.Succeeded)
                    {
                        TempData["Title"] = "Email Confirmation Success.";
                        TempData["Message"] = "Email confirmation is completed. You can now login into the application.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        StringBuilder Errors = new StringBuilder();
                        foreach(var Error in result.Errors)
                        {
                            Errors.Append(Error.Description + ". ");
                        }
                        TempData["Title"] = "Confirmation Email Failure";
                        TempData["Message"] = Errors.ToString();
                        return View("DisplayMessages");
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User Id.";
                    TempData["Message"] = "User Id which is present in confirm email link is in-valid.";
                    return View("DisplayMessages");
                }
            }
            else
            {
                TempData["Title"] = "Invalid Email Confirmation Link.";
                TempData["Message"] = "Email confirmation link is invalid, either missing the User Id or Confirmation Token.";
                return View("DisplayMessages");
            }

        }
        #endregion

        #region Login
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
                //Code for checking whether Email is confirmed or not
                var user = await userManager.FindByNameAsync(loginModel.Name);
                if(user!=null&&(await userManager.CheckPasswordAsync(user,loginModel.Password))&&user.EmailConfirmed==false)
                {
                    ModelState.AddModelError("", "Your email is not confirmed");
                    return View(loginModel);
                }

                //Checking authenticating user credentials
                var result=await signInManager.PasswordSignInAsync(loginModel.Name,loginModel.Password,loginModel.RememberMe,false);
                if(result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction("Index","Home");
                    else
                        return LocalRedirect(loginModel.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View(loginModel);
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion
    }
}
