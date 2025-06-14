using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhysicalFit.ViewModels
{
    #region 註冊教練帳號
    public class RegisterCoachViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [Display(Name = "姓名")]
        public string CoachName { get; set; }

        [Required(ErrorMessage = "請輸入帳號")]
        [Display(Name = "帳號")]
        public string CoachAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Coachpwd { get; set; }

        [Required(ErrorMessage = "請輸入信箱")]
        [EmailAddress(ErrorMessage = "請輸入正確的信箱格式")]
        [Display(Name = "電子郵件")]
        public string CoachEmail { get; set; }

        [Display(Name = "聯絡電話")]
        public string CoachPhone { get; set; }

        [Display(Name = "所屬學校")]
        public string CoachSchool { get; set; }

        [Display(Name = "所屬單位 / 機關")]
        public string Organize { get; set; }

        [Display(Name = "隊伍名稱")]
        public string CoachTeam { get; set; }

        [Display(Name = "專項領域")]
        public string CoachSpecialty { get; set; }

        [Display(Name = "學校代碼 (如有)")]
        public string SchoolID { get; set; }
    }
    #endregion
}