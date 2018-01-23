using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly TomasosContext _context;

        public AdminController(TomasosContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        public IActionResult CreatePizza()
        {
            var model = new CreatePizzaViewModel();

            model.FoodTypes = new List<SelectListItem>();

            foreach (var type in _context.MatrattTyp)
            {
                model.FoodTypes.Add(new SelectListItem
                {
                    Value = type.MatrattTyp1.ToString(),
                    Text = type.Beskrivning
                });
            }

            model.Products = new List<SelectListItem>();

            foreach (var product in _context.Produkt)
            {
                model.Products.Add(new SelectListItem
                {
                    Value = product.ProduktId.ToString(),
                    Text = product.ProduktNamn
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePizza(CreatePizzaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Matratt meal = vm.Meal;

                _context.Matratt.Add(meal);

                _context.SaveChanges();


                foreach (var ingridientId in vm.SelectedIngridients)
                {
                    //try to find the product
                    Produkt product = _context.Produkt.FirstOrDefault(p => p.ProduktId == int.Parse(ingridientId));

                    if (product != null)
                    {
                        MatrattProdukt mp = new MatrattProdukt()
                        {
                            MatrattId = meal.MatrattId,
                            ProduktId = int.Parse(ingridientId)
                        };

                        _context.MatrattProdukt.Add(mp);
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult RemoveMeal(int id)
        {
            var matratt = _context.Matratt.FirstOrDefault(m => m.MatrattId == id);

            if (matratt != null)
            {
                //remove all matrattProdukt that contains the meal
                foreach (var matrattProdukt in _context.MatrattProdukt.Where(m => m.MatrattId == id))
                {
                    _context.MatrattProdukt.Remove(matrattProdukt);
                }

                _context.Remove(matratt);

                _context.SaveChanges();

            }

            return RedirectToAction("Menu", "Resturant");
        }

        public IActionResult EditPizza(int id)
        {
            var model = new CreatePizzaViewModel();

            model.Meal = _context.Matratt.FirstOrDefault(m => m.MatrattId == id);

            model.FoodTypes = new List<SelectListItem>();

            foreach (var type in _context.MatrattTyp)
            {
                model.FoodTypes.Add(new SelectListItem
                {
                    Value = type.MatrattTyp1.ToString(),
                    Text = type.Beskrivning,
                    //selected if meal is of product type
                    Selected = (type.MatrattTyp1 == model.Meal.MatrattId)
                });
            }

            model.Products = new List<SelectListItem>();

            foreach (var product in _context.Produkt.Include(p => p.MatrattProdukt))
            {
                model.Products.Add(new SelectListItem
                {
                    Value = product.ProduktId.ToString(),
                    Text = product.ProduktNamn,
                    //Selected if meal contains the ingridient
                    Selected = product.MatrattProdukt.Count(mp => mp.MatrattId == model.Meal.MatrattId) > 0,
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPizza(CreatePizzaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Matratt meal = _context.Matratt.Include(m => m.MatrattProdukt).FirstOrDefault(m => m.MatrattId == vm.Meal.MatrattId);

                if (meal != null)
                {
                    meal.Pris = vm.Meal.Pris;
                    meal.MatrattTyp = vm.Meal.MatrattTyp;

                    //add missing products to meal
                    foreach (var productId in vm.SelectedIngridients)
                    {
                        if (meal.MatrattProdukt.Count(m => m.ProduktId == int.Parse(productId)) == 0)
                        {
                            var product = _context.Produkt.FirstOrDefault(p => p.ProduktId == int.Parse(productId));
                            MatrattProdukt matrattProdukt = new MatrattProdukt()
                            {
                                Matratt = meal,
                                Produkt = product
                            };

                            _context.MatrattProdukt.Add(matrattProdukt);
                        }
                    }

                    //remove products
                    foreach (MatrattProdukt matrattProdukt in meal.MatrattProdukt)
                    {
                        //if selected ingridients dosen't contain the product...
                        if (!vm.SelectedIngridients.Contains(matrattProdukt.ProduktId.ToString()))
                        {
                            //...remove it
                            _context.MatrattProdukt.Remove(matrattProdukt);
                        }
                    }

                    _context.SaveChanges();
                }

            }

            return RedirectToAction("Menu", "Resturant");
        }

        public IActionResult CreateIngridient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateIngridient(IngridentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (_context.Produkt.Count(p => p.ProduktNamn == vm.Name) == 0)
                {
                    Produkt product = new Produkt
                    {
                        ProduktNamn = vm.Name
                    };

                    _context.Produkt.Add(product);

                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Menu", "Resturant");
        }

        public IActionResult Users()
        {
            List<Kund> model = _context.Kund.Where(k => k.AnvandarNamn != HttpContext.User.Identity.Name).ToList();

            return View(model);
        }

        public IActionResult Orders()
        {
            var model = _context.Bestallning.ToList();

            return View(model);
        }

        public IActionResult OrderDetails(int orderId)
        {
            OrderDetailsViewModel model = new OrderDetailsViewModel();
            model.Bestallning = _context.Bestallning.FirstOrDefault(b => b.BestallningId == orderId);
            model.BestallningMatratt = _context.BestallningMatratt
                .Include(bm => bm.Matratt)
                .Where(bm =>  bm.BestallningId == orderId)
                .ToList();

            return PartialView("_OrderDetails", model);
        }

        public IActionResult RemoveOrder(int orderId)
        {
            //remove the orderdetails
            foreach (BestallningMatratt bm in _context.BestallningMatratt.Where(b => b.BestallningId == orderId))
            {
                _context.Remove(bm);
            }
            _context.SaveChanges();

            //remove the order
            Bestallning order = _context.Bestallning.FirstOrDefault(b => b.BestallningId == orderId);
            _context.Remove(order);
            _context.SaveChanges();


            return RedirectToAction("Orders");
        }

        public IActionResult OrderDelivered(int orderId)
        {
            Bestallning order = _context.Bestallning.FirstOrDefault(b => b.BestallningId == orderId);
            order.Levererad = true;
            _context.SaveChanges();

            return RedirectToAction("Orders");
        }

        public IActionResult ListOfOrders(bool delivered)
        {
            var model = _context.Bestallning.Where(b => b.Levererad == delivered).OrderByDescending(b => b.BestallningDatum).ToList();

            return PartialView("_ListOfOrders", model);
        }
    }
}