using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhysicalFit.ViewModels
{
    #region 運動員註冊
    public class AthleteRegisterViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        public string AthleteName { get; set; }

        [Required(ErrorMessage = "請輸入生日")]
        [DataType(DataType.Date)]
        public DateTime? AthleteBirthday { get; set; }

        [Required(ErrorMessage = "請輸入帳號（身份證號）")]
        public string AthleteID { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Athletepwd { get; set; }

        [Required(ErrorMessage = "請再次輸入密碼")]
        [Compare("Athletepwd", ErrorMessage = "兩次輸入的密碼不一致")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string AthleteSchool { get; set; }
        public string AthleteOrganize { get; set; }
        public string AthleteTeam { get; set; }

        public List<int> CoachIDs { get; set; }

        public bool NoCoach { get; set; }

        // 前端用來顯示教練選單
        public List<System.Web.Mvc.SelectListItem> AvailableCoaches { get; set; }
    }
    #endregion
}