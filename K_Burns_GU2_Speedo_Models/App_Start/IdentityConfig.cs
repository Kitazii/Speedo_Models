using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using K_Burns_GU2_Speedo_Models.Models;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;
using SendGrid.Helpers.Mail;
using System.Net;
using SendGrid;

namespace K_Burns_GU2_Speedo_Models
{
    /// <summary>
    /// Provides email services for sending emails.
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// Sends an asynchronous email message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SendAsync(IdentityMessage message)
        {
            return configSendGridasync(message);
        }

        /// <summary>
        /// Configures and sends an email message using SendGrid.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task configSendGridasync(IdentityMessage message)
        {
            //var apiKey = "";
            //var client = new SendGridClient(apiKey);
            //var from = new EmailAddress("mr.kieranburns@gmail.com", "Kieran");
            //var subject = message.Subject;
            //var to = new EmailAddress(message.Destination);
            //var plainTextContent = message.Body;
            //var htmlContent = message.Body;
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            //// Send the email
            //if (client != null)
            //{
            //    await client.SendEmailAsync(msg);
            //}
            //else
            //{
            //    Trace.TraceError("Failed to create Web Transport");
            //    await Task.FromResult(0);
            //}
        }
    }

    /// <summary>
    /// Provides SMS services for sending text messages.
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// Sends an asynchronous SMS message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Gets the Twilio account SID, Authentication token and phone number to send from, from the configuration in the app settings
            var accountSid = ConfigurationManager.AppSettings["SMSAccountIdentification"];
            var authToken = ConfigurationManager.AppSettings["SMSAccountPassword"];
            var fromNumber = ConfigurationManager.AppSettings["SMSAccountFrom"];

            // Initialize Twilio client with account SID and authentication token
            TwilioClient.Init(accountSid, authToken);

            try
            {
                // Create and send the SMS message using Twilio
                MessageResource result = MessageResource.Create(
                new PhoneNumber(message.Destination),   // Destination phone number
                from: new PhoneNumber(fromNumber),      // Sender phone number
                body: message.Body                      // Message body
                );

                // Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
                Trace.TraceInformation(result.Status.ToString());
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                // Log the exception
                Trace.TraceError("Twilio API error: " + ex.Message);
                throw; // Rethrow the exception to be handled by the calling method
            }

            // Twilio doesn't currently have an async API, so return success.
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Configures the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    /// </summary>
    public class ApplicationUserManager : UserManager<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserManager"/> class.
        /// </summary>
        /// <param name="store">The user store.</param>
        public ApplicationUserManager(IUserStore<User> store)
            : base(store)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ApplicationUserManager"/>.
        /// </summary>
        /// <param name="options">The identity factory options.</param>
        /// <param name="context">The OWIN context.</param>
        /// <returns>An instance of <see cref="ApplicationUserManager"/>.</returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<User>(context.Get<Models.SpeedoModelsDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 1,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = true,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<User>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<User>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });

            // Configure email and SMS services
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();

            // Configure user token provider
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<User>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    /// <summary>
    /// Configures the application sign-in manager used in this application.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<User, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSignInManager"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="authenticationManager">The authentication manager.</param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        /// <summary>
        /// Creates a claims identity for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the claims identity for the user.</returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        /// <summary>
        /// Creates an instance of the <see cref="ApplicationSignInManager"/>.
        /// </summary>
        /// <param name="options">The identity factory options.</param>
        /// <param name="context">The OWIN context.</param>
        /// <returns>An instance of <see cref="ApplicationSignInManager"/>.</returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
