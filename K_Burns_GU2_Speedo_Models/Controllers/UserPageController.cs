using K_Burns_GU2_Speedo_Models.CustomAttributes;
using K_Burns_GU2_Speedo_Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using Stripe.Reporting;
using System.Web.Services.Description;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Name: Kieran Burns
    /// Date Finalised: 24/05/2024
    /// Class Name: UserPageController
    /// </summary>

    /// <summary>
    /// This class holds all the dashboard/backend functionality for employees and customers.
    /// REGISTERED USERS HAVE ACCESS TO THIS SHARED PAGE.
    /// </summary>

    [RedirectUnauthorizedUsersFilter]

    public class UserPageController : AccountController
    {
        // Declare global private DBContext object
        private SpeedoModelsDbContext db = new SpeedoModelsDbContext();

        /// <summary>
        /// creating constructor 0 args, calling parent constructor.
        /// </summary>
        public UserPageController() : base() { }

        /// <summary>
        /// creating constructor 2 args, calling parent constructor.
        /// </summary>
        public UserPageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) : base(userManager, signInManager) { }

        //***********   For CUSTOMERS ONLY  *************

        /// <summary>
        /// All the Custom Attribute filters in this Controller redirects user to the home/index if they are logged in as the mentioned roles type.
        /// </summary>

        /// <summary>
        /// The MyPlacedOrders gets user by id and gets the orders (as a list) from the database, where the userid matches in the db.
        /// We then send the orders list to the view.
        /// </summary>
        /// /// <returns>The view with the user's placed orders.</returns>
        [AuthorizeRedirect(Roles = "Customer", RedirectUrl = "/Home/Index")]
        public ActionResult MyPlacedOrders()
        {
            // Get the current users ID
            string userId = User.Identity.GetUserId();

            // Get order that belongs to specific user Id from the db
            var orders = db.Orders
                .Where(o => o.User.Id == userId)
                .Include(o => o.ShippingInfo)
                .ToList();

            // Throw orders object to the view
            return View(orders);
        }

        //**********************************************************************************************************************************************************

        //  SHARED EMPLOYEE DASHBOARDS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The DashboardIndex gets the logged in user and their role,
        /// as well as getting a list of all the users and their orders, then sends them to the view.
        /// </summary>
        ///  /// <returns>The dashboard view with user information and orders.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Assistant Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public async Task <ActionResult> DashboardIndex()
        {
            // Get the current user by ID
            string userId = User.Identity.GetUserId();

            // Get current user
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Get current user role
            // Store it in a viewbag
            // To be thrown to the front end
            ViewBag.RoleName = user.CurrentRole;

            // Get a list of all the users
            var users = await db.Users
                       .Include(u => u.Orders) // Ensure you have included Orders in your query
                       .OrderBy(u => u.Forename)
                       .ToListAsync();

            // Pass the users to the view
            return View(users);
        }

        /// <summary>
        /// The AdvancedDashboardIndex gets the logged in user and their role,
        /// as well as getting a list of all the users and their orders info,
        /// we also get a count for all the orders, products, users and invoices that exist in the system to display as part of the generating report functionality.
        /// We send the user, orders and report information to the view.
        /// </summary>
        /// <returns>The advanced dashboard view with user, order, and report information.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public async Task<ActionResult> AdvancedDashboardIndex()
        {
            // Get user by ID
            string userId = User.Identity.GetUserId();

            // Get current user
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Get all the existing objects from the db
            var orders = await db.Orders
                .Include(o => o.Payment)
                .ToListAsync();
            var products = await db.Products.ToListAsync();
            var users = await db.Users.ToListAsync();
            var invoices = await db.Invoices.ToListAsync();

            // Get the count for each list
            var ordersCount = orders.Count;
            var productsCount = products.Count;
            var usersCount = users.Count;
            var invoicesCount = invoices.Count;

            // Get all users and there orders
            // Orders by the users username
            var viewModels = await db.Users
                       .Include(u => u.Orders) // Ensure you have included Orders in your query
                       .OrderBy(u => u.Forename)
                       .Select
                       (u => new AdvancedDashboardViewModels
                       {
                           User = u,
                           OrdersCount = ordersCount,
                           ProductsCount = productsCount,
                           UsersCount = usersCount,
                           InvoicesCount = invoicesCount
                       }).ToListAsync();

            // Get current user role
            // Store it in a viewbag
            // To be thrown to the front end
            ViewBag.RoleName = user.CurrentRole;

            // Send the list of users to the Index view
            return View(viewModels);
        }

        //**********************************************************************************************************************************************************

        //  MANAGE REGISTERED USERS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The ManageCustomers action gets all users from the database and sends them to the view.
        /// </summary>
        /// <returns>The view with a list of all users.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        public ActionResult ManageCustomers()
        {
            // Get all the users from the db and store it in a list
            var users = db.Users.OrderBy(u => u.Forename).ToList();

            // Send the list of employees
            return View(users);
        }

        //**********************************************************************************************************************************************************

        //  CRUD METHODS FOR USERS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The DeleteUser action method gets a user by ID and presents a confirmation/detail view.
        /// If the user ID is incorrect or the user attempts to delete themselves, defined redirects are executed.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="isCustomer">Indicates if the user is a customer.</param>
        /// <returns>The view with user details for confirmation.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant", RedirectUrl = "/Home/Index")]
        public ActionResult DeleteUser(string userId, bool? isCustomer)
        {
            // Check if the userId is null or empty
            if (userId == null || userId == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Prevent users from deleting themselves
            if (userId == User.Identity.GetUserId())
            {
                if (isCustomer == true)
                {
                    return RedirectToAction("ManageCustomers", "Admin");
                }
                return RedirectToAction("ManageEmployees", "Admin", new { redirected = true });
            }

            // Get the user by ID
            var user = db.Users.First(u => u.Id == userId);

            // Return the user to the view for confirmation
            return View(user);
        }

        /// <summary>
        /// The DeleteUserConfirmed action method handles the actual deletion of a user.
        /// It gets the user and their orders, deletes them from the database, and redirects to the appropriate management page, depedening on user type (customer or employee).
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>The appropriate management view after deletion.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant", RedirectUrl = "/Home/Index")]
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
                .Include(o => o.Parcel)
                .ToList();

            // If no user is found, return a NotFound result
            if (theUser == null)
            {
                return HttpNotFound();
            }

            // Remove the user from the database
            db.Users.Remove(theUser);

            // Remove each order associated with the user
            foreach (var order in theOrders)
            {
                db.Orders.Remove(order);
            }
            await db.SaveChangesAsync();  // Save changes asynchronously

            // Check if the user is an employee and set a confirmation message
            if (theUser is Employee)
            {
                TempData["ConfirmationMessage"] = $"EMPLOYEE USER: '{theUser.UserName}' HAS BEEN SUCCESSFULLY DELETED";
                return RedirectToAction("ManageEmployees");
            }

            // Otherwise set a confirmation message for customer deletion
            TempData["ConfirmationMessage"] = $"CUSTOMER USER: '{theUser.UserName}' HAS BEEN SUCCESSFULLY DELETED";
            return RedirectToAction("ManageCustomers");
        }

        /// <summary>
        /// The EditUser action gets a user by ID and presents a view for editing their details.
        /// It differentiates between employees and customers, providing relevant information and roles.
        /// </summary>
        /// <param name="userId">The ID of the user to edit.</param>
        /// <returns>The view with user details for editing.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public async Task<ActionResult> EditUser(string userId)
        {
            // If the ID passed from the front end is empty, return a bad request
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find user by ID and store in User
            User user = await UserManager.FindByIdAsync(userId);
            // Get user's current role
            string oldRole = (await UserManager.GetRolesAsync(userId)).Single(); //only ever a single role

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
                // Send it to the view displaying the roles in a dropdownlist with the user's current role displayed as selected
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
                    Roles = items,
                    OldRole = oldRole,
                    Salary = employee.Salary,
                    EmployementStatus = employee.EmployementStatus
                });
            }
            else if (user is Customer customer)
            {
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

            // If all else false we return a bad request page
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// The EditUserConfirmed action handles the updating of a user's details based on the provided model.
        /// It updates user information, including roles and specific details for employees and customers.
        /// </summary>
        /// <param name="userId">The ID of the user to edit.</param>
        /// <param name="model">The model containing the updated user details.</param>
        /// <returns>The appropriate management view after editing.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditUser")]
        public async Task<ActionResult> EditUserConfirmed(string userId, EditUserViewModel model)
        {
            try
            {
                string oldRole = null;

                if (ModelState.IsValid)
                {
                    // Get user by ID
                    User user = await UserManager.FindByIdAsync(userId);

                    // Update user properties with the values from the model
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

                    // Upadate user
                    await UserManager.UpdateAsync(user);

                    // If the user is an employee
                    if (user is Employee employee)
                    {

                        oldRole = (await UserManager.GetRolesAsync(userId)).Single(); //Only ever a single role.

                        // Update employee according to the model information
                        employee.Salary = model.Salary;
                        employee.EmployementStatus = model.EmployementStatus;

                        if (oldRole != model.Role)
                        {
                            //Remove user from the old role first.
                            await UserManager.RemoveFromRoleAsync(userId, oldRole);
                            //now we are adding the user to the new role
                            await UserManager.AddToRoleAsync(userId, model.Role);
                        }

                        // Update user information
                        db.Users.AddOrUpdate(user);

                        // Save changes to the databse
                        await db.SaveChangesAsync();

                        // Store confirmation message in a TempData to be accessed in the view
                        TempData["ConfirmationMessage"] = $"EMPLOYEE USER: '{user.UserName}' HAS BEEN SUCCESSFULLY UPDATED";

                        // Redirect to Manage Employees view
                        return RedirectToAction("ManageEmployees");
                    }
                    // Otherwise user is a customer
                    else if (user is Customer customer)
                    {
                        // Update customer according to the model information
                        customer.CustomerType = model.CustomerType;

                        // Update user information
                        db.Users.AddOrUpdate(user);

                        // Save changes to the databse
                        await db.SaveChangesAsync();

                        // Store confirmation message in a TempData to be accessed in the view
                        TempData["ConfirmationMessage"] = $"CUSTOMER USER: '{user.UserName}' HAS BEEN SUCCESSFULLY UPDATED";

                        // Redirect to Manage Customers view
                        return RedirectToAction("ManageCustomers");
                    }

                }

                // Otherwise if Model state is not valid, we return the model
                return View(model);
            }
            // If all else fails, we catch an exception
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }

        }

        /// <summary>
        /// The UserDetails action gets and displays the details of a user based on the provided ID.
        /// </summary>
        /// <param name="userId">The ID of the user to display details for.</param>
        /// <returns>The view with user details.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        public ActionResult UserDetails(string userId)
        {
            // If the ID being passed from the front end is empty
            // Return a bad request
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find user by ID and store in User
            User user = UserManager.FindById(userId);

            // Throw the user to the view
            return View(user);
        }

        //**********************************************************************************************************************************************************

        //  MANAGE PRODUCTS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The RecordProduct action gets a new product model and the categories from the database, and sends them to the view for product creation.
        /// </summary>
        /// <returns>The view for recording a new product.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult RecordProduct()
        {
            // Declare a product object of type ProductViewModel
            ProductViewModel product = new ProductViewModel();

            // Get all the categories from the db and store them as a SelectedListItem
            // so that we can display the categories in a dropdownlist
            var categories = db.Categories
                .Select(c => new SelectListItem
                {
                    Text = c.CategoryName,
                    Value = c.CategoryName
                }).ToList();

            // Assign the categories to the product categories property
            product.Categories = categories;

            // Send the create product model to the view
            return View(product);
        }

        /// <summary>
        /// The RecordProduct action gets the product details from the form, validates and saves the product to the database, and sends a confirmation message.
        /// </summary>
        /// <param name="product">The product details from the form.</param>
        /// <returns>The appropriate view after recording the product.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RecordProduct(ProductViewModel product)
        {

            // Ensure it is not null
            // Get all the categories from the db and store them as a SelectedListItem
            // so that we can display the categories in a dropdownlist
            var categories = db.Categories
                .Select(c => new SelectListItem
                {
                    Text = c.CategoryName,
                    Value = c.CategoryName
                }).ToList();

            // Get the category by name
            Category category = db.Categories.Where(c => c.CategoryName == product.Category).First();

            // Assign the categories to the product categories property
            product.Categories = categories;

            // If model is not null
            if (ModelState.IsValid)
            {
                // Build the product
                Product newProduct = new Product
                {
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    StockLevel = product.StockLevel,
                    ImageUrl = "/Images/Products/New_Product.png",
                    OnSale = false,
                    Discontinued = false,
                    DateCreated = DateTime.Now,
                    ProductSize = product.ProductSize,
                    Category = category
                };

                // Set OnSale if there is a sale difference
                if (product.SaleDifference > 0)
                {
                    newProduct.OnSale = true;
                }

                // Add the new product to the database
                db.Products.Add(newProduct);
                db.SaveChanges();

                // Redirect to ManageProduct action
                return RedirectToAction("ManageProduct", "UserPage");
            }

            // Otherwise Return the view with the product model
            return View(product);
        }

        /// <summary>
        /// The ManageProduct action gets all the products from the database and sends them to the view.
        /// </summary>
        /// <returns>The view with all products.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ManageProduct()
        {
            // Get all the products from the db
            var products = db.Products
                .Include(p => p.Category)
                .ToList()
                .OrderByDescending(p => p.DateCreated);

            // If products are null, return a bad request
            if (products == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the products to the view
            return View(products);
        }

        /// <summary>
        /// The ProductDetails action gets the product details from the database and sends them to the view.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The view with product details.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ProductDetails(int? productId)
        {
            // Get the product and its category from the db
            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == productId);

            // If product is null, return a bad request
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the product to the view
            return View(product);
        }

        /// <summary>
        /// The DeleteProduct action gets the product details for confirmation of deletion.
        /// </summary>
        /// <param name="productId">The ID of the product to delete.</param>
        /// <returns>The view with product details for confirmation.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult DeleteProduct(int? productId)
        {
            // Get the product and its category from the db
            Product product = db.Products
                .Include(p => p.Category)
                .First(p => p.ProductId == productId);

            // If product is null, return a bad request
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the product to the view
            return View(product);
        }

        /// <summary>
        /// The DeletedProduct action handles the deletion of a product from the database.
        /// </summary>
        /// <param name="productId">The ID of the product to delete.</param>
        /// <returns>The appropriate view after deletion.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Stock Control Manager,Sales Assistant,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteProduct")]
        public async Task<ActionResult> DeletedProduct(int? productId)
        {

            // Get the product and its category from the db
            Product product = await db.Products
                .Include(p => p.Category)
                .Include(p => p.BasketItems)
                .FirstAsync(p => p.ProductId == productId);

            // Ensure the product is not null
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Product cannot be found.");
            }

            // Set the IsDeleted flag to true
            product.IsDeleted = true;
            await db.SaveChangesAsync();

            // Set a confirmation message
            TempData["ConfirmationMessage"] = "Product: " + product.ProductName + ", Has Successfully Been Deleted!";

            // Redirect to ManageProduct action
            return RedirectToAction("ManageProduct", "UserPage");
        }

        /// <summary>
        /// The EditProduct action gets the product details for editing, including the categories, and sends them to the view.
        /// </summary>
        /// <param name="productId">The ID of the product to edit.</param>
        /// <returns>The view with product details for editing.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Stock Control Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult EditProduct(int productId)
        {
            // Fefault sales difference price
            // If it is not on sale
            int salesDifference = 0;

            // Get selected product from the db
            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == productId);

            // Get category name
            string categoryName = product.Category.CategoryName;

            // Get categories as a selectlist item
            // and land on the category name of the current product
            var categories = db.Categories
                   .Select(c => new SelectListItem
                   {
                       Text = c.CategoryName,
                       Value = c.CategoryName,
                       Selected = c.CategoryName == categoryName
                   })
                   .ToList();

            // Change the sales difference price if the product is on sale
            if (product.OnSale)
            {
                salesDifference = DiscountPercentage(product.Price, product.DiscountPrice);
            }

            // Create and send the product view model to the view
            return View(new ProductViewModel
            {
                ProductId = productId,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                Price = product.Price,
                StockLevel = product.StockLevel,
                ImageUrl = product.ImageUrl,
                OnSale = product.OnSale,
                Discontinued = product.Discontinued,
                ProductSize = product.ProductSize,
                SaleDifference = salesDifference,
                Category = product.Category.CategoryName,
                Categories = categories
            });
        }

        /// <summary>
        /// The EditProduct action handles the updating of a product's details and saves the changes to the database.
        /// </summary>
        /// <param name="product">The product details from the form.</param>
        /// <returns>The appropriate view after editing the product.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Stock Control Manager", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct(ProductViewModel product)
        {
            // Ensure it is not null
            // Get all the categories from the db and store them as a SelectedListItem
            // so that we can display the categories in a dropdownlist
            var categories = await db.Categories
                .Select(c => new SelectListItem
                {
                    Text = c.CategoryName,
                    Value = c.CategoryName
                }).ToListAsync();

            // Get the category by name
            Category category = await db.Categories.Where(c => c.CategoryName == product.Category).FirstAsync();

            // Assign the categories to the product categories property
            product.Categories = categories;

            // If model is valid
            if (ModelState.IsValid)
            {
                // Get the product from the database
                Product theProduct = await db.Products.FirstAsync(p => p.ProductId == product.ProductId);

                // Update product properties
                theProduct.ProductName = product.ProductName;
                theProduct.ProductDescription = product.ProductDescription;
                theProduct.Price = product.Price;
                theProduct.DiscountPrice = product.DiscountPrice;
                theProduct.StockLevel = product.StockLevel;
                theProduct.ImageUrl = product.ImageUrl;
                theProduct.OnSale = false;
                theProduct.Discontinued = product.Discontinued;
                theProduct.ProductSize = product.ProductSize;
                theProduct.Category = category;

                // Set OnSale if there is a sale difference
                if (product.SaleDifference > 0)
                {
                    theProduct.OnSale = true;
                }

                // Add or update the product in the database
                db.Products.AddOrUpdate(theProduct);
                await db.SaveChangesAsync();

                // Set a confirmation message
                TempData["ConfirmationMessage"] = "Product: " + product.ProductName + ", Is Successfully Changed!";

                // Redirect to ManageProduct action
                return RedirectToAction("ManageProduct", "UserPage");

            }

            // Otherwise return the view with the product model
            return View(product);
        }

        //**********************************************************************************************************************************************************

        //  MANAGE ORDERS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The PlacedOrders action gets all the orders along with the associated users and shipping information from the database, and sends them to the view.
        /// </summary>
        /// <returns>The view with all placed orders.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Sales Assistant,Stock Control Manager,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        public ActionResult PlacedOrders()
        {
            // Get all the orders and the user/users it belongs too
            // from the db and store it in a list
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingInfo)
                .ToList();

            // Send the list of orders to the view
            return View(orders);
        }

        /// <summary>
        /// The DetailsOrder action gets the details of a specific order by ID, including the user, orderlines, product, and category information, and sends them to the view.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <returns>The view with order details.</returns>
        [AuthorizeRedirect(Roles = "Invoice Clerk,Admin,Assistant Manager,Sales Manager,Sales Assistant,Stock Control Manager,Warehouse Assistant,Customer", RedirectUrl = "/Home/Index")]
        public ActionResult DetailsOrder(int? orderId)
        {
            // Get all the orders and the user/users it belongs too
            // from the db and store it in a list
            Order order = db.Orders
                .Include(o => o.User)
                .Include("OrderLines.Product.Category")
                .FirstOrDefault(o => o.OrderId == orderId);

            // If order is null, return a bad request
            if (order == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the order to the view
            return View(order);
        }

        /// <summary>
        /// The AmendOrder action gets the details of a specific order by ID for editing, including the user, orderlines, product, and category information, and sends them to the view.
        /// </summary>
        /// <param name="orderId">The ID of the order to amend.</param>
        /// <returns>The view with order details for editing.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant,Customer", RedirectUrl = "/Home/Index")]
        public ActionResult AmendOrder(int? orderId)
        {

            // Get all the orders and the user/users it belongs too
            // from the db and store it in a list
            Order order = db.Orders
                .Include(o => o.User)
                .Include("OrderLines.Product.Category")
                .FirstOrDefault(o => o.OrderId == orderId);

            // If order is null, return a bad request
            if (order == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the order to the view
            return View(order);
        }

        /// <summary>
        /// The AmendedOrder action handles the update of a specific order, including changes to orderlines and recalculating totals, and saves the changes to the database.
        /// </summary>
        /// <param name="order">The order details from the form.</param>
        /// <param name="selectedOrderLineId">The ID of the selected order line to amend.</param>
        /// <returns>The appropriate view after editing the order.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant,Customer", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("AmendOrder")]
        public async Task<ActionResult> AmendedOrder(Order order, string selectedOrderLineId)
        {
            // Ensure the selectedOrderLineId is not null or empty
            if (string.IsNullOrEmpty(selectedOrderLineId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Parse the order line ID from the selectedOrderLineId
            int orderLineId = Int32.Parse(selectedOrderLineId);

            // Get the quantity from the orderlines in the provided order object
            int quantity = order.OrderLines.Where(ol => ol.OrderLineId == orderLineId).FirstOrDefault().Quantity;

            // Get the order and its associated details from the db
            order = await db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingInfo)
                .Include("OrderLines.Product.Category")
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            // Get the old quantity from the order lines in the database
            int oldQuantity = order.OrderLines.Where(ol => ol.OrderLineId == orderLineId).FirstOrDefault().Quantity;

            // If the new quantity is the same as the old quantity, return the view
            if (quantity == oldQuantity)
            {
                return View(order);
            }

            // If the new quantity is less than or equal to 0, return an error message and the view
            if (quantity <= 0)
            {
                TempData["AlertMessage"] = "Quantity must be greater than 0";
                return View(order);
            }

            // Declare int to be calculated later
            int quantityDifference = 0;

            // Update the orderline quantity in the order object
            order.OrderLines.Where(ol => ol.OrderLineId == orderLineId).FirstOrDefault().Quantity = quantity;

            // Get the orderline and its product details from the database
            OrderLine orderLine = db.OrderLines
                        .Include(ol => ol.Product.Category)
                        .FirstOrDefault(ol => ol.OrderLineId == orderLineId);

            // Check if the new quantity is less than the old quantity
            if (quantity < oldQuantity)
            {
                // Calculate the difference between old and new quantity
                quantityDifference = oldQuantity - quantity;

                // Increase the stock level by the quantity difference
                orderLine.Product.StockLevel += quantityDifference;
            }
            else
            {
                // Calculate the difference between new and old quantity
                quantityDifference = quantity - oldQuantity;

                // Decrease the stock level by the quantity difference
                orderLine.Product.StockLevel -= quantityDifference;

                // Check if the new stock level is below 0
                if (orderLine.Product.StockLevel < 0)
                {
                    // Revert the quantity to the old quantity if stock is insufficient
                    order.OrderLines.Where(ol => ol.OrderLineId == orderLineId).FirstOrDefault().Quantity = oldQuantity;

                    // Set an alert message indicating insufficient stock level
                    TempData["AlertMessage"] = "Stock Level not sufficient";

                    // Return the order view with the alert message
                    return View(order);
                }
            }
            // Update the order total and orderline totals
            order.OrderTotal -= orderLine.LineTotal;
            order.OrderLines.Remove(orderLine);

            // Check if Product is on sale
            if (orderLine.Product.OnSale)
            {
                // If so we multiply quantity with Discount price
                orderLine.LineTotal = orderLine.Quantity * orderLine.Product.DiscountPrice;
            }
            else
            {
                // otherwise multiply quantity with price
                orderLine.LineTotal = orderLine.Quantity * orderLine.Product.Price;
            }

            // Update the order total, the total amount and orderline
            order.OrderTotal += orderLine.LineTotal;
            order.TotalAmount = order.OrderTotal + order.ShippingInfo.ShippingCost;
            order.OrderLines.Add(orderLine);

            // Save the updated order to the database
            db.Orders.AddOrUpdate(order);
            await db.SaveChangesAsync();

            // Redirect to the AmendOrder action with the updated order ID
            return RedirectToAction("AmendOrder", new { orderId = order.OrderId });
        }

        /// <summary>
        /// The RemoveOrder action gets the details of a specific order by ID for confirmation of deletion, including the user, order lines, product, and category information, and sends them to the view.
        /// </summary>
        /// <param name="orderId">The ID of the order to remove.</param>
        /// <returns>The view with order details for confirmation.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant,Customer", RedirectUrl = "/Home/Index")]
        public ActionResult RemoveOrder(int? orderId)
        {
            // Get the order and its associated user, orderlines, products, and categories from the db
            Order order = db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingInfo)
                .Include("OrderLines.Product.Category")
                .FirstOrDefault(o => o.OrderId == orderId);

            // If order is null, return a bad request
            if (order == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Return the order to the view
            return View(order);
        }

        /// <summary>
        /// The OrderRemoved action handles the deletion of a specific order, updates the stock levels, and removes the order from the database.
        /// </summary>
        /// <param name="orderId">The ID of the order to remove.</param>
        /// <returns>The appropriate view after deleting the order.</returns>
        [AuthorizeRedirect(Roles = "Admin,Assistant Manager,Sales Manager,Sales Assistant,Customer", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("RemoveOrder")]
        public async Task<ActionResult> OrderRemoved(string orderId)
        {
            // Parse the order ID from the provided string
            int id = Int32.Parse(orderId);

            // Get the order and its associated details from the db
            Order order = await db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingInfo)
                .Include("OrderLines.Product.Category")
                .Include(o => o.Invoice)
                .Include(o => o.Payment)
                .Include(o => o.Parcel)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            // Update the stock levels for each orderline in the order
            foreach (var orderLine in order.OrderLines)
            {
                orderLine.Product.StockLevel += orderLine.Quantity;
            }

            // Remove the order from the database
            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            // Redirect to the appropriate view based on the user's role
            if (User.IsInRole("Customer"))
            {
                return RedirectToAction("MyPlacedOrders");
            }
                
            return RedirectToAction("PlacedOrders");
        }

        //**********************************************************************************************************************************************************

        //  GENERATING REPORTS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// The OrdersReport action gets all orders along with their associated users from the database, and sends them to the view.
        /// </summary>
        /// <returns>The view with all orders for the report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult OrdersReport()
        {
            // Get all orders and the associated users from the database
            var orders = db.Orders
                .Include(o => o.User)
                .ToList();

            // Return the orders to the view
            return View(orders);
        }

        /// <summary>
        /// The InvoicesReport action gets all invoices along with their associated orders and users from the database, and sends them to the view.
        /// </summary>
        /// <returns>The view with all invoices for the report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult InvoicesReport()
        {
            // Get all invoices and the associated orders and users from the database
            var invoices = db.Invoices
                .Include("Order.User")
                .ToList();

            // Return the invoices to the view
            return View(invoices);
        }

        /// <summary>
        /// The ProductsReport action gets all products from the database and sends them to the view.
        /// </summary>
        /// <returns>The view with all products for the report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ProductsReport()
        {
            // Get all products from the database
            var products = db.Products
                .ToList();

            // Return the products to the view
            return View(products);
        }

        /// <summary>
        /// The UsersReport action gets all users from the database and sends them to the view.
        /// </summary>
        /// <returns>The view with all users for the report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult UsersReport()
        {
            // Get all users from the database
            var users = db.Users
                .ToList();

            // Return the users to the view
            return View(users);
        }

        /// <summary>
        /// The ByDay action gets the specified report (Products, Orders, Invoices, Users) for the day and sends it to the view.
        /// </summary>
        /// <param name="reportType">The type of report to generate (Products, Orders, Invoices, Users).</param>
        /// <returns>The view with the daily report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ByDay(string reportType)
        {
            // Switch statement to determine which report to get
            switch (reportType)
            {
                case "Products":
                    return View("ReportByDay", GetProductsReport(reportType));
                case "Orders":
                    return View("ReportByDay", GetOrdersReport(reportType));
                case "Invoices":
                    return View("ReportByDay", GetInvoicesReport(reportType));
                case "Users":
                    return View("ReportByDay", GetUsersReport(reportType));
            }

            // Return a bad request if the report type is invalid
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// The ByWeek action gets the specified report (Products, Orders, Invoices, Users) for the week and sends it to the view.
        /// </summary>
        /// <param name="reportType">The type of report to generate (Products, Orders, Invoices, Users).</param>
        /// <returns>The view with the weekly report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ByWeek(string reportType)
        {
            // Switch statement to determine which report to get
            switch (reportType)
            {
                case "Products":
                    return View("ReportByWeek", GetProductsReport(reportType));
                case "Orders":
                    return View("ReportByWeek", GetOrdersReport(reportType));
                case "Invoices":
                    return View("ReportByWeek", GetInvoicesReport(reportType));
                case "Users":
                    return View("ReportByWeek", GetUsersReport(reportType));
            }

            // Return a bad request if the report type is invalid
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// The ByMonth action gets the specified report (Products, Orders, Invoices, Users) for the month and sends it to the view.
        /// </summary>
        /// <param name="reportType">The type of report to generate (Products, Orders, Invoices, Users).</param>
        /// <returns>The view with the monthly report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ByMonth(string reportType)
        {
            // Switch statement to determine which report to get
            switch (reportType)
            {
                case "Products":
                    return View("ReportByMonth", GetProductsReport(reportType));
                case "Orders":
                    return View("ReportByMonth", GetOrdersReport(reportType));
                case "Invoices":
                    return View("ReportByMonth", GetInvoicesReport(reportType));
                case "Users":
                    return View("ReportByMonth", GetUsersReport(reportType));
            }

            // Return a bad request if the report type is invalid
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        //****************  REGULAR METHODS TO DEAL WITH REPORTS   *********************

        /// <summary>
        /// Gets the products report for the specified report type.
        /// </summary>
        /// <param name="reportType">The type of the report to generate.</param>
        /// <returns>A ReportsViewModel containing the products report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        public ReportsViewModel GetProductsReport(string reportType)
        {
            // Get all products from the database
            var products = db.Products
                .ToList();

            // Create a ReportsViewModel object with the report type and products list
            ReportsViewModel reports = new ReportsViewModel
            {
                ReportType = reportType,
                Products = products
            };

            // Return the reports view model
            return reports;
        }

        /// <summary>
        /// Gets the orders report for the specified report type.
        /// </summary>
        /// <param name="reportType">The type of the report to generate.</param>
        /// <returns>A ReportsViewModel containing the orders report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        public ReportsViewModel GetOrdersReport(string reportType)
        {
            // Get all orders and their associated users from the database
            var orders = db.Orders
                .Include(o => o.User)
                .ToList();

            // Create a ReportsViewModel object with the report type and orders list
            ReportsViewModel reports = new ReportsViewModel
            {
                ReportType = reportType,
                Orders = orders
            };

            // Return the reports view model
            return reports;
        }

        /// <summary>
        /// Gets the invoices report for the specified report type.
        /// </summary>
        /// <param name="reportType">The type of the report to generate.</param>
        /// <returns>A ReportsViewModel containing the invoices report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        public ReportsViewModel GetInvoicesReport(string reportType)
        {
            // Get all invoices and their associated orders and users from the database
            var invoices = db.Invoices
                .Include("Order.User")
                .ToList();

            // Create a ReportsViewModel object with the report type and invoices list
            ReportsViewModel reports = new ReportsViewModel
            {
                ReportType = reportType,
                Invoices = invoices
            };

            // Return the reports view model
            return reports;
        }

        /// <summary>
        /// Gets the users report for the specified report type.
        /// </summary>
        /// <param name="reportType">The type of the report to generate.</param>
        /// <returns>A ReportsViewModel containing the users report.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Manager", RedirectUrl = "/Home/Index")]
        public ReportsViewModel GetUsersReport(string reportType)
        {
            // Get all users from the database
            var users = db.Users
                .ToList();

            // Create a ReportsViewModel object with the report type and users list
            ReportsViewModel reports = new ReportsViewModel
            {
                ReportType = reportType,
                Users = users
            };

            // Return the reports view model
            return reports;
        }

        //**********************************************************************************************************************************************************

        //  INVOICE CLERK
        //**********************************************************************************************************************************************************

        //**********************************************************************************************************************************************************

        //  CRUD METHODS FOR INVOICE
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Displays the create invoice page.
        /// </summary>
        /// <returns>The create invoice view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult CreateInvoice()
        {
            // Create invoice view model to be thrown to the view
            InvoiceViewModel invoiceViewModel = new InvoiceViewModel();

            // Pass the invoiceViewModel to the view
            return View(invoiceViewModel);
        }

        /// <summary>
        /// Processes the creation of a new invoice.
        /// </summary>
        /// <param name="invoiceViewModel">The invoice view model containing the invoice details.</param>
        /// <returns>The manage invoices view if successful, otherwise redisplays the create invoice view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateInvoice(InvoiceViewModel invoiceViewModel)
        {
            // Get order from the database and all the navigational properties needed
            Order order = await db.Orders
                .Include("OrderLines.Product")
                .Where(o => o.User.Email == invoiceViewModel.User.Email)
                .Where(o => o.User.UserName == invoiceViewModel.User.UserName)
                .Include(o => o.User)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.OrderId == invoiceViewModel.Order.OrderId);

            // If order is empty
            if (order == null)
            {
                // Send a warning message to user
                TempData["AlertMessage"] = "Check both Order Id and User email/username exists in the database";
                return View(invoiceViewModel);
            }

            // Change attributes of order to match users input
            order.User.Forename = invoiceViewModel.User.Forename;
            order.User.Surname = invoiceViewModel.User.Surname;
            order.User.PhoneNumber = invoiceViewModel.User.Forename;

            order.ShippingInfo.ShippingStreet = invoiceViewModel.Shipping.ShippingStreet;
            order.ShippingInfo.ShippingCity = invoiceViewModel.Shipping.ShippingCity;
            order.ShippingInfo.ShippingPostcode = invoiceViewModel.Shipping.ShippingPostcode;


            // Getting existing invoice from db (if there are any)
            Invoice invoiceCheck = await db.Invoices
                .Where(i => i.Order.OrderId == invoiceViewModel.Order.OrderId)
                .FirstOrDefaultAsync();

            // If empty then the invoice already exists in the db
            if (invoiceCheck != null)
            {
                // Update the Alert Message Temp Data
                TempData["AlertMessage"] = "Invoice " + invoiceCheck.InvoiceId + " already exists";
                // Redirect user to the manage invoices page
                return RedirectToAction("ManageInvoices");
            }

            // If the shipping info is does not match the users home address
            if (order.ShippingInfo.ShippingStreet != order.User.Street
                && order.ShippingInfo.ShippingPostcode != order.User.Postcode
                && order.ShippingInfo.ShippingCity != order.User.City)
            {
                // Turn shippingtohomeaddress boolean to false
                order.ShippingInfo.ShippingToHomeAddress = false;
            }

            // Declare invoice date to now
            // Assign order to our invoice view model object
            invoiceViewModel.Invoice.InvoiceDate = DateTime.Now;
            invoiceViewModel.Invoice.Order = order;

            // Assign invoice view model invoice, to new invoice object
            Invoice invoice = invoiceViewModel.Invoice;

            // Add to the db & save changes
            db.Invoices.AddOrUpdate(invoice);
            db.Orders.AddOrUpdate(order);
            await db.SaveChangesAsync();

            // Set confirmation message
            TempData["ConfirmationMessage"] = "Invoice ID " + invoice.InvoiceId + " has been successfully created";

            // Redirect user to the manage invoices page
            return RedirectToAction("ManageInvoices");
        }

        /// <summary>
        /// Displays the manage invoices page.
        /// </summary>
        /// <returns>The manage invoices view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ManageInvoices()
        {
            // Load all the invoices from the db
            var invoices = db.Invoices
                .Include("Order.OrderLines")
                .Include(i => i.Order.User)
                .Include(i => i.Order.ShippingInfo)
                .ToList();

            // If the invoice is empty
            if (invoices == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Pass the invoices object to the view
            return View(invoices);
        }

        /// <summary>
        /// Displays the delete invoice page for a specific invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to delete.</param>
        /// <returns>The delete invoice view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult DeleteInvoice(int? invoiceId)
        {
            // Get selected invoice by ID
            Invoice invoice = db.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include("Order.OrderLines.Product")
                .Include(i => i.Order.User)
                .Include(i => i.Order.ShippingInfo)
                .First();

            // If the invoice is empty
            if (invoice == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Create a list of orderline items for the view model
            var items = db.OrderLines
                .Where(i => i.Order.Invoice.InvoiceId == invoiceId)
                .Select(r => new SelectListItem
                {
                    Text = r.Product.ProductName + " x" + r.Quantity,
                    Value = r.Product.ProductName,
                })
                .ToList();

            // Create invoice view model to be thrown to the view
            InvoiceViewModel invoiceViewModel = new InvoiceViewModel
            {
                Invoice = invoice,
                Order = invoice.Order,
                User = invoice.Order.User,
                Shipping = invoice.Order.ShippingInfo,
                OrderLines = invoice.Order.OrderLines,
                OrderLineItems = items
            };

            // Pass the invoiceViewModel to the view
            return View(invoiceViewModel);
        }

        /// <summary>
        /// Processes the deletion of a specific invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to delete.</param>
        /// <returns>The manage invoices view after deletion.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpPost, ActionName("DeleteInvoice")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletedInvoice(int? invoiceId)
        {
            // Get selected invoice by ID
            Invoice invoice = await db.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include("Order.OrderLines.Product")
                .Include(i => i.Order.User)
                .Include(i => i.Order.ShippingInfo)
                .FirstAsync();

            // If the invoice is empty
            if (invoice == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Remove the user from the database
            db.Invoices.Remove(invoice);

            // Save changes to the database
            await db.SaveChangesAsync();

            // Redirect user to the manage invoices page
            return RedirectToAction("ManageInvoices");
        }

        /// <summary>
        /// Displays the details of a specific invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to view.</param>
        /// <returns>The invoice details view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Invoice Clerk", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult InvoiceDetails(int? invoiceId)
        {
            // Get selected invoice by ID
            Invoice invoice = db.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include("Order.OrderLines.Product")
                .Include(i => i.Order.User)
                .Include(i => i.Order.ShippingInfo)
                .First();

            // If the invoice is empty
            if (invoice == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Create a list of orderline items for the view model
            var items = db.OrderLines
                .Where(i => i.Order.Invoice.InvoiceId == invoiceId)
                .Select(r => new SelectListItem
                {
                    Text = r.Product.ProductName + " x" + r.Quantity,
                    Value = r.Product.ProductName,
                })
                .ToList();

            // Create invoice view model to be thrown to the view
            InvoiceViewModel invoiceViewModel = new InvoiceViewModel
            {
                Invoice = invoice,
                Order = invoice.Order,
                User = invoice.Order.User,
                Shipping = invoice.Order.ShippingInfo,
                OrderLines = invoice.Order.OrderLines,
                OrderLineItems = items
            };

            // Pass the invoiceViewModel to the view
            return View(invoiceViewModel);
        }

        //**********************************************************************************************************************************************************

        //  WAREHOUSE ASSISTANT
        //**********************************************************************************************************************************************************

        //**********************************************************************************************************************************************************

        //  CRUD METHODS FOR DISPATCHING GOODS
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Displays the dispatch goods view.
        /// </summary>
        /// <returns>The view for dispatching goods.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult DispatchGoods()
        {
            // Create dispatch view model to be thrown to the view
            DispatchViewModel dispatchViewModel = new DispatchViewModel();

            // Send this view model to the view
            return View(dispatchViewModel);
        }

        /// <summary>
        /// Handles the dispatching of goods.
        /// </summary>
        /// <param name="dispatchViewModel">The dispatch view model containing dispatch details.</param>
        /// <returns>The appropriate view after dispatching goods.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DispatchGoods(DispatchViewModel dispatchViewModel)
        {
            // Get the specific order
            Order order = await db.Orders
                .Include(o => o.Parcel)
                .Include(o => o.ShippingInfo)
                .Where(o => o.User.UserName == dispatchViewModel.User.UserName)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == dispatchViewModel.Order.OrderId);

            // If order is empty
            if (order == null)
            {
                // Send a warning message to user
                TempData["AlertMessage"] = "Check both Order Id and Users username exists in the database";
                return View(dispatchViewModel);
            }

            // If the parcel property is not epty
            if (order.Parcel != null)
            {
                // Alert the user that the orderId already exists
                TempData["AlertMessage"] = "Dispatch for OrderId " + order.OrderId + " already exists";

                // Redirect user to the manage dispatch page
                return RedirectToAction("ManageDispatches");
            }
            // Change attributes of order to match users input
            order.Parcel = dispatchViewModel.Parcel;
            order.ShippingInfo.ShippingCity = dispatchViewModel.Shipping.ShippingCity;
            order.ShippingInfo.ShippingPostcode = dispatchViewModel.Shipping.ShippingPostcode;
            order.ShippingInfo.ShippingStreet = dispatchViewModel.Shipping.ShippingStreet;

            // If the shipping info is does not match the users home address
            if (order.ShippingInfo.ShippingStreet != order.User.Street
                && order.ShippingInfo.ShippingPostcode != order.User.Postcode
                && order.ShippingInfo.ShippingCity != order.User.City)
            {
                // Turn shippingtohomeaddress boolean to false
                order.ShippingInfo.ShippingToHomeAddress = false;
            }

            // Set order status to Packaging
            order.Status = Status.Packaging;

            // Add or update order
            db.Orders.AddOrUpdate(order);

            // Save changes to the database
            await db.SaveChangesAsync();

            // Redirect user to the manage dispatch page
            return RedirectToAction("ManageDispatches");
        }

        /// <summary>
        /// Displays the manage dispatches view.
        /// </summary>
        /// <returns>The view for managing dispatches.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ManageDispatches()
        {
            // Get all the users orders from the db
            // as long as the parcel is not empty
            var orders = db.Orders
                .Include(o => o.Parcel)
                .Where(o => o.Parcel != null)
                .ToList();

            // If no orders can be found
            if (orders == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Pass them to the view
            return View(orders);
        }

        /// <summary>
        /// Displays the delete dispatch view.
        /// </summary>
        /// <param name="orderId">The ID of the order to delete dispatch for.</param>
        /// <returns>The view for deleting a dispatch.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult DeleteDispatch(int? orderId)
        {
            // Get selected order
            Order order = db.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.ShippingInfo)
                .Include(o => o.Parcel)
                .Include(o => o.User)
                .First(o => o.OrderId == orderId);

            // If no orders can be found
            if (order == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get each product in the orderline
            var items = db.OrderLines
                .Where(i => i.Order.OrderId == order.OrderId)
                .Select(r => new SelectListItem
                {
                    Text = r.Product.ProductName + " x" + r.Quantity,
                    Value = r.Product.ProductName,
                })
                .ToList();

            // Create dispatch view model to be thrown to the view
            DispatchViewModel dispatchViewModel = new DispatchViewModel
            {
                Order = order,
                User = order.User,
                Shipping = order.ShippingInfo,
                Parcel = order.Parcel,
                OrderLines = order.OrderLines,
                OrderLineItems = items
            };

            // Pass the dispatch view model to the view
            return View(dispatchViewModel);
        }

        /// <summary>
        /// Handles the deletion of a dispatch.
        /// </summary>
        /// <param name="orderId">The ID of the order to delete dispatch for.</param>
        /// <returns>The appropriate view after deleting the dispatch.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpPost, ActionName("DeleteDispatch")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletedDispatch(int? orderId)
        {
            // Get parcel
            Parcel parcel = await db.Parcels
                .Where(p => p.Order.OrderId == orderId)
                .Include(p => p.Order)
                .FirstAsync();

            // If no orders can be found
            if (parcel == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Remove the parcel from the database
            db.Parcels.Remove(parcel);

            // Save changes in the database
            await db.SaveChangesAsync();

            // Redirect user to the manage dispatch page
            return RedirectToAction("ManageDispatches");
        }

        /// <summary>
        /// Displays the dispatch details view.
        /// </summary>
        /// <param name="orderId">The ID of the order to display dispatch details for.</param>
        /// <returns>The view with dispatch details.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult DispatchDetails(int? orderId)
        {
            // Get selected order
            Order order = db.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.ShippingInfo)
                .Include(o => o.Parcel)
                .Include(o => o.User)
                .First(o => o.OrderId == orderId);

            // If no orders can be found
            if (order == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get each product in the orderline
            var items = db.OrderLines
                .Where(i => i.Order.OrderId == order.OrderId)
                .Select(r => new SelectListItem
                {
                    Text = r.Product.ProductName + " x" + r.Quantity,
                    Value = r.Product.ProductName,
                })
                .ToList();

            // Create dispatch view model to be thrown to the view
            DispatchViewModel dispatchViewModel = new DispatchViewModel
            {
                Order = order,
                User = order.User,
                Shipping = order.ShippingInfo,
                Parcel = order.Parcel,
                OrderLines = order.OrderLines,
                OrderLineItems = items
            };

            // Pass the dispatch view model to the view
            return View(dispatchViewModel);
        }

        /// <summary>
        /// Displays the edit dispatch view.
        /// </summary>
        /// <param name="orderId">The ID of the order to edit dispatch for.</param>
        /// <returns>The view for editing a dispatch.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult EditDispatch(int? orderId)
        {
            // Get the selected order from the db
            Order order = db.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefault(o => o.OrderId == orderId);

            // If no orders can be found
            if (order == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get each product in the orderline
            var items = db.OrderLines
                .Where(o => o.Order.OrderId == orderId)
                .Include(i => i.Product)
                .ToList();

            // Initialise and Instantiate dispatchViewModel object
            DispatchViewModel dispatchViewModel = new DispatchViewModel
            {
                Order = order,
                OrderLines = items
            };

            // Throw the dispatchViewModel to the view
            return View(dispatchViewModel);
        }

        /// <summary>
        /// Handles the editing of a dispatch.
        /// </summary>
        /// <param name="dispatchViewModel">The dispatch view model containing dispatch details.</param>
        /// <returns>The appropriate view after editing the dispatch.</returns>
        [AuthorizeRedirect(Roles = "Admin,Warehouse Assistant", RedirectUrl = "/Home/Index")]
        [HttpPost, ActionName("EditDispatch")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditedDispatch(DispatchViewModel dispatchViewModel)
        {
            // Get the selected order from the db
            Order order = await db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == dispatchViewModel.Order.OrderId);

            // If no orders can be found
            if (order == null)
            {
                // Return a bad request page
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Assign dispatchViewModel order status to order status
            order.Status = dispatchViewModel.Order.Status;

            // Add order to the db
            db.Orders.AddOrUpdate(order);

            // Save changes in the database
            await db.SaveChangesAsync();

            // Store confirmationMessage in TempData to be caught/used in the view
            TempData["ConfirmationMessage"] = "OrderId " + order.OrderId + " status has been succesfully changed.";

            // Redirect user to the manage dispatch page
            return RedirectToAction("ManageDispatches");
        }

        //**********************************************************************************************************************************************************

        //  SALES ASSISTANT & SALES MANAGER
        //**********************************************************************************************************************************************************

        //**********************************************************************************************************************************************************

        //  CRUD METHODS FOR CREATING ORDER
        //**********************************************************************************************************************************************************

        /// <summary>
        /// Displays the product count view for creating an order.
        /// </summary>
        /// <returns>The product count view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Assistant,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult ProductCount()
        {
            // Get ModelState from TempData if it exists
            if (TempData["ModelState"] != null)
            {
                ModelState.Merge((ModelStateDictionary)TempData["ModelState"]);
            }

            // Create order view model to be thrown to the view
            CreateOrderViewModel orderViewModel = new CreateOrderViewModel();

            // Throw orderViewModel to the view
            return View(orderViewModel);
        }

        /// <summary>
        /// Displays the create order view.
        /// </summary>
        /// <param name="orderViewModel">The order view model.</param>
        /// <returns>The create order view.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Assistant,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpGet]
        public ActionResult CreateOrder(CreateOrderViewModel orderViewModel)
        {
            if (!ModelState.IsValid)
            {
                // Store the ModelState in TempData
                TempData["ModelState"] = ModelState;

                // Redirect back to the ProductCount action
                return RedirectToAction("ProductCount");
            }

            // Get all products and their categories
            var product = db.Products
                .Include(p => p.Category)
                .ToList();

            // Check if the product count exceeds available products
            if (orderViewModel.ProductCount > product.Count())
            {
                TempData["AlertMessage"] = "Product Count cannot surpass the amount of products in storage";
                return RedirectToAction("ProductCount", TempData["AlertMessage"]);
            }

            // Initialize order lines in the view model
            orderViewModel.OrderLines = new List<OrderLine>();

            // Add empty orderlines based on the product count
            for (int i = 0; i < orderViewModel.ProductCount; i++)
            {
                orderViewModel.OrderLines.Add(new OrderLine());
            }

            // Pass the orderViewModel to the view
            return View(orderViewModel);
        }

        /// <summary>
        /// Creates a new order based on the provided order view model.
        /// </summary>
        /// <param name="orderViewModel">The order view model.</param>
        /// <returns>A redirect to the placed orders page or the create order view with validation messages.</returns>
        [AuthorizeRedirect(Roles = "Admin,Sales Assistant,Sales Manager", RedirectUrl = "/Home/Index")]
        [HttpPost, ActionName("CreateOrder")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatedOrder(CreateOrderViewModel orderViewModel)
        {

            // Fetch the products related to the orderlines in a single query
            var productIds = orderViewModel.OrderLines.Select(ol => ol.Product.ProductId).ToList();
            var products = await db.Products.Include(p => p.Category)
                                      .Where(p => productIds.Contains(p.ProductId))
                                      .ToListAsync();

            // Get the user by username
            User user = await db.Users
                .Where(u => u.UserName == orderViewModel.User.UserName)
                .FirstOrDefaultAsync();

            // Check if the user is null
            if (user == null)
            {
                // If it is we prompt the user with the following error
                TempData["AlertMessage"] = "Username doesn't exist in the database";
                return View(orderViewModel);
            }

            // If the products ids dont match exactly and the products or user objects are empty
            if (productIds.Count != products.Count || products == null || user == null)
            {
                // We return the view
                return View(orderViewModel);
            }

            // Initialize shipping information
            orderViewModel.Shipping = new ShippingInfo();

            // Get shipping costs based on customer type
            if (user is Customer customer)
            {
                if (customer.CustomerType.Equals(CustomerType.Standard))
                {
                    orderViewModel.Shipping.ShippingCost = 4.99M;
                }

                else if (customer.CustomerType.Equals(CustomerType.Premium))
                {
                    orderViewModel.Shipping.ShippingCost = 2.99M;
                }

                else if (customer.CustomerType.Equals(CustomerType.Corporate))
                {
                    orderViewModel.Shipping.ShippingCost = 0.99M;
                }
            }

            // Set shipping information
            orderViewModel.Shipping.ShippingToHomeAddress = true;
            orderViewModel.Shipping.ShippingStreet = user.Street;
            orderViewModel.Shipping.ShippingPostcode = user.Postcode;
            orderViewModel.Shipping.ShippingCity = user.City;

            // Create a new order object
            var order = new Order
            {
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now,
                Status = orderViewModel.Order.Status,
                User = user,
                ShippingInfo = orderViewModel.Shipping,
                OrderLines = new List<OrderLine>()
            };


            decimal totalOrder = 0;

            // Add order lines to the order
            foreach (var line in orderViewModel.OrderLines)
            {
                // Check for valid quantity
                if (line.Quantity <= 0)
                {
                    return View(orderViewModel);
                }

                // Find the product associated with the orderline
                var product = products.FirstOrDefault(p => p.ProductId == line.Product.ProductId);

                if (product != null)
                {
                    var orderLine = new OrderLine
                    {
                        ProductId = product.ProductId,
                        Product = product,
                        Quantity = line.Quantity,
                    };

                    // Calculate the line total based on sale price or regular price
                    if (product.OnSale)
                    {
                        orderLine.LineTotal = product.DiscountPrice * line.Quantity;
                        totalOrder += orderLine.LineTotal;
                    }
                    else
                    {
                        orderLine.LineTotal = product.Price * line.Quantity;
                        totalOrder += orderLine.LineTotal;
                    }

                    // Check if the stock level is sufficient
                    if (product.StockLevel < line.Quantity)
                    {
                        TempData["AlertMessage"] = "product stock level must be sufficient";
                        return View(orderViewModel);
                    }

                    // Reduce the stock level
                    product.StockLevel -= line.Quantity;
                    order.OrderLines.Add(orderLine);
                }
            }

            // Set the order total and total amount
            order.OrderTotal = totalOrder;
            order.TotalAmount = totalOrder + orderViewModel.Shipping.ShippingCost;

            // Add the order to the database and save changes
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Give the confirmation message a value
            TempData["ConfirmationMessage"] = "Order ID " + order.OrderId + " has been successfuly created.";

            // Redirect to the placed orders page
            return RedirectToAction("PlacedOrders");
        }

        //****************  OTHER METHODS   *********************

        /// <summary>
        /// Calculates the discount percentage between the original price and the discount price.
        /// </summary>
        /// <param name="originalPrice">The original price of the product.</param>
        /// <param name="discountPrice">The discount price of the product.</param>
        /// <returns>The discount percentage as an integer.</returns>
        public int DiscountPercentage(decimal originalPrice, decimal discountPrice)
        {
            // Calculate the discount percentage
            decimal total = ((originalPrice - discountPrice) / originalPrice) * 100;

            // Convert the percentage to an integer
            int totalInt = (int)total;

            // Return the discount percentage
            return totalInt;
        }
    }
}