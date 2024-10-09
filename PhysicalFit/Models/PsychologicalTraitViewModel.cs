using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class PsychologicalTraitViewModel
    {
        public int UserID { get; set; }
        public DateTime PsychologicalDate { get; set; }
        public string Trait { get; set; }
        public string Feeling { get; set; }
        public int Score { get; set; }
    }
}