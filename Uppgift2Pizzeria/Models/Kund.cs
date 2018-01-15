using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Uppgift2Pizzeria.Models
{
    public partial class Kund
    {
        public Kund()
        {
            Bestallning = new HashSet<Bestallning>();
        }

        public int KundId { get; set; }

        [DisplayName("Fullständigt namn")]
        [Required(ErrorMessage = "Namn är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Fullständigt namn kan max vara 100 tecken.")]
        public string Namn { get; set; }

        [Required(ErrorMessage = "Gatuadress är obligatoriskt.")]
        [StringLength(50, ErrorMessage = "Gatuadress kan max vara 50 tecken.")]
        public string Gatuadress { get; set; }

        [DisplayName("Postnummer")]
        [Required(ErrorMessage = "Postnummer är obligatoriskt.")]
        [StringLength(20, ErrorMessage = "Postnummer kan max vara 20 tecken.")]
        public string Postnr { get; set; }

        [Required(ErrorMessage = "Postort är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Postort kan max vara 100 tecken.")]
        public string Postort { get; set; }

        [StringLength(50, ErrorMessage = "Email kan max vara 50 tecken.")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Telefon kan max vara 50 tecken.")]
        public string Telefon { get; set; }

        [DisplayName("Användarnamn")]
        [Required(ErrorMessage = "Användarnamn är obligatoriskt")]
        [StringLength(20, ErrorMessage = "Användarnamn kan inte vara längre än 20 tecken.")]
        public string AnvandarNamn { get; set; }

        [DisplayName("Lösenord")]
        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [StringLength(20, ErrorMessage = "Lösenord kan inte vara längre än 20 tecken.")]
        [UIHint("Password")]
        public string Losenord { get; set; }

        public ICollection<Bestallning> Bestallning { get; set; }
    }
}
