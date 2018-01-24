using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Uppgift2Pizzeria.ViewModels
{
    public class UserModel
    {
        [DisplayName("Användarnamn")]
        [Required(ErrorMessage = "Användarnamn är obligatoriskt")]
        [StringLength(20, ErrorMessage = "Användarnamn kan inte vara längre än 20 tecken.")]
        public string AnvandarNamn { get; set; }

        [DisplayName("Lösenord")]
        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [StringLength(20, ErrorMessage = "Lösenord kan inte vara längre än 20 tecken.")]
        [UIHint("Password")]
        public string Losenord { get; set; }
    }
}
