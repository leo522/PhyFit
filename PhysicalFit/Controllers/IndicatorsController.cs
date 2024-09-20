using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhysicalFit.Controllers
{
    public class IndicatorsController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        #region 指標
        public ActionResult Indicator()
        {
            return View();
        }
        #endregion

        #region 睡眠品質

        #endregion

        #region 疲勞程度

        #endregion

        #region 食慾

        #endregion

        #region 培訓意願

        #endregion

        #region 參賽意願

        #endregion

    }
}