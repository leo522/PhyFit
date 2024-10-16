using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class ForgotPwdViewModel
    {
        [Required]
        [Display(Name = "身份證號碼")]
        public string AthleteID { get; set; }
    }

    public class ResetPwdViewModel
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}