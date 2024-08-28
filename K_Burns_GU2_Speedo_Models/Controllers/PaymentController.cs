using K_Burns_GU2_Speedo_Models.Models;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.EnterpriseServices;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Configuration;
using System.IO;
using System.Web.Security;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Manages payment-related actions, including loading payment information, processing payments, and confirming orders.
    /// </summary>
    public class PaymentController : AccountController
    {

        // Declare global private DBContext object
        private SpeedoModelsDbContext db = new SpeedoModelsDbContext();

        /// <summary>
        /// Loads the payment page with the given shipping information.
        /// </summary>
        /// <param name="shippingInfo">The shipping information to be included.</param>
        /// <returns>The payment view with the order details, or the checkout view if shipping information is invalid.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoadPayment([Bind(Include = "ShippingId,ShippingCost,ShippingToHomeAddress,ShippingStreet,ShippingCity,ShippingPostcode")] ShippingInfo shippingInfo)
        {
            try
            {
                // If object is null return a bad request error
                if (shippingInfo == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                // Get user by ID
                string userId = User.Identity.GetUserId();
                User user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Get the specific user's basket. Create one if it doesn't exist.
                Basket basket = await db.Baskets
                    .Include("BasketItems.Product.Category")
                    .Where(b => b.User.Id == userId)
                    .FirstOrDefaultAsync();

                if (ModelState.IsValid)
                {
                    // Create a new order
                    Order order = new Order
                    {
                        OrderSize = basket.BasketSize,
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5),
                        OrderTotal = basket.BasketTotal,
                        TotalAmount = basket.TotalAmount,
                        Status = Status.Pending,
                        User = user,
                        ShippingInfo = shippingInfo,
                        OrderLines = ConvertBasketItems(basket.BasketItems)
                    };

                    shippingInfo.Order = order;

                    // Add and save the shipping information and order to the database
                    db.ShippingInfos.Add(shippingInfo);
                    db.Orders.Add(order);

                    await db.SaveChangesAsync();

                    return View("Payment", order);
                }
                else if (shippingInfo.ShippingToHomeAddress)
                {
                    // Make the shipping address info equal to the user address info
                    shippingInfo.ShippingStreet = user.Street;
                    shippingInfo.ShippingCity = user.City;
                    shippingInfo.ShippingPostcode = user.Postcode;

                    // Create a new order
                    Order order = new Order
                    {
                        OrderSize = basket.BasketSize,
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5),
                        OrderTotal = basket.BasketTotal,
                        TotalAmount = basket.TotalAmount,
                        Status = Status.Pending,
                        User = user,
                        ShippingInfo = shippingInfo,
                        OrderLines = ConvertBasketItems(basket.BasketItems)
                    };


                    shippingInfo.Order = order;

                    // Add and save the shipping information and order to the database
                    db.ShippingInfos.Add(shippingInfo);
                    db.Orders.Add(order);

                    await db.SaveChangesAsync();

                    // Pass the order object to the Payment view
                    return View("Payment", order);
                }

                // Redirect action to the Checkout action in the Checkout controller
                // and pass the reloaded boolean with the 'true' value
                return RedirectToAction("Checkout", "Checkout", new { reloaded = true });
            }

            // Catch any exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }  
        }

        /// <summary>
        /// Processes the payment for the given order.
        /// </summary>
        /// <param name="order">The order details.</param>
        /// <param name="PaymentType">The type of payment.</param>
        /// <returns>A redirect to the payment session, or an error if the payment fails.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment([Bind(Include = "OrderId,OrderDate,DeliveryDate,OrderTotal,Status,OrderLines")] Order order, string PaymentType)
        {
            // Get user by ID
            string userId = User.Identity.GetUserId();
            User user = db.Users.Find(userId);

            // Get the specific user's basket. Create one if it doesn't exist.
            Basket basket = db.Baskets
                .Include("BasketItems.Product.Category")
                .Where(b => b.User.Id == userId)
                .FirstOrDefault();

            // Get the shipping info
            ShippingInfo shippingInfo = db.ShippingInfos
                .Where(s => s.Order.OrderId == order.OrderId)
                .FirstOrDefault();

            //***************   STRIPE PAYMENT  *********************

            // Create Stripe line items
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in basket.BasketItems)
            {
                decimal price = item.Product.Price;

                if (item.Product.OnSale)
                {
                    price = item.Product.DiscountPrice;
                }

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = price * 100,
                        Currency = "gbp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.ProductName
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            decimal shippingCost = shippingInfo.ShippingCost; // display shipping cost

            // Add the shipping cost as a separate line item
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = shippingCost * 100, // Convert to smallest unit
                    Currency = "gbp",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Shipping"
                    }
                },
                Quantity = 1 // Shipping cost typically applies once per order
            });


            // Get the Uri object for the current request
            Uri requestUrl = Request.Url;

            // Build the base URL from the Uri object
            var baseUrl = requestUrl.GetLeftPart(UriPartial.Authority);

            // Create the session options for Stripe checkout
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = baseUrl + Url.Action("OrderConfirmed", "Payment", new { orderId = order.OrderId }),
                CancelUrl = baseUrl + Url.Action("Checkout", "Checkout")
            };

            // Create a new session service for Stripe
            var service = new Stripe.Checkout.SessionService();

            // Create a new session with the specified options
            Stripe.Checkout.Session session = service.Create(options);

            // Add the session URL to the response headers for redirection
            Response.Headers.Add("Location", session.Url);

            // Return a 303 status code to redirect the user to the Stripe checkout page
            return new HttpStatusCodeResult(303);

            //******************************************************
        }

        /// <summary>
        /// Confirms the order after successful payment.
        /// </summary>
        /// <param name="orderId">The ID of the order to confirm.</param>
        /// <returns>The order confirmation view, or an error view if confirmation fails.</returns>
        public async Task<ActionResult> OrderConfirmed(int? orderId)
        {
            try
            {
                // Get user by id
                string userId = User.Identity.GetUserId();
                User user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // Get the specific user's basket. Create one if it doesn't exist.
                Basket basket = await db.Baskets
                    .Include("BasketItems.Product.Category")
                    .Where(b => b.User.Id == userId)
                    .FirstOrDefaultAsync();

                // Get the order recently created order
                Order order = await db.Orders
                    .Include("ShippingInfo")
                    .Include("OrderLines.Product.Category")
                    .Where(o => o.User.Id == userId)
                    .FirstAsync(o => o.OrderId == orderId);


                // Change order status to order pending
                // Awaiting the Warehouse assistance to dispatch
                order.Status = Status.Pending;

                // Create payment object
                Payment payed = new Payment
                {
                    TotalAmount = order.TotalAmount,
                    Paid = true,
                    IsRefunded = false,
                    Order = order
                };

                // Assign payment to order
                order.Payment = payed;

                // Create Invoice (from models not stripe)
                Models.Invoice invoice = new Models.Invoice
                {
                    InvoiceDate = DateTime.Now,
                    ReceiptsVoucher = "18KU-62IIK",
                    Order = order
                };

                // Assign invoice to order object
                order.Invoice = invoice;

                // Remove the basket from the database
                db.Baskets.Remove(basket); //deal with this

                // Add the invoice to the db
                db.Invoices.Add(invoice);

                // dd the payments to the db
                db.Payments.Add(payed);

                // Make saves to the db
                await db.SaveChangesAsync();

                //***************************   !DEALS WITH SMS AND EMAILS!   *****************************************
                //***************************   !!UNCOMMENT TO TEST FUNCTIONALITY!!   *********************************

                //// Send SMS notification if the user has a phone number
                //if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                //{
                //    var message = new IdentityMessage
                //    {
                //        Destination = user.PhoneNumber,
                //        Body = "Your order has been confirmed. Order ID: " + order.OrderId
                //    };
                //    await UserManager.SmsService.SendAsync(message);
                //}

                //// Generate PDF Invoice
                //var pdfPath = CreatePdfInvoice(order);

                //// Send an email with the generated PDF
                //await SendEmailWithPdf(user.Email, pdfPath);


                // Save session state
                Session["BasketSize"] = 0;

                // Throw order object to the view
                return View(order);
            }

            // Catch any exception errors
            catch (Exception ex)
            {
                return View("TryCatchError", ex);
            }
        }

        //*************************************************************************************
        //*******************************   NORMAL METHODS  ***********************************
        //*************************************************************************************

        /// <summary>
        /// Converts basket items to orderlines.
        /// </summary>
        /// <param name="basketItems">The list of basket items.</param>
        /// <returns>A list of orderlines.</returns>
        private List<OrderLine> ConvertBasketItems(List<BasketItem> basketItems)
        {
            var orderLines = new List<OrderLine>();

            // Iterate through each basket item to convert it to an orderline
            foreach (var item in basketItems)
            {
                // Create a new orderline from the basket item
                var orderLine = new OrderLine
                {
                    Quantity = item.Quantity,
                    LineTotal = item.ItemTotal,
                    Product = item.Product
                };

                // Add the orderline to the list
                orderLines.Add(orderLine);
            }

            // Return the list of orderlines
            return orderLines;
        }

        /// <summary>
        /// Creates a PDF invoice for the given order.
        /// </summary>
        /// <param name="order">The order for which to create an invoice.</param>
        /// <returns>The file path of the created PDF invoice.</returns>
        private string CreatePdfInvoice(Order order)
        {
            // Define the file path for the PDF invoice
            string fileName = Path.Combine(Server.MapPath("~/App_Data"), $"Invoice_{order.OrderId}.pdf");

            // Create a new PDF document
            using (var document = new Document())
            {
                // Initialize the PDF writer with the file path
                PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                document.Open(); // Open the document for writing

                // Add invoice content to the document
                document.Add(new Paragraph("Invoice"));
                document.Add(new Paragraph($"Order ID: {order.OrderId}"));
                document.Add(new Paragraph($"Date: {DateTime.Now}"));
                document.Add(new Paragraph($"Total: {order.TotalAmount}"));

                // Close the document
                document.Close();
            }

            // Return the file path of the created PDF invoice
            return fileName;
        }

        /// <summary>
        /// Sends an email with the attached PDF invoice to the user.
        /// </summary>
        /// <param name="userEmail">The email address of the user.</param>
        /// <param name="filePath">The file path of the PDF invoice.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SendEmailWithPdf(string userEmail, string filePath)
        {
            //// Initialize the SendGrid client with the API key
            //var apiKey = "";
            //var client = new SendGridClient(apiKey);

            //// Set up the email sender and recipient
            //var from = new EmailAddress("mr.kieranburns@gmail.com", "Kieran");
            //var subject = "Your Order Invoice";
            //var to = new EmailAddress(userEmail);

            //// Set up the email content
            //var plainTextContent = "Here is your order invoice attached.";
            //var htmlContent = "<strong>Here is your order invoice attached.</strong>";

            //// Create the email message with SendGrid
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            //// Attach the PDF invoice to the email
            //using (var fileStream = System.IO.File.OpenRead(filePath))
            //{
            //    await msg.AddAttachmentAsync(Path.GetFileName(filePath), fileStream);
            //    var response = await client.SendEmailAsync(msg);
            //}

            //// Optionally, delete the file if you no longer need it after sending
            //System.IO.File.Delete(filePath);
        }
    }
}