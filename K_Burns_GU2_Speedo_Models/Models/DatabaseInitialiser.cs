using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    /// <summary>
    /// Database initializer for creating and seeding the SpeedoModels database.
    /// </summary>
    public class DatabaseInitialiser : DropCreateDatabaseAlways<SpeedoModelsDbContext>
    {
        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        /// <param name="context">The database context.</param>
        protected override void Seed(SpeedoModelsDbContext context)
        {
            base.Seed(context);

            //if there are no records stored in the Users table
            if(!context.Users.Any())
            {
                //Create some the roles and storing them in the Roles table

                //We need a RoleManager to create and store roles
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //if the Admin role doesn't exist
                if(!roleManager.RoleExists("Admin"))
                {
                    //then we create an admin role
                    roleManager.Create(new IdentityRole("Admin"));
                }

                //if the Sales Assistant role doesn't exist
                if (!roleManager.RoleExists("Sales Assistant"))
                {
                    //then we create an Sales Assistant role
                    roleManager.Create(new IdentityRole("Sales Assistant"));
                }

                //if the Invoice Clerk role doesn't exist
                if (!roleManager.RoleExists("Invoice Clerk"))
                {
                    //then we create an Invoice Clerk role
                    roleManager.Create(new IdentityRole("Invoice Clerk"));
                }

                //if the Sales Manager role doesn't exist
                if (!roleManager.RoleExists("Sales Manager"))
                {
                    //then we create an Sales Manager role
                    roleManager.Create(new IdentityRole("Sales Manager"));
                }

                //if the Assistant Manager role doesn't exist
                if (!roleManager.RoleExists("Assistant Manager"))
                {
                    //then we create an Assistant Manager role
                    roleManager.Create(new IdentityRole("Assistant Manager"));
                }

                //if the Stock Control Manager role doesn't exist
                if (!roleManager.RoleExists("Stock Control Manager"))
                {
                    //then we create an Stock Control Manager role
                    roleManager.Create(new IdentityRole("Stock Control Manager"));
                }

                //if the Warehouse Assistant role doesn't exist
                if (!roleManager.RoleExists("Warehouse Assistant"))
                {
                    //then we create an Warehouse Assistant role
                    roleManager.Create(new IdentityRole("Warehouse Assistant"));
                }

                //if the Customer role doesn't exist
                if (!roleManager.RoleExists("Customer"))
                {
                    //then we create an Warehouse Assistant role
                    roleManager.Create(new IdentityRole("Customer"));
                }

                context.SaveChanges();

                //******************************************************************************
                //******************************CREATE USERS************************************
                //******************************************************************************

                //we need a UserManager to create and store our users (Customer & Employees)
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

                //create an ADMIN
                //first check to see if the admin exists in the database
                if (userManager.FindByName("admin@SpeedoModels.com") == null)
                {
                    //create simple password for testing
                    userManager.PasswordValidator = new PasswordValidator
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireUppercase = false,
                        RequireNonLetterOrDigit = false
                    };

                    //create an Admin User
                    var admin = new Employee
                    {
                        UserName = "admin@SpeedoModels.com",
                        Email = "admin@SpeedoModels.com",
                        Forename = "Adam",
                        Surname = "Smith",
                        Street = "123 Backbone St",
                        City = "Glasgow",
                        Postcode = "G6 8LP",
                        RegisteredAt = DateTime.Now.AddYears(-3),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 100385M,
                        EmployementStatus = EmployementStatus.FullTime,
                        PhoneNumber = "07283742267"
                    };

                    //add the admin to the Users table
                    userManager.Create(admin, "a");
                    //assign it to the Admin role
                    userManager.AddToRole(admin.Id, "Admin");

                    //create some Sales Assistant Users
                    var salesAssistant1 = new Employee
                    {
                        UserName = "Employee2@SpeedoModels.com",
                        Email = "Employee2@SpeedoModels.com",
                        Forename = "Tommy",
                        Surname = "Peterson",
                        Street = "14 Cloud St",
                        City = "Edinburgh",
                        Postcode = "E5 8E7",
                        RegisteredAt = DateTime.Now.AddYears(-1),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 29999M,
                        EmployementStatus = EmployementStatus.PartTime,
                        PhoneNumber = "07283745643"
                    };

                    //check if the Sales Assistant doesn't exists in the database
                    if (userManager.FindByName("Tom@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Sales Assistant to the Users table
                        userManager.Create(salesAssistant1, "sa");
                        //assign it to the Sales Assistant role
                        userManager.AddToRole(salesAssistant1.Id, "Sales Assistant");
                    }

                    var salesAssistant2 = new Employee
                    {
                        UserName = "Jack@SpeedoModels.com",
                        Email = "Jack@SpeedoModels.com",
                        Forename = "Jack",
                        Surname = "Donald",
                        Street = "17 Air Avenue",
                        City = "Edinburgh",
                        Postcode = "E4 83T",
                        RegisteredAt = DateTime.Now.AddYears(-2),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 29999M,
                        EmployementStatus = EmployementStatus.FullTime,
                        PhoneNumber = "07283435698"
                    };

                    //check if the Sales Assistant doesn't exists in the database
                    if (userManager.FindByName("Jack@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Sales Assistant to the Users table
                        userManager.Create(salesAssistant2, "sa2");
                        //assign it to the Sales Assistant role
                        userManager.AddToRole(salesAssistant2.Id, "Sales Assistant");
                    }

                    //create some Invoice Clerk Users
                    var invoiceClerk1 = new Employee
                    {
                        UserName = "Sarah@SpeedoModels.com",
                        Email = "Sarah@SpeedoModels.com",
                        Forename = "Sarah",
                        Surname = "Brown",
                        Street = "2 Care Ave",
                        City = "Glasgow",
                        Postcode = "G4 43R",
                        RegisteredAt = DateTime.Now.AddMonths(-6),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 19999M,
                        EmployementStatus = EmployementStatus.PartTime,
                        PhoneNumber = "07563245643"
                    };

                    //check if the Invoice Clerk doesn't exists in the database
                    if (userManager.FindByName("Sarah@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Invoice Clerk to the Users table
                        userManager.Create(invoiceClerk1, "ic");
                        //assign it to the Invoice Clerk role
                        userManager.AddToRole(invoiceClerk1.Id, "Invoice Clerk");
                    }

                    var invoiceClerk2 = new Employee
                    {
                        UserName = "Jeff@SpeedoModels.com",
                        Email = "Jeff@SpeedoModels.com",
                        Forename = "Jeff",
                        Surname = "Byres",
                        Street = "45 Cocunut St",
                        City = "Glasgow",
                        Postcode = "G7 7GH",
                        RegisteredAt = DateTime.Now.AddMonths(-9),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 19999M,
                        EmployementStatus = EmployementStatus.FullTime,
                        PhoneNumber = "07637482913"
                    };

                    //check if the Invoice Clerk doesn't exists in the database
                    if (userManager.FindByName("Jeff@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Invoice Clerk to the Users table
                        userManager.Create(invoiceClerk2, "ic2");
                        //assign it to the Invoice Clerk role
                        userManager.AddToRole(invoiceClerk2.Id, "Invoice Clerk");
                    }

                    //create a Sales Manager Users
                    var salesManager = new Employee
                    {
                        UserName = "Gill@SpeedoModels.com",
                        Email = "Gill@SpeedoModels.com",
                        Forename = "Gill",
                        Surname = "Morrison",
                        Street = "156 Uptown st",
                        City = "Glasgow",
                        Postcode = "G4 55T",
                        RegisteredAt = DateTime.Now.AddYears(-2),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 49999M,
                        EmployementStatus = EmployementStatus.FullTime,
                        PhoneNumber = "07445252343"
                    };

                    //check if the Sales Manager doesn't exists in the database
                    if (userManager.FindByName("Gill@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Sales Manager to the Users table
                        userManager.Create(salesManager, "sm");
                        //assign it to the Sales Manager role
                        userManager.AddToRole(salesManager.Id, "Sales Manager");
                    }

                    //create a Assistant Manager Users
                    var assistantManager = new Employee
                    {
                        UserName = "Gregg@SpeedoModels.com",
                        Email = "Gregg@SpeedoModels.com",
                        Forename = "Gregg",
                        Surname = "Morrison",
                        Street = "157 Uptown st",
                        City = "Glasgow",
                        Postcode = "G4 55T",
                        RegisteredAt = DateTime.Now.AddYears(-3),
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployementStatus = EmployementStatus.FullTime,
                        Salary = 39999M,
                        PhoneNumber = "07445252468"
                    };

                    //check if the Sales Manager doesn't exists in the database
                    if (userManager.FindByName("Gregg@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Sales Manager to the Users table
                        userManager.Create(assistantManager, "am");
                        //assign it to the Sales Manager role
                        userManager.AddToRole(assistantManager.Id, "Assistant Manager");
                    }

                    //create a Stock Control Manager Users
                    var stockControlManager = new Employee
                    {
                        UserName = "Harry@SpeedoModels.com",
                        Email = "Harry@SpeedoModels.com",
                        Forename = "Harry",
                        Surname = "Button",
                        Street = "1 Round st",
                        City = "Glasgow",
                        Postcode = "G1 1TL",
                        RegisteredAt = DateTime.Now.AddMonths(-9),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 39999M,
                        EmployementStatus = EmployementStatus.FullTime,
                        PhoneNumber = "07746372901"
                    };

                    //check if the Stock Control Manager doesn't exists in the database
                    if (userManager.FindByName("Harry@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Stock Control to the Users table
                        userManager.Create(stockControlManager, "scm");
                        //assign it to the Stock Control role
                        userManager.AddToRole(stockControlManager.Id, "Stock Control Manager");
                    }

                    //create some Warehouse Assistant Users
                    var warehouseAssistant1 = new Employee
                    {
                        UserName = "Alana@SpeedoModels.com",
                        Email = "Alana@SpeedoModels.com",
                        Forename = "Alana",
                        Surname = "Duff",
                        Street = "100 Pear st",
                        City = "Edinburgh",
                        Postcode = "E4 14R",
                        RegisteredAt = DateTime.Now.AddMonths(-2),
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployementStatus = EmployementStatus.FullTime,
                        Salary = 19999M,
                        PhoneNumber = "07664738234"
                    };

                    //check if the Warehouse Assistant doesn't exists in the database
                    if (userManager.FindByName("Alana@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Warehouse Assistant to the Users table
                        userManager.Create(warehouseAssistant1, "wa");
                        //assign it to the Warehouse Assistant role
                        userManager.AddToRole(warehouseAssistant1.Id, "Warehouse Assistant");
                    }

                    var warehouseAssistant2 = new Employee
                    {
                        UserName = "Garry@SpeedoModels.com",
                        Email = "Garry@SpeedoModels.com",
                        Forename = "Garry",
                        Surname = "Allan",
                        Street = "14 Tire st",
                        City = "Glasgow",
                        Postcode = "G1 234",
                        RegisteredAt = DateTime.Now.AddMonths(-5),
                        EmailConfirmed = true,
                        IsActive = true,
                        Salary = 19999M,
                        EmployementStatus = EmployementStatus.PartTime,
                        PhoneNumber = "07503926378"
                    };

                    //check if the Warehouse Assistant doesn't exists in the database
                    if (userManager.FindByName("Garry@SpeedoModels.com") == null)
                    {
                        //if not then,
                        //add the Warehouse Assistant to the Users table
                        userManager.Create(warehouseAssistant2, "wa");
                        //assign it to the Warehouse Assistant role
                        userManager.AddToRole(warehouseAssistant2.Id, "Warehouse Assistant");
                    }

                    //create customer users
                    var customer1 = new Customer
                    {
                        UserName = "Paul@gmail.com",
                        Email = "Paul@gmail.com",
                        Forename = "Paul",
                        Surname = "Mcelroy",
                        Street = "153 Pearl st",
                        City = "Glagow",
                        Postcode = "G3 PTH",
                        RegisteredAt = DateTime.Now.AddDays(-15),
                        EmailConfirmed = true,
                        IsActive = true,
                        CustomerType = CustomerType.Premium,
                        PhoneNumber = "07477200050"
                    };

                    //check if the Customer doesn't exists in the database
                    if (userManager.FindByName("Paul@gmail.com") == null)
                    {
                        //if not then,
                        //add the Customer to the Users table
                        userManager.Create(customer1, "c");
                        //assign it to the Customer role
                        userManager.AddToRole(customer1.Id, "Customer");
                    }

                    var customer2 = new Customer
                    {
                        UserName = "CollectorAgency@gmail.com",
                        Email = "CollectorAgency@gmail.com",
                        Forename = "James",
                        Surname = "Bell",
                        Street = "111 Squirel St",
                        City = "Glagow",
                        Postcode = "G4 5TF",
                        RegisteredAt = DateTime.Now.AddMonths(-2),
                        EmailConfirmed = true,
                        IsActive = true,
                        CustomerType = CustomerType.Corporate,
                        PhoneNumber = "020 12134322"
                    };

                    //check if the Customer doesn't exists in the database
                    if (userManager.FindByName("CollectorAgency@gmail.com") == null)
                    {
                        //if not then,
                        //add the Customer to the Users table
                        userManager.Create(customer2, "c2");
                        //assign it to the Customer role
                        userManager.AddToRole(customer2.Id, "Customer");
                    }

                    var customer3 = new Customer
                    {
                        UserName = "Liam@gmail.com",
                        Email = "Liam@gmail.com",
                        Forename = "Liam",
                        Surname = "Chelsea",
                        Street = "1 Care View",
                        City = "Edinbrugh",
                        Postcode = "E4 TF1",
                        RegisteredAt = DateTime.Now.AddMonths(-4),
                        EmailConfirmed = true,
                        IsActive = true,
                        CustomerType = CustomerType.Standard,
                        PhoneNumber = "07736251110"
                    };

                    //check if the Customer doesn't exists in the database
                    if (userManager.FindByName("Liam@gmail.com") == null)
                    {
                        //if not then,
                        //add the Customer to the Users table
                        userManager.Create(customer3, "c3");
                        //assign it to the Customer role
                        userManager.AddToRole(customer3.Id, "Customer");
                    }

                    //Save changes in the Database
                    context.SaveChanges();

                    //***********************************************************************************
                    //******************************CREATE CATEGORIES************************************
                    //***********************************************************************************

                    var cars = new Category { CategoryName = "Cars" };
                    var tracks = new Category { CategoryName = "Tracks" };
                    var standardSets = new Category { CategoryName = "Standard Sets" };
                    var childSets = new Category { CategoryName = "Child Sets" };
                    var limitedEditionSets = new Category { CategoryName = "Limited Edition Sets" };
                    var tools = new Category { CategoryName = "Tools" };

                    //adding the created category objects to the db sets
                    context.Categories.Add(cars);
                    context.Categories.Add(tracks);
                    context.Categories.Add(standardSets);
                    context.Categories.Add(childSets);
                    context.Categories.Add(limitedEditionSets);
                    context.Categories.Add(tools);

                    //save the changes in the database
                    context.SaveChanges();

                    //***********************************************************************************
                    //******************************CREATE PRODUCTS**************************************
                    //***********************************************************************************

                    //creating a few products that belong in the CARS category
                    var childScalextric = new Product
                    {
                        ProductName = "Micro Porshe",
                        ProductDescription = "Created for children aged 3+ The 992 is the latest iteration of the Porsche 911 family.",
                        Price = 12.99M,
                        ProductSize = 5,
                        StockLevel = 30,
                        ImageUrl = "/Images/Products/Micro_porshe.PNG",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddDays(-5),
                        Category = cars
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(childScalextric);

                    var superCarScalextric = new Product
                    {
                        ProductName = "Mercedes Benz",
                        ProductDescription = "Regular model 2024 Mercedes-AMG GT type.",
                        Price = 39.99M,
                        ProductSize = 4,
                        StockLevel = 15,
                        ImageUrl = "/Images/Products/Mercedes_benz.png",
                        Discontinued = false,
                        OnSale = true,
                        DateCreated = DateTime.Now.AddDays(-1),
                        Category = cars
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(superCarScalextric);

                    var classicCarsScalextric = new Product
                    {
                        ProductName = "Oldschool Bentley",
                        ProductDescription = "Classic model Bentley 8 Litre 1930 make.",
                        Price = 29.99M,
                        ProductSize = 3,
                        StockLevel = 55,
                        ImageUrl = "/Images/Products/Oldschool_bentley.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddDays(-3),
                        Category = cars
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(classicCarsScalextric);

                    var streetCarsScalextric = new Product
                    {
                        ProductName = "BMW Police Car",
                        ProductDescription = "Street model A familiar, if not always welcome motor.",
                        Price = 49.99M,
                        ProductSize = 8,
                        StockLevel = 25,
                        ImageUrl = "/Images/Products/BMW_police_car.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddDays(-4),
                        Category = cars
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(streetCarsScalextric);

                    //creating a few products that belong in the TRACK category
                    var straightTrack = new Product
                    {
                        ProductName = "Standard Straight",
                        ProductDescription = "350MM x2 1:32 scale.",
                        Price = 10.99M,
                        ProductSize = 35,
                        StockLevel = 60,
                        ImageUrl = "/Images/Products/Standard_straight.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-2),
                        Category = tracks
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(straightTrack);

                    var curvedTrack = new Product
                    {
                        ProductName = "Curved track",
                        ProductDescription = "45 degrees angle x2 1:32 scale.",
                        Price = 5.99M,
                        ProductSize = 30,
                        StockLevel = 65,
                        ImageUrl = "/Images/Products/Curved_track.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-2),
                        Category = tracks
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(curvedTrack);

                    var borderTrack = new Product
                    {
                        ProductName = "Track Border",
                        ProductDescription = "Comes as a bundle of both inner and out borders for both straight and curved tracks.",
                        Price = 15.99M,
                        ProductSize = 45,
                        StockLevel = 120,
                        ImageUrl = "/Images/Products/Track_border.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-5),
                        Category = tracks
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(borderTrack);

                    var extensionTrack = new Product
                    {
                        ProductName = "Track Extension",
                        ProductDescription = "Flexible design, great addition to any scalextrix set.",
                        Price = 40.99M,
                        ProductSize = 15,
                        StockLevel = 45,
                        ImageUrl = "/Images/Products/Extension_set.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-5),
                        Category = tracks
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(extensionTrack);

                    //creating a product that belong in the TOOLS category
                    var controller = new Product
                    {
                        ProductName = "Controller",
                        ProductDescription = "Adjustable Analogue Hand Throttle.",
                        Price = 19.99M,
                        ProductSize = 12,
                        StockLevel = 100,
                        ImageUrl = "/Images/Products/Controller.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-3),
                        Category = tools
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(controller);

                    //creating a product that belong in the STANDARD SET category
                    var standardSet = new Product
                    {
                        ProductName = "POLICE CHASE SET",
                        ProductDescription = "2 police cars, 4 robber cars, controllers and all track types (curved, straight, extensions).",
                        Price = 129.99M,
                        ProductSize = 40,
                        StockLevel = 45,
                        ImageUrl = "/Images/Products/Police_chase.png",
                        Discontinued = false,
                        OnSale = true,
                        DateCreated = DateTime.Now.AddMonths(-4),
                        Category = standardSets
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(standardSet);

                    //creating a product that belong in the CHILD SET category
                    var childSet = new Product
                    {
                        ProductName = "SPIDER SET",
                        ProductDescription = "4 Spider car, 4 goblin car, controllers and all track types (curved, straight, extensions).",
                        Price = 49.99M,
                        ProductSize = 35,
                        StockLevel = 0,
                        ImageUrl = "/Images/Products/Child_Spider_set.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-4),
                        Category = childSets
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(childSet);

                    //creating a product that belong in the LIMITED EDITION SET category
                    var limitedEditionSet = new Product
                    {
                        ProductName = "RACER CARS SET",
                        ProductDescription = "25 rare racer cars, controllers + advanced app function control and all track types (curved, straight, extensions) and classic grandstand building.",
                        Price = 599.99M,
                        ProductSize = 55,
                        StockLevel = 10,
                        ImageUrl = "/Images/Products/Limited_racer_set.png",
                        Discontinued = false,
                        OnSale = true,
                        DateCreated = DateTime.Now.AddMonths(-2),
                        Category = limitedEditionSets
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(limitedEditionSet);

                    //creating a few products that belong in the TOOLS category
                    var building = new Product
                    {
                        ProductName = "Pit Garage",
                        ProductDescription = "1:32 scale, every scalextrix cars need.",
                        Price = 29.99M,
                        ProductSize = 100,
                        StockLevel = 45,
                        ImageUrl = "/Images/Products/Pit_garage.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-2),
                        Category = tools
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(building);

                    //creating a few products that belong in the TOOLS category
                    var spares = new Product
                    {
                        ProductName = "Tyres pack",
                        ProductDescription = "Pack of 4 tyres, coupled with a pit garage and your all set to go.",
                        Price = 4.99M,
                        ProductSize = 2,
                        StockLevel = 70,
                        ImageUrl = "/Images/Products/Tyres_pack.png",
                        Discontinued = false,
                        OnSale = false,
                        DateCreated = DateTime.Now.AddMonths(-3),
                        Category = tools
                    };

                    //adding the created product objects to the db sets
                    context.Products.Add(spares);

                    //save the changes in the database
                    context.SaveChanges();

                    //***********************************************************************************
                    //******************************PARCEL***********************************************
                    //***********************************************************************************

                    var parcel = new Parcel
                    {
                        Weight = 100,
                        Width = 5,
                        Height = 10,
                        Length = 15,
                        ParcelType = ParcelType.Small,
                        Method = Method.RoyalMail
                    };

                    //adding the created parcel object to the db sets
                    context.Parcels.Add(parcel);

                    //***********************************************************************************
                    //******************************ORDERS***********************************************
                    //***********************************************************************************
                    var order = new Order
                    {
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5),
                        Status = Status.Delivered,
                        User = customer1,
                        Parcel = parcel
                    };

                    //adding the order to the db
                    context.Orders.Add(order);

                    //save the changes in the database
                    context.SaveChanges();  

                    //***********************************************************************************
                    //******************************SHIPPING INFO****************************************
                    //***********************************************************************************
                    var shipping = new ShippingInfo
                    {
                        ShippingCost = 2.99M,
                        ShippingToHomeAddress = true,
                        ShippingStreet = customer1.Street,
                        ShippingCity = customer1.City,
                        ShippingPostcode = customer1.Postcode,
                    };


                    //***********************************************************************************
                    //******************************ORDERLINES*******************************************
                    //***********************************************************************************
                    var orderLine1 = new OrderLine
                    {
                        Quantity = 3,
                        LineTotal = childScalextric.Price*3,
                        Product = childScalextric
                    };

                    //adding the created orderline object to the db
                    context.OrderLines.Add(orderLine1);

                    var orderLine2 = new OrderLine
                    {
                        Quantity = 1,
                        LineTotal = classicCarsScalextric.Price,
                        Product = classicCarsScalextric
                    };

                    //adding the created orderline object to the db
                    context.OrderLines.Add(orderLine2);

                    //***********************************************************************************
                    //******************************PAYMENTS*********************************************
                    //***********************************************************************************

                    //create a payment that has been made
                    var payment = new Payment
                    {
                        TotalAmount = orderLine1.LineTotal+orderLine2.LineTotal + shipping.ShippingCost,
                        Paid = true,
                        IsRefunded = false
                    };


                    //***********************************************************************************
                    //******************************INVOICE*********************************************
                    //***********************************************************************************
                    var invoice = new Invoice
                    {
                        InvoiceDate = DateTime.Now,
                        ReceiptsVoucher = "18KU-62IIK"
                    };

                    //***********************************************************************************
                    //******************************ORDERS UPDATED***************************************
                    //***********************************************************************************
                    order.OrderTotal = payment.TotalAmount - shipping.ShippingCost;
                    order.TotalAmount = payment.TotalAmount;
                    order.ShippingInfo = shipping;
                    order.OrderLines = new List<OrderLine> { orderLine1, orderLine2 };
                    order.Payment = payment;
                    order.Invoice = invoice;

                    shipping.Order = order;
                    payment.Order = order;
                    invoice.Order = order;


                    //updating the order, shipping, payment and invoice
                    //in the db
                    context.Orders.AddOrUpdate(order);
                    context.ShippingInfos.AddOrUpdate(shipping);
                    context.Payments.AddOrUpdate(payment);
                    context.Invoices.AddOrUpdate(invoice);

                    //save the changes in the database
                    context.SaveChanges();

                }
            }
        }
    }
}