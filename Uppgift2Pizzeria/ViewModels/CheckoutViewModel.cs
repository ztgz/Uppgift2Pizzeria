using System.Collections.Generic;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class CheckoutViewModel
    {
        public List<Matratt> Meals { get; set; }
        public Kund User { get; set; }
        public int BonusPointsAdded { get; set; }
        public int BonusPointsRemoved { get; set; }
        public int Discount { get; set; }
        public List<Matratt> FreeMeals { get; set; }

        public CheckoutViewModel()
        {
            BonusPointsAdded = 0;
            BonusPointsRemoved = 0;
            Discount = 0;
        }
    }
}
