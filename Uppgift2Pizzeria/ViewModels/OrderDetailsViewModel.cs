using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class OrderDetailsViewModel
    {
        public Bestallning Bestallning { get; set; }
        public List<BestallningMatratt> BestallningMatratt { get; set; }
    }
}
