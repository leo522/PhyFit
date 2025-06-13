using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class EWMAViewModel
    {
        public int N { get; set; }
        public double Alpha => 2.0 / (N + 1);
        public double EWMAWeighting => 1 - Math.Pow(1 - Alpha, N + 1);
        public double Result { get; set; }

        public void Calculate()
        {
            Result = EWMAWeighting;
        }
    }
}