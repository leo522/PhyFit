using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit
{
    public class TrainingLoadResult
    {
        public bool IsSuccess { get; set; }
        public int TrainingLoadSum { get; set; }
        public string ErrorMessage { get; set; }
    }
}