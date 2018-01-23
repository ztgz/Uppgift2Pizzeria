using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.ViewModels
{
    public class UserRoleViewModel
    {
        public Kund User { get; set; }
        public string Role { get; set; }
    }
}
