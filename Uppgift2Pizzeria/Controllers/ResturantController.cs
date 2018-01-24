using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Controllers
{
    public class ResturantController : Controller
    {
        private const string SessionBasket = "_Basket";

        private TomasosContext _context;

        public ResturantController(TomasosContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

         public IActionResult Menu()
        {
            //Create a model for the view
            var model = new List<MenuViewModel>();

            //Get all the meals
            var meals = _context.Matratt.Include(m => m.MatrattProdukt).ToList();

            //For all the meals..
            foreach (var meal in meals)
            {
                //...create a new menuItem ...
                MenuViewModel menuItem = new MenuViewModel();

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

        [Authorize]
        public IActionResult Checkout()
        {
            CheckoutViewModel model = new CheckoutViewModel();
            model.Meals = GetBasket();

            //Get information about logged in user, contains name and bonus points
            model.User = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == GetUsernname());

            return View(model);
        }

        [Authorize]
        public IActionResult SendOrder()
        {
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

        [Authorize]
        public IActionResult Confirmed()
        {
            return View();
        }

        [Authorize]
        public IActionResult EmptyBasket()
        {
            SaveBasket(new List<Matratt>());

            return RedirectToAction("Checkout");
        }

        [Authorize]
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

        private string GetUsernname()
        {
            return HttpContext.User.Identity.Name;
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