using System.Collections.Generic;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class CheckoutViewModel
    {
        public List<Matratt> Meals { get; set; }
        public Kund User { get; set; }
    }
}
