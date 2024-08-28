using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using K_Burns_GU2_Speedo_Models.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Runtime.CompilerServices;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Only allows admin to have authority to use this controller
    /// and redirects to the main page if the user is already logged in.
    /// </summary>
    [RedirectUnauthorizedUsersFilter]
    public class AdminController : AccountController
    {
        // Creating an instance of the Speedo Models DB Context
        private SpeedoModelsDbContext db = new SpeedoModelsDbContext();

        /// <summary>
        /// Creates a parameterless constructor, calling the parent constructor.
        /// </summary>
        public AdminController() : base() { }

        /// <summary>
        /// Creates a constructor with 2 arguments, calling the parent constructor.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) : base(userManager, signInManager) { }

        //**********************************************************************************************************************************************************

        //  CREATE A NEW EMPLOYEE
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Displays the Create Employee view.
        /// </summary>
        /// <returns>The Create Employee view.</returns>
        [HttpGet]
        public ActionResult CreateEmployee()
        {
            // Initialize a new instance of CreateEmployeeViewModel.
            CreateEmployeeViewModel employee = new CreateEmployeeViewModel();

            // Get all the roles from the database (except "Customer") and store them as a SelectedListItem so that we can display the roles in a dropdownlist
            var roles = db.Roles
                .Where(r => r.Name != "Customer")
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

            // Assign the roles to the employee roles property
            employee.Positions = roles;

            // Send the employees model to the view
            return View(employee);
        }

        /// <summary>
        /// Processes the creation of a new employee.
        /// </summary>
        /// <param name="model">The CreateEmployeeViewModel containing the employee details.</param>
        /// <returns>Redirects to ManageEmployees view if successful; otherwise, returns the CreateEmployee view with the model.</returns>
        [HttpPost]
        public ActionResult CreateEmployee(CreateEmployeeViewModel model)
        {
            // To ensure model.Roles isn't null
            // Get all the roles from the database (except "Customer") and store them as a SelectedListItem so that we can display the roles in a dropdownlist
            var roles = db.Roles
                .Where(r => r.Name != "Customer")
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

            // Assign the roles to the model roles property
            model.Positions = roles;

            // If model is not null
            if (ModelState.IsValid)
            {
                // Build the employee
                Employee newEmployee = new Employee
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    Forename = model.FirstName,
                    Surname = model.LastName,
                    Salary = model.Salary,
                    EmployementStatus = model.Status,
                    IsActive = true,
                    RegisteredAt = DateTime.Now

                };

                // Create the user, store it in the database and pass the password over to be hashed
                var result = UserManager.Create(newEmployee, model.Password);

                // If user was stored in the database successfully
                if (result.Succeeded)
                {
                    // Then add user to the selected role
                    UserManager.AddToRole(newEmployee.Id, model.Position);

                    // Redirect to the ManageEmployees view.
                    return RedirectToAction("ManageEmployees");
                }
            }

            // If the model state is not valid, return the CreateEmployee view with the model.
            return View(model);
        }

        //**********************************************************************************************************************************************************

        //  MANAGE REGISTERED USERS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Manages registered employees.
        /// </summary>
        /// <param name="redirected">Indicates if the user was redirected due to an unauthorized action.</param>
        /// <returns>The ManageEmployees view with the list of users.</returns>
        public ActionResult ManageEmployees(bool? redirected)
        {
            // Get all the users from the db and store it in a list
            var users = db.Users.OrderBy(u => u.Forename).ToList();

            // If the user was redirected due to an unauthorized action, display an alert message.
            if (redirected == true)
            {
                TempData["AlertMessage"] = "CANNOT DELETE OWN ACCOUNT";
            }

            // Return the ManageEmployees view with the list of users.
            return View(users);
        }

        //public ActionResult ManageCustomers()
        //{
        //    //get all the users from the db and store it in a list
        //    var users = db.Users.OrderBy(u => u.Forename).ToList();

        //    //send the list of employees
        //    return View(users);
        //}

        //**********************************************************************************************************************************************************

        //  CRUD METHODS FOR USERS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Deletes a user based on the provided user ID.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        /// <param name="isCustomer">Indicates if the user is a customer.</param>
        /// <returns>The DeleteUser view if the user exists; otherwise, redirects to the appropriate view.</returns>
        public ActionResult DeleteUser(string userId, bool? isCustomer)
        {
            // Check if the user ID is null or empty.
            if (string.IsNullOrEmpty(userId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Check if the user is trying to delete their own account.
            if (userId == User.Identity.GetUserId())
            {
                // If the user is a customer, redirect to ManageCustomers.
                if (isCustomer == true)
                {
                    return RedirectToAction("ManageCustomers", "Admin");
                }

                // Otherwise, redirect to ManageEmployees with a redirected flag.
                return RedirectToAction("ManageEmployees", "Admin", new { redirected = true });
            }

            // Find the user by ID in the database.
            var user = db.Users.First(u => u.Id == userId);

            // Return the DeleteUser view with the user.
            return View(user);
        }

        /// <summary>
        /// Confirms the deletion of a user.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        /// <returns>Redirects to the appropriate view based on the user type.</returns>
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUserConfirmed(string userId)
        {
            // Ensure the userId is not null or empty
            if (string.IsNullOrEmpty(userId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User ID is invalid.");
            }

            // Find the user by the given ID
            var theUser = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
            var theOrders = db.Orders
                .Where(o => o.User.Id == userId)
                .Include(o => o.Invoice)
                .Include(o => o.ShippingInfo)
                .Include(o => o.OrderLines)
                .Include(o => o.Invoice)
                .Include(o => o.Payment)
                .ToList();

            // If no user is found, return a NotFound result.
            if (theUser == null)
            {
                // If no user is found, return a NotFound result
                return HttpNotFound();
            }

            // Remove the user from the database
            db.Users.Remove(theUser);

            // Remove each order associated with the user
            foreach (var order in theOrders)
            {
                db.Orders.Remove(order);
            }

            // Save changes asynchronously.
            await db.SaveChangesAsync();

            // Check if the user is an employee.
            if (theUser is Employee)
            {
                // Set a confirmation message and redirect to ManageEmployees.
                TempData["ConfirmationMessage"] = $"EMPLOYEE USER: '{theUser.UserName}' HAS BEEN SUCCESSFULLY DELETED";
                return RedirectToAction("ManageEmployees");
            }

            // Set a confirmation message and redirect to ManageCustomers.
            TempData["ConfirmationMessage"] = $"CUSTOMER USER: '{theUser.UserName}' HAS BEEN SUCCESSFULLY DELETED";
            return RedirectToAction("ManageCustomers");
        }

        /// <summary>
        /// Retrieves the details of a user for editing.
        /// </summary>
        /// <param name="userId">The ID of the user to be edited.</param>
        /// <returns>The EditUser view with the user's details.</returns>
        [HttpGet]
        public async Task<ActionResult> EditUser(string userId)
        {
            // If the user ID is null, return a BadRequest result.
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find user by id and store in User
            User user = await UserManager.FindByIdAsync(userId);

            // Get user's current role
            string oldRole = (await UserManager.GetRolesAsync(userId)).Single(); // Only ever a single role

            if (user is Employee employee)
            {
                // Get all the roles from the database and store them as a list of selectedlistitems
                var items = db.Roles
                    .Select(r => new SelectListItem
                    {
                        Text = r.Name,
                        Value = r.Name,
                        Selected = r.Name == oldRole
                    })
                    .Where(i => i.Value != "Customer")
                    .ToList();

                // Build the changeroleviewmodel object including the list of roles
                // and send it to the view displaying the roles in a dropdownlist with the user's current role displayed as selected
                return View("EditUser", new EditUserViewModel
                {
                    UserName = user.UserName,
                    Forename = user.Forename,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Street = user.Street,
                    City = user.City,
                    Postcode = user.Postcode,
                    RegisteredAt = user.RegisteredAt,
                    IsActive = user.IsActive,
                    Role = oldRole,
                    Roles = items,
                    OldRole = oldRole,
                    Salary = employee.Salary,
                    EmployementStatus = employee.EmployementStatus
                });
            }

            // Check if the user is a customer.
            else if (user is Customer customer)
            {
                // Return the EditUser view with the customer's details.
                return View("EditUser", new EditUserViewModel
                {
                    UserName = user.UserName,
                    Forename = user.Forename,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Street = user.Street,
                    City = user.City,
                    Postcode = user.Postcode,
                    RegisteredAt = user.RegisteredAt,
                    IsActive = user.IsActive,
                    OldRole = oldRole,
                    CustomerType = customer.CustomerType
                });
            }
            // If the user is neither an employee nor a customer, return a BadRequest result.
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Confirms the edits made to a user's details.
        /// </summary>
        /// <param name="userId">The ID of the user being edited.</param>
        /// <param name="model">The EditUserViewModel containing the updated details.</param>
        /// <returns>Redirects to the appropriate view based on the user type.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditUser")]
        public async Task<ActionResult> EditUserConfirmed(string userId, EditUserViewModel model)
        {
            try
            {
                string oldRole = null;

                // If roles is null and role exists, we need to get he roles that exist in the db
                if (model.Roles == null && model.Role != null)
                {
                    // Get all the roles from the database and store them as a list of selectedlistitems
                    var items = db.Roles
                        .Select(r => new SelectListItem
                        {
                            Text = r.Name,
                            Value = r.Name,
                            Selected = r.Name == oldRole
                        })
                        .Where(i => i.Value != "Customer")
                        .ToList();
                    model.Roles = items;
                }

                // Ensure the model state is valid.
                if (ModelState.IsValid)
                {
                    // Find the user by ID.
                    User user = await UserManager.FindByIdAsync(userId); //get user by id

                    // Update the user's details with the values from the model.
                    user.UserName = model.UserName;
                    user.Forename = model.Forename;
                    user.Surname = model.Surname;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Street = model.Street;
                    user.City = model.City;
                    user.Postcode = model.Postcode;
                    user.IsActive = model.IsActive;

                    // Update the password if the NewPassword is not null or empty
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var newPasswordHash = UserManager.PasswordHasher.HashPassword(model.NewPassword);
                        user.PasswordHash = newPasswordHash;
                    }

                    // Update the user asynchronously.
                    await UserManager.UpdateAsync(user);

                    // Check if the user is an employee.
                    if (user is Employee employee)
                    {
                        
                        // Get the user's current role.
                        oldRole = (await UserManager.GetRolesAsync(userId)).Single(); //Only ever a single role.

                        //check if the model role is empty(it should never be)
                        if (model.Role == null)
                        {
                            model.Role = oldRole;
                        }

                        // Update the employee-specific details.
                        employee.Salary = model.Salary;
                        employee.EmployementStatus = model.EmployementStatus;

                        // If the role has changed, update the role.
                        if (oldRole != model.Role)
                        {
                            // Remove user from the old role first.
                            await UserManager.RemoveFromRoleAsync(userId, oldRole);
                            // Now we are adding the user to the new role
                            await UserManager.AddToRoleAsync(userId, model.Role);
                        }

                        // Save the changes to the database.
                        db.Users.AddOrUpdate(user);
                        await db.SaveChangesAsync();

                        // Set a confirmation message and redirect to ManageEmployees.
                        TempData["ConfirmationMessage"] = $"EMPLOYEE USER: '{user.UserName}' HAS BEEN SUCCESSFULLY UPDATED";
                        return RedirectToAction("ManageEmployees");
                    }
                    // Check if the user is a customer.
                    else if (user is Customer customer)
                    {
                        // Update the customer-specific details
                        customer.CustomerType = model.CustomerType;

                        // Save the changes to the database.
                        db.Users.AddOrUpdate(user);
                        await db.SaveChangesAsync();

                        // Set a confirmation message and redirect to ManageCustomers.
                        TempData["ConfirmationMessage"] = $"CUSTOMER USER: '{user.UserName}' HAS BEEN SUCCESSFULLY UPDATED";
                        return RedirectToAction("ManageCustomers");
                    }

                }

                // If the model state is not valid, pass the model to the view and return it.
                return View(model);
            }
            catch (Exception ex)
            {
                // If an exception occurs, return the TryCatchError view with the exception.
                return View("TryCatchError", ex);
            }

        }

        /// <summary>
        /// Retrieves the details of a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The UserDetails view with the user's details.</returns>
        public ActionResult UserDetails(string userId)
        {
            // If the user ID is null, return a BadRequest result.
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find user by id and store in User
            User user = UserManager.FindById(userId);

            // Return the UserDetails view with the user's details.
            return View(user);
        }
    }
}