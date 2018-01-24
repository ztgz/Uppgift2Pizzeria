using System.Collections.Generic;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class MenuViewModel
    {
        public Matratt Meal { get; set; }
        public List<Produkt> Ingridients;
        public Kund User { get; set; }

        public MenuViewModel()
        {
            Ingridients = new List<Produkt>();
        }
    }
}
