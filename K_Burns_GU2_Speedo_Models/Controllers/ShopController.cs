using K_Burns_GU2_Speedo_Models.Models;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using K_Burns_GU2_Speedo_Models.CustomAttributes;
using System.Web.Compilation;
using Microsoft.Ajax.Utilities;
using System.Threading.Tasks;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Controller for managing Shop related functions such as the Product page and filtering by category
    /// </summary>
    public class ShopController : Controller
    {
        // Declare global private DBContext object
        private SpeedoModelsDbContext context = new SpeedoModelsDbContext();

        /// <summary>
        /// Retrieves all products that are not discontinued and displays them on the index page.
        /// </summary>
        /// <returns>View with list of products.</returns>
        public ActionResult Index()
        {
            // Getting all the products that are not discontinued and deleted
            var products = context.Products.Where(p => p.Discontinued == false && p.IsDeleted == false).ToList();

            // Declare empty string
            string selectedCategory = null;

            // Refresh entities
            foreach (var item in products)
            {
                context.Entry(item).Reload();//refresh entities
            }

            // Send all categories to the Viewbag
            ViewBag.Categories = context.Categories;

            // Send no selected category to the index view
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        //***********************************************************************************
        //***********************   PRODUCTS PAGE BY CATEGORY *******************************
        //***********************************************************************************

        /// <summary>
        /// Retrieves products by category and displays them.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>View with list of products in the selected category.</returns>
        public ActionResult Products(int? id)
        {
            // Getting all the products that are not discontinued and deleted that are in different categories
            var products = context.Products.Where(p => p.Discontinued == false && p.IsDeleted == false).Where(p => p.CategoryId == id).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Also don't forget to send all the categories in a viewbag
            ViewBag.Categories = context.Categories;

            // Send selected category to the view, to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        //***********************************************************************************
        //***********************   ADD TO BASKET  ******************************************
        //***********************************************************************************

        /// <summary>
        /// Adds a product to the basket.
        /// </summary>
        /// <param name="ProductId">Product ID</param>
        /// <returns>Redirects to Index action.</returns>
        [AuthorizeRedirect(Roles = "Customer")] 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(int ProductId)
        {
            try
            {
                // create a product object
                Product product = await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == ProductId);

                // Get user by id
                string userId = User.Identity.GetUserId();
                User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Check Basket Exists and if it does store it in 'basket' object
                bool isNewBasket = false;
                var basket = await EnsureBasketExists(User.Identity.GetUserId(), isNewBasket);

                // Low stock level check
                if (product.StockLevel <= 5)
                {
                    TempData["WarningMessage"] = "PRODUCT STOCK LEVEL IS LOW";
                }

                // Loop through the orderlines in the basket
                for (int i = 0; i < basket.BasketItems.Count; i++)
                {
                    // If the product already exists in the basket
                    if (basket.BasketItems[i].ProductId == product.ProductId)
                    {
                        // Send a temporary product already exists in basket string
                        // To be thrown to the front end
                        TempData["AlertMessage"] = "PRODUCT ALREADY IN BASKET";
                        TempData["WarningMessage"] = null; //so that it doesnt pop up again
                        // Redirect to Http get method
                        return RedirectToAction("Index");
                    }
                }

                // Set basket Item to null
                BasketItem basketItem = null;

                if (product.OnSale)
                {
                    // Create a new basket item
                    // Get the discount price for the item total
                    basketItem = new BasketItem
                    {
                        Quantity = 1,
                        ItemTotal = product.DiscountPrice,
                        Product = product,
                        Basket = basket
                    };
                }
                else
                {
                    // Create a new basket item
                    // Get the normal price for the item total
                    basketItem = new BasketItem
                    {
                        Quantity = 1,
                        ItemTotal = product.Price,
                        Product = product,
                        Basket = basket
                    };
                }

                // Add the new orderLine to the basket
                basket.BasketItems.Add(basketItem);

                // Update the basket size. This might vary based on how you want to count size.
                basket.BasketSize += basketItem.Quantity; // Or recalculate based on total quantity of items

                basket.BasketTotal += basketItem.ItemTotal;

                // Deduct stock level immediately to avoid inconsistencies.
                product.StockLevel -= basketItem.Quantity;

                // IMPORTANT: Update the product immediately to ensure the stock level change is persisted.
                // Assuming 'context' is your DbContext instance.
                context.Entry(product).State = EntityState.Modified;

                // Add basket to db
                await context.SaveChangesAsync(); // Save changes immediately to persist the new stock level.

                // Save session state
                Session["BasketSize"] = basket.BasketSize;

                // Go back to index
                return RedirectToAction("Index");
            }

            // Catch any exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }
        }

        //***********************************************************************************
        //***********************   PRODUCT  PAGE  ******************************************
        //***********************************************************************************

        /// <summary>
        /// Displays details of a specific product.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>View with product details.</returns>
        [HttpGet]
        public ActionResult Product(int? id)
        {
            // If ID is null return a bad request error
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Find product by ID
            Product product = context.Products.Find(id);

            // If product object is empty
            if (product == null)
            {
                // Return an HttpNotFound
                return HttpNotFound();
            }

            // Gets random products
            var allProducts = GetRandomProducts(product.ProductId);


            // Create the view model to toggle quantity
            ProductPageViewModel viewModel = new ProductPageViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                Price = product.Price,
                OnSale = product.OnSale,
                ImageUrl = product.ImageUrl,
                Products = allProducts,
                Quantity = 1 // Default quantity
            };

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories.ToList();

            // Store all products in this ViewBag and send it to the view to be iterated through
            ViewBag.AllProducts = allProducts;

            // Send product variable to the view
            return View(viewModel);
        }

        /// <summary>
        /// Adds a product to the basket from the product details page.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="viewModel">ProductPageViewModel</param>
        /// <returns>Redirects to Product action.</returns>
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Product(int id, [Bind(Include = "ProductId,ProductName,ProductDescription,Price,ImageUrl,OnSale,Products,Quantity")] ProductPageViewModel viewModel)
        {
            try
            {
                if (id != viewModel.ProductId)
                {
                    id = viewModel.ProductId;
                }

                // Create a product object
                Product product = await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);


                // If product is empty
                if (product == null)
                {
                    // Return error
                    return HttpNotFound();
                }

                // If the product quantity is not validated
                if (!ValidateProductQuantity(viewModel, product))
                {
                    // Redirect to Product Action
                    return RedirectToAction("Product");
                }

                // Check Basket Exists and if it does store it in 'basket' object
                bool isNewBasket = false;
                var basket = await EnsureBasketExists(User.Identity.GetUserId(), isNewBasket);

                // Loop through the basket items in the basket
                for (int i = 0; i < basket.BasketItems.Count; i++)
                {
                    // If the product already exists in the basket
                    if (basket.BasketItems[i].ProductId == product.ProductId)
                    {
                        // Send a temporary product already exists in basket string
                        // to be thrown to the front end
                        TempData["AlertMessage"] = "PRODUCT ALREADY IN BASKET";
                        // Redirect to Http get method
                        return RedirectToAction("Product");
                    }
                }

                // Add product to the basket
                await AddProductToBasket(product, viewModel, basket);

                // Low product check
                if (product.StockLevel <= 5)
                {
                    TempData["WarningMessage"] = "PRODUCT STOCK LEVEL IS LOW";
                }

                // Save session state
                Session["BasketSize"] = basket.BasketSize;

                // Redirect to Product action
                return RedirectToAction("Product");
            }

            // Catch any exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }      
        }

        //*****************************************************************************************************
        //***********************   INDIVDUAL PRODUCT CATEGORY   **********************************************
        //*****************************************************************************************************

        /// <summary>
        /// Displays products in the "Cars" category.
        /// </summary>
        /// <returns>View with list of products in the "Cars" category.</returns>
        public ActionResult Cars()
        {
            // Find the Cars category
            var cars = context.Categories.Where(c => c.CategoryName == "Cars").SingleOrDefault();

            // Getting all the car products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == cars.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == cars.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        /// <summary>
        /// Displays products in the "Child Sets" category.
        /// </summary>
        /// <returns>View with list of products in the "Child Sets" category.</returns>
        public ActionResult ChildSets()
        {
            // Find the Child Sets category
            var childSets = context.Categories.Where(c => c.CategoryName == "Child Sets").SingleOrDefault();

            // Getting all the Child Set products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == childSets.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == childSets.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        /// <summary>
        /// Displays products in the "Tools" category.
        /// </summary>
        /// <returns>View with list of products in the "Tools" category.</returns>
        public ActionResult Tools()
        {
            // Find the Tools category
            var tools = context.Categories.Where(c => c.CategoryName == "Tools").SingleOrDefault();

            // Getting all the Tool products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == tools.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == tools.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        /// <summary>
        /// Displays products in the "Limited Edition Sets" category.
        /// </summary>
        /// <returns>View with list of products in the "Limited Edition Sets" category.</returns>
        public ActionResult LimitedSets()
        {
            // Find the Limited Sets category
            var limitedSets = context.Categories.Where(c => c.CategoryName == "Limited Edition Sets").SingleOrDefault();

            // Getting all the Limited Set products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == limitedSets.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == limitedSets.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        /// <summary>
        /// Displays products in the "Standard Sets" category.
        /// </summary>
        /// <returns>View with list of products in the "Standard Sets" category.</returns>
        public ActionResult StandardSets()
        {
            // Find the Standard Sets category
            var standardSets = context.Categories.Where(c => c.CategoryName == "Standard Sets").SingleOrDefault();

            // Getting all the Standard Set products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == standardSets.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == standardSets.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        /// <summary>
        /// Displays products in the "Tracks" category.
        /// </summary>
        /// <returns>View with list of products in the "Tracks" category.</returns>
        public ActionResult Tracks()
        {
            // Find the Tracks category
            var tracks = context.Categories.Where(c => c.CategoryName == "Tracks").SingleOrDefault();

            // Getting all the Track products that are not discountinued
            var products = context.Products.Where(p => p.Discontinued == false).Where(p => p.CategoryId == tracks.CategoryId).ToList();

            // Get the name of the passed in category
            // If it is empty it will assign null to our variable
            var selectedCategory = context.Categories
                .Where(c => c.CategoryId == tracks.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            // Sending all the categories, in a viewbag, to the view
            ViewBag.Categories = context.Categories;

            // Send selected category to the view to be displayed
            ViewBag.SelectedCategory = selectedCategory;

            // Send products variable to the "Products" view
            return View("Products", products);
        }

        //**********************************************************************************************
        //********************************** NORMAL METHODS ********************************************
        //**********************************************************************************************

        /// <summary>
        /// Gets random products excluding a specific product.
        /// </summary>
        /// <param name="excludeProductId">Product ID to exclude.</param>
        /// <returns>List of random products.</returns>
        private List<Product> GetRandomProducts(int excludeProductId)
        {
            // Initialize a new instance of random
            Random random = new Random();

            // Get all products from the database, excluding the specified product
            List<Product> allProducts = context.Products
                .Where(p => p.ProductId != excludeProductId)
                .Where(p => p.IsDeleted == false)
                .ToList();

            // Randomly select 3 products from the list
            allProducts = allProducts
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();

            // Return the list of randomly selected products
            return allProducts;
        }

        //****************************************************************************************************************************
        //********************************** NORMAL METHODS DEALING WITH ADDING TO BASKET ********************************************
        //****************************************************************************************************************************

        /// <summary>
        /// Validates the quantity of a product.
        /// </summary>
        /// <param name="viewModel">ProductPageViewModel</param>
        /// <param name="product">Product</param>
        /// <returns>True if quantity is valid; otherwise, false.</returns>
        private bool ValidateProductQuantity(ProductPageViewModel viewModel, Product product)
        {
            // Check if the requested quantity exceeds the available stock
            if (viewModel.Quantity > product.StockLevel)
            {
                // If insufficient stock, set an alert message and return false
                TempData["AlertMessage"] = "PRODUCT STOCK LEVEL IS INSUFFICIENT";
                return false;
            }
            // Check if the requested quantity is less than or equal to zero
            else if (viewModel.Quantity <= 0)
            {
                // If invalid quantity, set an alert message and return false
                TempData["AlertMessage"] = "QUANTITY MUST BE 1 OR MORE";
                return false;
            }

            // If quantity is valid, return true
            return true;
        }

        /// <summary>
        /// Ensures a basket exists for a user, creating one if it doesn't.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isNewBasket">Indicates if a new basket was created.</param>
        /// <returns>Basket</returns>
        private async Task<Basket> EnsureBasketExists(string userId, bool isNewBasket)
        {
            // Get the user from the database
            User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Get the specific user's basket. Create one if it doesn't exist.
            Basket basket = await context.Baskets.Include(b => b.BasketItems).Where(b => b.User.Id == userId)
                                           .FirstOrDefaultAsync();

            // Initialize the flag indicating whether a new basket was created
            isNewBasket = false;
            if (basket == null)
            {
                // Create a new basket if one doesn't exist
                basket = new Basket
                {
                    BasketItems = new List<BasketItem>(),
                    BasketSize = 0, // Initialize as appropriate
                    BasketAbandoned = false, // Initialize as appropriate
                    User = user // Ensure the user exists in your context
                };
                // Add the new basket to the database context
                context.Baskets.Add(basket);
                // Mark that this is a new basket so we know to save it immediately.
                isNewBasket = true;
            }

            // If it's a new basket, save it here to ensure it gets an ID.
            if (isNewBasket)
            {
                // Save changes to the database to assign an ID to the new basket
                await context.SaveChangesAsync();
            }

            //return the basket object
            return basket;
        }

        /// <summary>
        /// Adds a product to a basket.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="viewModel">ProductPageViewModel</param>
        /// <param name="basket">Basket</param>
        /// <returns>Task</returns>
        private async Task AddProductToBasket(Product product, ProductPageViewModel viewModel, Basket basket)
        {
            // Declare empty basketItem object
            BasketItem basketItem = null;

            // Check if the product is on sale
            if (product.OnSale)
            {
                // Create a new basket item with discount price
                basketItem = new BasketItem
                {
                    Quantity = viewModel.Quantity,
                    ItemTotal = viewModel.Quantity * product.DiscountPrice,
                    Product = product,
                    Basket = basket
                };
            }
            else
            {
                // Create a new basket item with normal price
                basketItem = new BasketItem
                {
                    Quantity = viewModel.Quantity,
                    ItemTotal = viewModel.Quantity * product.Price,
                    Product = product,
                    Basket = basket
                };
            }
            

            // Add the new orderLine to the basket
            basket.BasketItems.Add(basketItem);

            // Update the basket size. This might vary based on how you want to count size.
            basket.BasketSize += basketItem.Quantity; // Or recalculate based on total quantity of items

            // Update the basket total
            basket.BasketTotal += basketItem.ItemTotal;

            // Deduct stock level immediately to avoid inconsistencies.
            product.StockLevel -= viewModel.Quantity;

            // IMPORTANT: Update the product immediately to ensure the stock level change is persisted.
            // Assuming 'context' is your DbContext instance.
            context.Entry(product).State = EntityState.Modified;

            // Save changes immediately to persist the new stock level.
            await context.SaveChangesAsync(); 
        }
    }
}