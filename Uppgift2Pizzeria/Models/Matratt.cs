using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Uppgift2Pizzeria.Models
{
    public partial class Matratt
    {
        public Matratt()
        {
            BestallningMatratt = new HashSet<BestallningMatratt>();
            MatrattProdukt = new HashSet<MatrattProdukt>();
        }

        public int MatrattId { get; set; }

        [DisplayName("Namn på maträtt")]
        [Required(ErrorMessage = "Namn måste angess på maträtt")]
        public string MatrattNamn { get; set; }
        public string Beskrivning { get; set; }

        [Required(ErrorMessage = "Pris är obligatoriskt")]
        public int Pris { get; set; }

        [DisplayName("Typ av rätt")]
        [Required(ErrorMessage = "Typ av maträtt är olbigatoriskt")]
        public int MatrattTyp { get; set; }

        public MatrattTyp MatrattTypNavigation { get; set; }
        public ICollection<BestallningMatratt> BestallningMatratt { get; set; }
        public ICollection<MatrattProdukt> MatrattProdukt { get; set; }
    }
}
