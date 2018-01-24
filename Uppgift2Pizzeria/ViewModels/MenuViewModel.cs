using System.Collections.Generic;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class MenuViewModel
    {
        public Matratt Meal { get; set; }
        public List<Produkt> Ingridients;

        public MenuViewModel()
        {
            Ingridients = new List<Produkt>();
        }
    }
}
