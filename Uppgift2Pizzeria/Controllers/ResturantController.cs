using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Controllers
{
    public class ResturantController : Controller
    {
        private const string SessionUsername = "_Name";
        private const string SessionBasket = "_Basket";
        private const string SessionRedirectTo = "_Redirect";

        private TomasosContext _context;

        public ResturantController(TomasosContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoggIn()
        {
            if (IsLoggedIn())
                return RedirectToAction("MyAccount");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoggIn(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                var kund = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == userModel.AnvandarNamn);

                //If there is a kund with username and password...
                if (kund != null && kund.Losenord == userModel.Losenord)
                {
                    //..store username in session
                    HttpContext.Session.SetString(SessionUsername, kund.AnvandarNamn);

                    //Get view to redirect to...
                    string toView = HttpContext.Session.GetString(SessionRedirectTo);
                                        
                    //... if it is empty set standard
                    if (String.IsNullOrEmpty(toView))
                        toView = "MyAccount";

                    //...and redirect to view
                    return RedirectToAction(toView);
                }
                else
                {
                    ModelState.AddModelError("", "Användarnamn eller lösenord stämmer inte");
                }
            }

            return View("LoggIn");
        }

        public IActionResult LoggOut()
        {
            LoggOutAccount();

            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(Kund formKund)
        {
            //If logged in: logg out account
            if (IsLoggedIn())
            {
                LoggOutAccount();
            }

            //It the model is not vaild
            if (!ModelState.IsValid)
            {
                return View();
            }
            //If there a user with the username already exsist
            if (_context.Kund.SingleOrDefault(k => k.AnvandarNamn == formKund.AnvandarNamn) != null)
            {
                ModelState.AddModelError("", "Användarnamn är upptaget");
                return View();
            }

            //Create user
            var kund = new Kund()
            {
                AnvandarNamn = formKund.AnvandarNamn,
                Losenord = formKund.Losenord,
                Namn = formKund.Namn,
                Email = formKund.Email,
                Gatuadress = formKund.Gatuadress,
                Postnr = formKund.Gatuadress,
                Postort = formKund.Postort,
                Telefon = formKund.Gatuadress
            };

            _context.Kund.Add(kund);

            _context.SaveChanges();

            return RedirectToAction("LoggIn");
        }

        public IActionResult MyAccount()
        {
            //If logged in...
            if (IsLoggedIn())
            {
                //...try to get Kund from database
                var model = _context.Kund.FirstOrDefault(k => k.AnvandarNamn == GetUsernname());

                //Open my pages if a valid account is found
                if (model != null)
                    return View(model);
            }

            return View("LoggIn");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MyAccount(Kund formKund)
        {
            if (ModelState.IsValid)
            {
                var kund = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == formKund.AnvandarNamn);

                if (kund != null)
                {
                    kund.Losenord = formKund.Losenord;
                    kund.Namn = formKund.Namn;
                    kund.Gatuadress = formKund.Gatuadress;
                    kund.Postnr = formKund.Postnr;
                    kund.Postort = formKund.Postort;
                    kund.Email = formKund.Email;
                    kund.Telefon = formKund.Telefon;

                    _context.SaveChanges();
                }

                return RedirectToAction("UserUpdated");

            }

            //Reload MyAccount with new settings
            return RedirectToAction("MyAccount");
        }

        public IActionResult UserUpdated()
        {
            if (!IsLoggedIn())
            {
                return RedirectToLoggin("MyAccount");
            }

            return View();
        }

        public IActionResult Menu()
        {
            //Create a model for the view
            var model = new List<MenuModel>();

            //Get all the meals
            var meals = _context.Matratt.Include(m => m.MatrattProdukt).ToList();

            //For all the meals..
            foreach (var meal in meals)
            {
                //...create a new menuItem ...
                MenuModel menuItem = new MenuModel();

                //... and add the meal to the menuItem...
                menuItem.Meal = meal;

                //... find all the products for the meal...
                foreach (var mealProduct in meal.MatrattProdukt)
                {
                    Produkt prod = _context.Produkt.FirstOrDefault(p => p.ProduktId == mealProduct.ProduktId);

                    //... and add them to the menu item...
                    if (prod != null)
                        menuItem.Ingridients.Add(prod);
                }

                //... ad the menu item to the model
                model.Add(menuItem);
            }

            return View(model);
        }

        public IActionResult OrderMeal(int id)
        {
            //Get the meal that will be in the basket
            var meal = _context.Matratt.SingleOrDefault(m => m.MatrattId == id);

            //if meal was found...
            if (meal != null)
            {
                //...Get basket... 
                List<Matratt> basket = GetBasket();

                //... add meal to basket...
                basket.Add(meal);

                //... and save basket
                SaveBasket(basket);
            }

            //Reload menu
            return RedirectToAction("Menu");
        }

        public IActionResult Checkout()
        {
            if (!IsLoggedIn())
            {
                return RedirectToLoggin("Checkout");
            }
            
            return View(GetBasket());
        }

        public IActionResult SendOrder()
        {
            if (!IsLoggedIn())
                return RedirectToLoggin("Checkout");

            var basket = GetBasket();

            if (basket.Count > 0)
            {
                Bestallning order = new Bestallning();

                //Set the customer who orded the food
                order.Kund = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == GetUsernname());

                //Set the price of the order
                order.Totalbelopp = basket.Sum(b => b.Pris);

                //Date of order
                order.BestallningDatum = DateTime.Now;
                
                //Order not yet delivered
                order.Levererad = false;
                
                //Add order to database
                _context.Add(order);
                _context.SaveChanges();

                //Add meals to order as long as basket is not empty
                while (basket.Count > 0)
                {
                    //Number of the specific meal in the basket
                    int count = basket.Count(b => b.MatrattId == basket[0].MatrattId);
                    
                    //Create new meal in order
                    BestallningMatratt bestallningMatratt = new BestallningMatratt
                    {
                        Antal = count,
                        MatrattId = basket[0].MatrattId,
                        BestallningId = order.BestallningId
                    };

                    //Add to context 
                    _context.Add(bestallningMatratt);

                    //Remove all of the added meals from basket
                    basket = basket.Where(b => b.MatrattId != basket[0].MatrattId).ToList();
                }

                //Save to database
                _context.SaveChanges();

                //Clear basket...
                basket = new List<Matratt>();
                
                //...and save basket
                SaveBasket(basket);
            }

            return RedirectToAction("Confirmed");
        }

        public IActionResult Confirmed()
        {
            return View();
        }

        public IActionResult EmptyBasket()
        {
            SaveBasket(new List<Matratt>());

            return RedirectToAction("Checkout");
        }

        public IActionResult RemoveItemFromBasket(int id)
        {
            List<Matratt> meals = GetBasket();

            for (int i = 0; i < meals.Count; i++)
            {
                if (meals[i].MatrattId == id)
                {
                    meals.RemoveAt(i);

                    SaveBasket(meals);
                }
            }

            return RedirectToAction("Checkout");
        }

        private IActionResult RedirectToLoggin(string toView)
        {
            if (toView is null)
                toView = "MyAccount";

            HttpContext.Session.SetString(SessionRedirectTo, toView);

            return RedirectToAction("LoggIn");
        }

        private bool IsLoggedIn()
        {
            //Try to recive username
            string username = GetUsernname();

            //User is logged in if it has a username
            return !String.IsNullOrWhiteSpace(username);
        }

        private void LoggOutAccount()
        {
            HttpContext.Session.SetString(SessionUsername, "");
        }

        private string GetUsernname()
        {
            return HttpContext.Session.GetString(SessionUsername);
        }

        private List<Matratt> GetBasket()
        {
            List<Matratt> basket;

            var basketStr = HttpContext.Session.GetString(SessionBasket);

            if (basketStr != null)
            {
                basket = JsonConvert.DeserializeObject<List<Matratt>>(basketStr);
            }
            else
            {
                basket = new List<Matratt>();
            }

            return basket;
        }

        private void SaveBasket(List<Matratt> basket)
        {
            //Serialize basket
            var serialized = JsonConvert.SerializeObject(basket);
            
            //and save it in session
            HttpContext.Session.SetString(SessionBasket, serialized);
        }
    }
}