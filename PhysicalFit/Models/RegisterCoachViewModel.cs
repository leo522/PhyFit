using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class RegisterCoachViewModel
    {
        public string SchoolID { get; set; }
        public string CoachSchool { get; set; }
        public string CoachName { get; set; }
        public string CoachEmail { get; set; }
        public string CoachAccount { get; set; }
        public string Coachpwd { get; set; }
        public string CoachPhone { get; set; }
        public string CoachTeam { get; set; }
        public string CoachSpecialty { get; set; }
        public string Organize { get; set; }
    }
}