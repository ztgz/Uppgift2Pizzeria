using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class CreatePizzaViewModel
    {
        public List<SelectListItem> FoodTypes { get; set; }
        public Matratt Meal { get; set; }
        public List<SelectListItem> Products { get; set; }
        public List<string> SelectedIngridients { get; set; }
    }
}
