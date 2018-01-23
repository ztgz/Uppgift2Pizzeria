using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Uppgift2Pizzeria.ViewModels
{
    public class IngridentViewModel
    {
        [Required(ErrorMessage = "Ett namn måste anges")]
        public string Name { get; set; }
    }
}
