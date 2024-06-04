using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using K_Burns_GU2_Speedo_Models.Models;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using System.Reflection;
using System.Data.Entity;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Manages account-related actions such as login, registration, password reset, and external logins.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class with specified user and sign-in managers.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Gets or sets the sign-in manager.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        /// <summary>
        /// Gets or sets the user manager.
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// GET: /Account/Login
        /// 
        /// Displays the login page.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The login view.</returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// 
        /// Processes the login request.
        /// </summary>
        /// <param name="model">The login view model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The appropriate action result based on the login outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            if (result == SignInStatus.Success)
            {
                // After successful login, get the user by email.
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Check if the email is confirmed
                    if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        // Redirect to email confirmation page
                        return View("EmailConfirmation", user);
                    }

                    // Now you can check if the logged-in user is in the "Customer" role.
                    var isInCustomerRole = await UserManager.IsInRoleAsync(user.Id, "Customer");
                    if (isInCustomerRole)
                    {
                        SpeedoModelsDbContext context = new SpeedoModelsDbContext();

                        var abandonedBasket = context.Baskets.Include(b => b.User).Include(b => b.BasketItems).FirstOrDefault(b => b.User.Id == user.Id && b.BasketAbandoned);

                        if (abandonedBasket != null)
                        {
                            abandonedBasket.BasketAbandoned = false;
                            context.SaveChanges();

                            // Update session variable if you are using session to track basket size or contents.
                            Session["BasketSize"] = abandonedBasket.BasketItems.Sum(ol => ol.Quantity);
                        }
                    }
                }

                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Handle other sign-in statuses (LockedOut, RequiresVerification, Failure)
                switch (result)
                {
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
        }

        /// <summary>
        /// GET: /Account/VerifyCode
        /// 
        /// Displays the verify code page for two-factor authentication.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="rememberMe">Whether to remember the login.</param>
        /// <returns>The verify code view.</returns>
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// POST: /Account/VerifyCode
        /// 
        /// Verifies the code for two-factor authentication.
        /// </summary>
        /// <param name="model">The verify code view model.</param>
        /// <returns>The appropriate action result based on the verification outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        /// <summary>
        /// GET: /Account/Register
        /// 
        /// Displays the registration page.
        /// </summary>
        /// <returns>The register view.</returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// 
        /// Processes the registration request.
        /// </summary>
        /// <param name="model">The register view model.</param>
        /// <returns>The appropriate action result based on the registration outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Customer customerUser = new Customer 
                { 
                    UserName = model.Email,
                    Email = model.Email,
                    Forename = model.FirstName,
                    Surname = model.LastName,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    //record the date and time the user registers at
                    RegisteredAt = DateTime.Now,
                    IsActive = true,
                    CustomerType = model.CustomerType
                };
                var result = await UserManager.CreateAsync(customerUser, model.Password);
                //if successful..
                if (result.Succeeded)
                {
                    //.. then add user to member role
                    await UserManager.AddToRoleAsync(customerUser.Id, "Customer");
                    //await SignInManager.SignInAsync(customerUser, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(customerUser.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = customerUser.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(customerUser.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    ViewBag.Title = "Confirmation Email Sent";
                    return View("EmailConfirmation", customerUser);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// GET: /Account/ConfirmEmail
        /// 
        /// Confirms the user's email address.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="code">The confirmation code.</param>
        /// <returns>The appropriate view based on the confirmation outcome.</returns>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// 
        /// Displays the forgot password page.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/ForgotPassword
        /// 
        /// Processes the forgot password request.
        /// </summary>
        /// <param name="model">The forgot password view model.</param>
        /// <returns>The appropriate action result based on the forgot password outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// GET: /Account/ForgotPasswordConfirmation
        /// 
        /// Displays the forgot password confirmation page.
        /// </summary>
        /// <returns>The forgot password confirmation view.</returns>
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// GET: /Account/ResetPassword
        /// 
        /// Displays the reset password page.
        /// </summary>
        /// <param name="code">The reset code.</param>
        /// <returns>The reset password view.</returns>
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        /// <summary>
        /// POST: /Account/ResetPassword
        /// 
        /// Processes the reset password request.
        /// </summary>
        /// <param name="model">The reset password view model.</param>
        /// <returns>The appropriate action result based on the reset password outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        /// <summary>
        /// GET: /Account/ResetPasswordConfirmation
        /// 
        /// Displays the reset password confirmation page.
        /// </summary>
        /// <returns>The reset password confirmation view.</returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/ExternalLogin
        /// 
        /// Initiates the process for external login.
        /// </summary>
        /// <param name="provider">The external login provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>A challenge result for the external login provider.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// GET: /Account/SendCode
        /// 
        /// Displays the send code page for two-factor authentication.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="rememberMe">Whether to remember the login.</param>
        /// <returns>The send code view.</returns>
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// POST: /Account/SendCode
        /// 
        /// Sends a two-factor authentication code.
        /// </summary>
        /// <param name="model">The send code view model.</param>
        /// <returns>The appropriate action result based on the send code outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        /// <summary>
        /// GET: /Account/ExternalLoginCallback
        /// 
        /// Handles the callback after external login.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The appropriate action result based on the external login outcome.</returns>
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        /// <summary>
        /// POST: /Account/ExternalLoginConfirmation
        /// 
        /// Confirms the external login.
        /// </summary>
        /// <param name="model">The external login confirmation view model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The appropriate action result based on the external login confirmation outcome.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        /// <summary>
        /// POST: /Account/LogOff
        /// 
        /// Logs off the current user.
        /// </summary>
        /// <returns>Redirects to the home page after logging off.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //check if the user is in the "Customer" role.
            if (User.IsInRole("Customer"))
            {
                SpeedoModelsDbContext context = new SpeedoModelsDbContext();

                string userId = User.Identity.GetUserId();

                //Get the users basket
                var basket = context.Baskets.Include(b => b.BasketItems).Include(b => b.User).FirstOrDefault(b => b.User.Id == userId && !b.BasketAbandoned);

                if (basket != null)
                {
                    // Mark the basket as abandoned.
                    basket.BasketAbandoned = true;
                    context.Entry(basket).State = EntityState.Modified;
                    context.SaveChanges();
                }
                // Clear the session variable for BasketSize.
                Session["BasketSize"] = null;
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// GET: /Account/ExternalLoginFailure
        /// 
        /// Displays the external login failure page.
        /// </summary>
        /// <returns>The external login failure view.</returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether to release both managed and unmanaged resources or only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        /// <summary>
        /// Gets the authentication manager.
        /// </summary>
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// Adds the specified errors to the model state.
        /// </summary>
        /// <param name="result">The identity result containing the errors.</param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// Redirects to a local URL if it is local; otherwise, redirects to the home page.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The redirect action result.</returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Represents a challenge result for external login.
        /// </summary>
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChallengeResult"/> class.
            /// </summary>
            /// <param name="provider">The login provider.</param>
            /// <param name="redirectUri">The redirect URI.</param>
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ChallengeResult"/> class.
            /// </summary>
            /// <param name="provider">The login provider.</param>
            /// <param name="redirectUri">The redirect URI.</param>
            /// <param name="userId">The user ID.</param>
            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            /// <summary>
            /// Executes the result by issuing a challenge.
            /// </summary>
            /// <param name="context">The controller context.</param>
            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}