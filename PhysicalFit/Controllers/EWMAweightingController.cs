using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhysicalFit.Controllers
{
    public class EWMAweightingController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        #region 指標
        public ActionResult Indicators()
        {
            return View(new EWMAViewModel());
        }

        [HttpPost]
        public ActionResult Calculate(EWMAViewModel model)
        {
            // 計算 EWMA 權重
            model.Calculate();

            return View("Indicators", model); // 返回同一視圖並顯示結果
        }
        #endregion

        #region 睡眠品質
        public ActionResult SleepQuality()
        {
            return View();
        }
        #endregion

        #region 疲勞程度
        public ActionResult FaitgueLevel()
        {
            return View();
        }
        #endregion

        #region 食慾
        public ActionResult Appetite()
        {
            return View();
        }
        #endregion

        #region 培訓意願
        public ActionResult WillingnessofTraining()
        {
            return View();
        }
        #endregion

        #region 參賽意願
        public ActionResult WillingnessofRace()
        {
            return View();
        }
        #endregion
    }
}