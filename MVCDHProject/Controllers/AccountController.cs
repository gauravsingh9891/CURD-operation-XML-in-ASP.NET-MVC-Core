using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using MVCDHProject.Models;
using System.Security.Claims;
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
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                //IdentityUser represents a new user or (Model class) with a given set if attributes
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = userModel.Name,
                    Email = userModel.Email,
                    PhoneNumber = userModel.Mobile
                };

                //Creates a new user and returns a result which tell about success or failure
                var result = await userManager.CreateAsync(identityUser, userModel.Password);
                if (result.Succeeded)
                {
                    //Implementing logic for sending a mail to confirm the Email
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);    //Generate token for confirm password for a user
                    var confirmationUrlLink = Url.Action("ConfirmEmail", "Account", new { UserId = identityUser.Id, Token = token }, Request.Scheme);     //Building an url link for sending
                    SendMail(identityUser, confirmationUrlLink, "Email Confirmation Link");  //Calling Send Mail Method
                    TempData["Title"] = "Email Confirmation Link";
                    TempData["Message"] = "A confirm email link has been sent to your registered mail, click on it to confirm.";
                    return View("DisplayMessages");
                }
                else
                {
                    foreach (var Error in result.Errors)
                    {
                        //Displaying error details to the user
                        ModelState.AddModelError("", Error.Description);
                    }
                }
            }
            return View(userModel);
        }
        #endregion

        #region Sending Email's & Confirm Mail
        public void SendMail(IdentityUser identityUser, string requestLink, string subject)
        {
            StringBuilder mailBody = new StringBuilder();
            mailBody.Append("Hello " + identityUser.UserName + "<br/><br/>");
            if (subject == "Email Confirmation Link")
                mailBody.Append("Click on the link below to confirm your email.");
            else if (subject == "Change Password Link")
                mailBody.Append("Click on the link below to reset your password.");

            mailBody.Append("<br/>");
            mailBody.Append(requestLink);
            mailBody.Append("<br/><br/>");
            mailBody.Append("Customer Support");

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = mailBody.ToString();

            MailboxAddress fromAddress = new MailboxAddress("Customer Support", "gaurav.coder127@gmail.com");
            MailboxAddress toAddress = new MailboxAddress(identityUser.UserName, identityUser.Email);

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
            if (userId != null && token != null)
            {
                var User = await userManager.FindByIdAsync(userId);
                if (User != null)
                {
                    var result = await userManager.ConfirmEmailAsync(User, token);
                    if (result.Succeeded)
                    {
                        TempData["Title"] = "Email Confirmation Success.";
                        TempData["Message"] = "Email confirmation is completed. You can now login into the application.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        StringBuilder Errors = new StringBuilder();
                        foreach (var Error in result.Errors)
                        {
                            Errors.Append(Error.Description + "<br/>");
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                //Code for checking whether Email is confirmed or not
                var user = await userManager.FindByNameAsync(loginModel.Name);
                if (user != null && (await userManager.CheckPasswordAsync(user, loginModel.Password)) && user.EmailConfirmed == false)
                {
                    ModelState.AddModelError("", "Your email is not confirmed");
                    return View(loginModel);
                }

                //Checking authenticating user credentials
                var result = await signInManager.PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false);
                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction("Index", "Home");
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

        #region Forgot Passwored & Reset Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var User = await userManager.FindByNameAsync(model.Name);
                if (User != null && await userManager.IsEmailConfirmedAsync(User))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(User);
                    var confirmationUrlLink = Url.Action("ChangePassword", "Account", new { UserId = User.Id, Token = token }, Request.Scheme);
                    SendMail(User, confirmationUrlLink, "Change Password Link");
                    TempData["Title"] = "Change Password Link";
                    TempData["Message"] = "Change password link has been sent to your mail, click on it and change password.";
                    return View("DisplayMessages");
                }
                else
                {
                    TempData["Title"] = "Change Password Mail Generation Failed.";
                    TempData["Message"] = "Either the Username you have entered is in-valid or your email is not confirmed.";
                    return View("DisplayMessages");
                }
            }
            return View(model);
        }

        [HttpGet]
        public ViewResult ChangePassword()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var User = await userManager.FindByIdAsync(model.Userid);
                if (User != null)
                {
                    var result = await userManager.ResetPasswordAsync(User, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Title"] = "Reset Password Success";
                        TempData["Message"] = "You password has been reset successfully";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        foreach (var Error in result.Errors)
                            ModelState.AddModelError("", Error.Description);
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User";
                    TempData["Message"] = "No user exists with the given User Id";
                    return View("DisplayMessages");
                }
            }
            return View(model);
        }
        #endregion

        #region External Login (Facebook & Google)
        public IActionResult ExternalLogin(string returnUrl, string Provider)
        {
            var url = Url.Action("CallBack", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(Provider, url);
            return new ChallengeResult(Provider, properties);       //Redirect to google or facebool login page
        }
        public async Task<IActionResult> CallBack(string returnUrl)
        {
            //It will redirect to Home page if returnUrl=null
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "~/";
            }

            //Collecting information from Google or Facebook, if returnUrl is not null
            var info = await signInManager.GetExternalLoginInfoAsync();

            //If Google or Facebook doesn't provide any Data the showing Error
            if (info == null)
            {
                ModelState.AddModelError("", "Error in loading external login information.");
                return View("Login");
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);   //It checks user coming first or second time by checkin entery available in "AspNetUserLogins" Table
            if (signInResult.Succeeded) //Here record is available in AspNetUserLogins then if means second time coming, then direct login
            {
                return LocalRedirect(returnUrl);
            }
            else      //Record is not availabe in AspNetUserLogins means it's first time user coming
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);    //Picking email from collected data form Google or Facebook
                if(email!=null)
                {
                    var user = await userManager.FindByEmailAsync(email); //Getting the information of user crossponding the email id from "AspNetUsers" Table

                    //If no user found crossponding the email means user is not exsisting in "AspNetUsers" Table
                    if (user==null)
                    { 
                        //Creating new account
                        user = new IdentityUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone)
                        };

                        //This will register or insert new user details in "AspNetUsers" table because user is not existing
                        var identityResult = await userManager.CreateAsync(user);
                    }

                    //This will insert the record in "AspNetUserLogins"
                    await userManager.AddLoginAsync(user, info);

                    //This will login you in application
                    await signInManager.SignInAsync(user, false);

                    return LocalRedirect(returnUrl);
                }

                //Display Any Error occured
                TempData["Title"] = "Error";
                TempData["Message"] = "Email claim not received from thire party provider";
                return RedirectToAction("DisplayMessages");
            }
        }
        #endregion
    }
}
