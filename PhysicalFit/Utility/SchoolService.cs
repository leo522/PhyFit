using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Utility
{
    #region 預填學校資料
    public class SchoolService
    {
        private readonly PhFitnessEntities _db;

        public SchoolService(PhFitnessEntities context)
        {
            _db = context;
        }

        public string GetSchoolNameByCode(string schoolCode)
        {
            return _db.PrimarySchoolList.Where(s => s.SchoolCode.ToString() == schoolCode).Select(s => s.SchoolName).FirstOrDefault()
                ?? _db.JuniorHighSchoolList.Where(s => s.SchoolCode.ToString() == schoolCode).Select(s => s.SchoolName).FirstOrDefault()
                ?? _db.GeneralHighSchoolList.Where(s => s.SchoolCode.ToString() == schoolCode).Select(s => s.SchoolName).FirstOrDefault()
                ?? _db.UniversitySchoolList.Where(s => s.SchoolCode.ToString() == schoolCode).Select(s => s.SchoolName).FirstOrDefault();
        }

        public string GetOrgNameById(int? orgID)
        {
            return _db.Organization.FirstOrDefault(o => o.ID == orgID)?.OrgName;
        }
    }
    #endregion
}