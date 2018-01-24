using System.ComponentModel.DataAnnotations;

namespace Uppgift2Pizzeria.ViewModels
{
    public class IngridentViewModel
    {
        [Required(ErrorMessage = "Ett namn måste anges")]
        public string Name { get; set; }
    }
}
