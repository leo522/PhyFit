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

        [Required]
        [Display(Name = "新密碼")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "確認新密碼")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
        public string ConfirmPassword { get; set; }
    }
}