using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using System.Diagnostics;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Manages user account settings and security features.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageController"/> class.
        /// </summary>
        public ManageController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageController"/> class with specified user and sign-in managers.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Gets the sign-in manager instance.
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
        /// Gets the user manager instance.
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
        /// GET: /Manage/Index
        /// 
        /// Displays the index page with user account settings.
        /// </summary>
        /// <param name="message">The manage message ID.</param>
        /// <returns>The index view with user account settings.</returns>
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        /// <summary>
        /// POST: /Manage/RemoveLogin
        /// 
        /// Removes an external login.
        /// </summary>
        /// <param name="loginProvider">The login provider.</param>
        /// <param name="providerKey">The provider key.</param>
        /// <returns>Redirects to the manage logins view with a message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        /// <summary>
        /// GET: /Manage/AddPhoneNumber
        /// 
        /// Displays the add phone number page.
        /// </summary>
        /// <returns>The add phone number view.</returns>
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        /// <summary>
        /// POST: /Manage/AddPhoneNumber
        /// 
        /// Adds a phone number to the user account.
        /// </summary>
        /// <param name="model">The add phone number view model.</param>
        /// <returns>Redirects to verify phone number view if successful, otherwise redisplays the add phone number view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                try
                {
                    await UserManager.SmsService.SendAsync(message);
                }
                catch (Twilio.Exceptions.ApiException ex)
                {
                    // Log the error
                    Trace.TraceError("Twilio API error: " + ex.Message);

                    // Add an error message to the ModelState
                    ModelState.AddModelError(string.Empty, "Failed to send verification SMS. Please make sure the phone number is valid and try again.");

                    // Return the view with the model to display the error
                    return View(model);
                }
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        /// <summary>
        /// POST: /Manage/EnableTwoFactorAuthentication
        /// 
        /// Enables two-factor authentication for the user account.
        /// </summary>
        /// <returns>Redirects to the index view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /// <summary>
        /// POST: /Manage/DisableTwoFactorAuthentication
        /// 
        /// Disables two-factor authentication for the user account.
        /// </summary>
        /// <returns>Redirects to the index view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /// <summary>
        /// GET: /Manage/VerifyPhoneNumber
        /// 
        /// Displays the verify phone number page.
        /// </summary>
        /// <param name="phoneNumber">The phone number to verify.</param>
        /// <returns>The verify phone number view.</returns>
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        /// <summary>
        /// POST: /Manage/VerifyPhoneNumber
        /// 
        /// Verifies the phone number.
        /// </summary>
        /// <param name="model">The verify phone number view model.</param>
        /// <returns>Redirects to the index view if successful, otherwise redisplays the verify phone number view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        /// <summary>
        /// POST: /Manage/RemovePhoneNumber
        /// 
        /// Removes the phone number from the user account.
        /// </summary>
        /// <returns>Redirects to the index view with a message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        /// <summary>
        /// GET: /Manage/ChangePassword
        /// 
        /// Displays the change password page.
        /// </summary>
        /// <returns>The change password view.</returns>
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// POST: /Manage/ChangePassword
        /// 
        /// Changes the user password.
        /// </summary>
        /// <param name="model">The change password view model.</param>
        /// <returns>Redirects to the index view if successful, otherwise redisplays the change password view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        /// <summary>
        /// GET: /Manage/SetPassword
        /// 
        /// Displays the set password page.
        /// </summary>
        /// <returns>The set password view.</returns>
        public ActionResult SetPassword()
        {
            return View();
        }

        /// <summary>
        /// POST: /Manage/SetPassword
        /// 
        /// Sets a password for the user account.
        /// </summary>
        /// <param name="model">The set password view model.</param>
        /// <returns>Redirects to the index view if successful, otherwise redisplays the set password view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// GET: /Manage/ManageLogins
        /// 
        /// Displays the manage logins page.
        /// </summary>
        /// <param name="message">The manage message ID.</param>
        /// <returns>The manage logins view.</returns>
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        /// <summary>
        /// POST: /Manage/LinkLogin
        /// 
        /// Links an external login to the user account.
        /// </summary>
        /// <param name="provider">The external login provider.</param>
        /// <returns>Redirects to the external login provider to link a login for the current user.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        /// <summary>
        /// GET: /Manage/LinkLoginCallback
        /// 
        /// Handles the callback after linking an external login to the user account.
        /// </summary>
        /// <returns>Redirects to the manage logins view with a message if successful.</returns>
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether to release both managed and unmanaged resources or only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
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
        /// Determines whether the user has a password.
        /// </summary>
        /// <returns><c>true</c> if the user has a password; otherwise, <c>false</c>.</returns>
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the user has a phone number.
        /// </summary>
        /// <returns><c>true</c> if the user has a phone number; otherwise, <c>false</c>.</returns>
        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        /// <summary>
        /// Enumeration for manage message IDs.
        /// </summary>
        public enum ManageMessageId
        {
            /// <summary> AddPhoneSuccess Enum Value: 0. </summary>
            AddPhoneSuccess,
            /// <summary> ChangePasswordSuccess Enum Value: 1. </summary>
            ChangePasswordSuccess,
            /// <summary> SetTwoFactorSuccess Enum Value: 2. </summary>
            SetTwoFactorSuccess,
            /// <summary> SetPasswordSuccess Enum Value: 3. </summary>
            SetPasswordSuccess,
            /// <summary> RemoveLoginSuccess Enum Value: 4. </summary>
            RemoveLoginSuccess,
            /// <summary> RemovePhoneSuccess Enum Value: 5. </summary>
            RemovePhoneSuccess,
            /// <summary> Error Enum Value: 6. </summary>
            Error
        }

#endregion
    }
}