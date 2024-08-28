using K_Burns_GU2_Speedo_Models.CustomAttributes;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using K_Burns_GU2_Speedo_Models.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// The custom filter redirects user to the home/index if they are logged in but not a customer
    /// 
    /// Handles the checkout process and basket management for customers.
    /// </summary>
    [AuthorizeRedirect(Roles = "Customer", RedirectUrl = "/Home/Index")] 
    public class CheckoutController : Controller
    {
        // Declare global private DBContext object
        private SpeedoModelsDbContext context = new SpeedoModelsDbContext();

        //*****************************************************************************************************
        //***********************   CHECKOUT   ****************************************************************
        //*****************************************************************************************************

        /// <summary>
        /// Displays the checkout page for the user, including their basket and shipping information.
        /// </summary>
        /// <param name="reloaded">Indicates if the page was reloaded due to missing shipping details.</param>
        /// <returns>The checkout view with the basket and shipping information.</returns>
        public async Task<ActionResult> Checkout(bool? reloaded)
        {
            // Get user by ID
            string userId = User.Identity.GetUserId();
            User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Retrieve the specific user's basket. Create one if it doesn't exist.
            Basket basket = await context.Baskets
                .Include("BasketItems.Product.Category")
                .Where(b => b.User.Id == userId)
                .FirstOrDefaultAsync();

            // If the basket is null, return 'Basket Is Empty' view
            if (basket == null)
            {
                return View("BasketIsEmpty");
            }

            // Get the shipping info from the database
            ShippingInfo shippingInfo = await context.ShippingInfos.FirstOrDefaultAsync();

            // Create a new view model for both the basket and shipping info
            BasketShippingViewModel basketViewModel = new BasketShippingViewModel();

            // Assign basket to the basketviewmodel
            basketViewModel.Basket = basket;

            // If shipping info is empty
            if (basketViewModel.ShippingInfo == null && reloaded == true)
            {
                // Create a new shipping info object
                // So that it isnt null when passing it to the view
                basketViewModel.ShippingInfo = new ShippingInfo();

                // If reloaded is true
                if (reloaded == true)
                {
                    // Create a Alert Message
                    TempData["AlertMessage"] = "SHIPPING DETAILS MUST BE SELECTED OR ADDED";
                    basketViewModel.PageReloaded = true; // Turn the PageReloaded attribute to true
                }
            }
            else
            {
                //create a new one
                basketViewModel.ShippingInfo = new ShippingInfo();
                //turn this to false to let the user know
                //they need to fill in the fields
                basketViewModel.ShippingInfo.ShippingToHomeAddress = false;
            }

            //get shipping costs based on customer type
            if (user is Customer customer)
            {
                if (customer.CustomerType.Equals(CustomerType.Standard))
                {
                    basketViewModel.ShippingInfo.ShippingCost = 4.99M;
                }

                else if (customer.CustomerType.Equals(CustomerType.Premium))
                {
                    basketViewModel.ShippingInfo.ShippingCost = 2.99M;
                }

                else if (customer.CustomerType.Equals(CustomerType.Corporate))
                {
                    basketViewModel.ShippingInfo.ShippingCost = 0.99M;
                }
            }

            // Calculate the total amount
            basketViewModel.Basket.TotalAmount = basketViewModel.Basket.BasketTotal + basketViewModel.ShippingInfo.ShippingCost;

            //Save the changes in the database
            await context.SaveChangesAsync();

            //pass the basketViewModel to the view
            return View(basketViewModel);
        }

        //*****************************************************************************************************
        //***********************   BASKET PAGE BUTTONS   *****************************************************
        //*****************************************************************************************************

        /// <summary>
        /// Increments the quantity of a basket item during checkout.
        /// </summary>
        /// <param name="BasketItemId">The ID of the basket item to increment.</param>
        /// <returns>Redirects to the checkout view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> IncrementCheckout(string BasketItemId)
        {
            try
            {
                // Get current basket item ID as an int
                int currentBaskeItemId = Int32.Parse(BasketItemId);

                // Get user by ID
                string userId = User.Identity.GetUserId();
                User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Get the specific user's basket. Create one if it doesn't exist.
                Basket basket = await context.Baskets
                    .Include("BasketItems.Product.Category")
                    .Where(b => b.User.Id == userId)
                    .FirstOrDefaultAsync();

                // If the basket is null, return 'Basket Is Empty' view
                if (basket == null)
                {
                    return View("BasketIsEmpty");
                }

                //Get basket item
                BasketItem basketItem = await context.BasketItems.FindAsync(currentBaskeItemId);

                if (basketItem != null && basketItem.Product.StockLevel > 0)
                {
                    // Change the basket items value in the basket
                    // By incrementing by 1
                    int newQuantity = ++basketItem.Quantity; //position is the id - 1
                    decimal price = basketItem.Product.Price;
                    basket.BasketSize++;

                    // Change price if product is on a sale
                    if (basketItem.Product.OnSale)
                    {
                        price = basketItem.Product.DiscountPrice;
                    }

                    // Get new item total
                    basketItem.ItemTotal = newQuantity * price;

                    // Manage basket attributes
                    basketItem.Product.StockLevel--;
                    // Basket.BasketItems.Add(basketItem);
                    basket.BasketTotal += price;

                    // Save changes to the database
                    await context.SaveChangesAsync();

                    // Save session state
                    Session["BasketSize"] = basket.BasketSize;
                }
                else
                {
                    // Update the Alert Message
                    TempData["AlertMessage"] = "PRODUCT STOCK LEVEL IS INSUFFICIENT";
                }

                // Redirect to the checkout page
                return RedirectToAction("Checkout");
            }

            // Catch any Exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }
            
        }

        /// <summary>
        /// Decrements the quantity of a basket item during checkout.
        /// </summary>
        /// <param name="BasketItemId">The ID of the basket item to decrement.</param>
        /// <returns>Redirects to the checkout view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DecrementCheckout(string BasketItemId)
        {
            try
            {
                // Get current basket item ID as an int
                int currentBasketItemId = Int32.Parse(BasketItemId);

                // Get user by ID
                string userId = User.Identity.GetUserId();
                User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Get the specific user's basket. Create one if it doesn't exist.
                Basket basket = await context.Baskets
                    .Include("BasketItems.Product.Category")
                    .Where(b => b.User.Id == userId)
                    .FirstOrDefaultAsync();

                // If the basket is null, return 'Basket Is Empty' view
                if (basket == null)
                {
                    return View("BasketIsEmpty");
                }

                //Get basket item
                BasketItem basketItem = await context.BasketItems.FindAsync(currentBasketItemId);

                if (basketItem != null && basketItem.Quantity > 1)
                {
                    // Change the basket items value in the basket
                    // By decrementing by 1
                    int newQuantity = --basketItem.Quantity;
                    decimal price = basketItem.Product.Price;
                    basket.BasketSize--;

                    // Change price if product is on a sale
                    if (basketItem.Product.OnSale)
                    {
                        price = basketItem.Product.DiscountPrice;
                    }

                    // Get new item total
                    basketItem.ItemTotal = newQuantity * price;

                    // Manage basket attributes
                    basketItem.Product.StockLevel++;
                    basket.BasketTotal -= price;
                    
                    // Save changes to the database
                    await context.SaveChangesAsync();

                    // Save session state
                    Session["BasketSize"] = basket.BasketSize;
                }

                // Redirect to the checkout page
                return RedirectToAction("Checkout");
            }

            // Catch any Exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }
            
        }

        /// <summary>
        /// Removes an item from the basket during checkout.
        /// </summary>
        /// <param name="BasketItemId">The ID of the basket item to remove.</param>
        /// <returns>Redirects to the checkout view or shows the basket is empty view if no items are left.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveItem(string BasketItemId)
        {
            try
            {
                // Get current item ID as an int
                int currentBasketItemId = Int32.Parse(BasketItemId);

                // Get user by ID
                string userId = User.Identity.GetUserId();
                User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Get the specific user's basket. Create one if it doesn't exist.
                Basket basket = await context.Baskets
                    .Include("BasketItems.Product.Category")
                    .Where(b => b.User.Id == userId)
                    .FirstOrDefaultAsync();

                // If the basket is null, return 'Basket Is Empty' view
                if (basket == null)
                {
                    return View("BasketIsEmpty");
                }

                // Find basket item
                BasketItem basketItem = await context.BasketItems.FindAsync(currentBasketItemId);

                // Update basket size and total
                basket.BasketSize -= basketItem.Quantity;
                basket.BasketTotal -= basketItem.ItemTotal;
                basketItem.Product.StockLevel += basketItem.Quantity;

                // Remove the item from the basket and database
                basket.BasketItems.Remove(basketItem);
                context.BasketItems.Remove(basketItem);

                // Save session state
                Session["BasketSize"] = basket.BasketSize;

                // Saves changes to the database
                await context.SaveChangesAsync();

                // If basket is empty, remove it and show the empty basket view
                if (basket.BasketSize == 0)
                {
                    context.Baskets.Remove(basket);
                    return View("BasketIsEmpty");
                }

                // Redirects to the checkout page
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                // Catch any Exception errors
                return View("TryCatchError", ex);
            } 
        }
    }
}