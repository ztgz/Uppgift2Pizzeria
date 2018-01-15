using System.Collections.Generic;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class MenuModel
    {
        public Matratt Meal { get; set; }
        public List<Produkt> Ingridients;

        public MenuModel()
        {
            Ingridients = new List<Produkt>();
        }
    }
}
