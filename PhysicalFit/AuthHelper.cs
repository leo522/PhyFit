using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace PhysicalFit
{
    public static class AuthHelper
    {
        #region 獲取資料表的教練ID
        public static int? GetCurrentCoachId()
        {
            var identity = HttpContext.Current.User.Identity as FormsIdentity;
            if (identity == null)
            {
                return null;
            }

            var userData = identity.Ticket.UserData;

            int coachId;

            if (int.TryParse(userData, out coachId))
            {
                return coachId;
            }

            return null;
        }
        #endregion

        #region 獲取資料表的運動員ID
        public static int? GetCurrentAthleteId()
        {
            var coachId = GetCurrentCoachId();
            if (coachId == null)
            {
                return null;
            }

            using (var db = new PhFitnessEntities())
            {
                var athlete = db.Athletes.FirstOrDefault(a => a.CoachID == coachId);

                return athlete?.ID;
            }
        }
        #endregion
    }
}