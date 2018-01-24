using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Uppgift2Pizzeria.Data;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Controllers
{
    public class ResturantController : Controller
    {
        private const string SessionBasket = "_Basket";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private TomasosContext _context;

        //They recive free pizzas for every 100 points 
        private const int FreePizza = 100;

        public ResturantController(UserManager<ApplicationUser> usrMgr,
            SignInManager<ApplicationUser> signInMgr, TomasosContext context)
        {
            _userManager = usrMgr;
            _signInManager = signInMgr;
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
            return View(GetCheckOutModel());
        }

        private CheckoutViewModel GetCheckOutModel()
        {
            CheckoutViewModel model = new CheckoutViewModel();

            model.Meals = GetBasket();

            //Get information about logged in user, contains name and bonus points
            model.User = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == GetUsernname());

            //The amount of bonus points the order would give
            model.BonusPointsAdded = CalculatePoints(model);

            //The amount of bonus points that would be deducted for free pizzas
            model.BonusPointsRemoved = CalculateDeductionPoints(model);

            //Which pizzas that would be free
            model.FreeMeals = GetFreePizzas(model);

            //if premium user buys 3 or more meals
            if(User.IsInRole("PremiumUser") && model.Meals.Count >= 3)
            {
                model.Discount = (int) ( 
                    (model.Meals.Sum(p => p.Pris) - model.FreeMeals.Sum(fm => fm.Pris)) 
                        * 0.2f );
            }

            return model;
        }

        [Authorize]
        public IActionResult SendOrder()
        {
            CheckoutViewModel vm = GetCheckOutModel();

            var basket = GetBasket();

            if (basket.Count > 0)
            {
                Bestallning order = new Bestallning();

                //Get the customer
                Kund user = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == GetUsernname());

                //Change the number of bonus points of customers account if premium user
                if (User.IsInRole("PremiumUser"))
                {
                    user.Poang += vm.BonusPointsAdded - vm.BonusPointsRemoved;
                }

                //_context.SaveChanges();

                //Set the customer who orded the food
                order.Kund = user;
               
                //Set the price of the order
                order.Totalbelopp = vm.Meals.Sum(m => m.Pris) - vm.FreeMeals.Sum(m => m.Pris)-vm.Discount;

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

                    break;
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

        private int CalculatePoints(CheckoutViewModel vm)
        {
            return (vm.Meals.Count * 10);
        }

        private int CalculateDeductionPoints(CheckoutViewModel vm)
        {
            int totalPoints = vm.BonusPointsAdded + vm.User.Poang;

            //There neeeds to be pizzas in order to recive free pizza
            int pizzasInOrder = vm.Meals.Count(m => m.MatrattTyp == 1);

            int pointsToDetuct = 0;

            if (totalPoints / FreePizza > 0 && pizzasInOrder > 0)
            {
                pointsToDetuct = Math.Min(totalPoints / FreePizza, pizzasInOrder);
            }

            return pointsToDetuct*FreePizza;
        }

        private List<Matratt> GetFreePizzas(CheckoutViewModel vm)
        {
            List<Matratt> freeMeals = new List<Matratt>();
            
            //Calculate the number of pizzas that will be deducted
            int numbersOfFree = CalculateDeductionPoints(vm) / FreePizza;

            if (numbersOfFree > 0)
            {
                //Get all the pizzas in the order
                var allPizzas = vm.Meals.Where(m => m.MatrattTyp == 1).OrderBy(m => m.Pris).ToList();

                //Return the free pizzas
                freeMeals = allPizzas.GetRange(0, numbersOfFree);
            }
            return freeMeals;
        }
        
    }
}