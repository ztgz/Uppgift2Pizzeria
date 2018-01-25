using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class CreatePizzaViewModel
    {
        public List<SelectListItem> FoodTypes { get; set; }
        public Matratt Meal { get; set; }
        public List<SelectListItem> Products { get; set; }

        [Required(ErrorMessage = "Minst en ingrediens måste väljas")]
        public List<string> SelectedIngridients { get; set; }
    }
}
