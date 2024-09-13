using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace PhysicalFit.Controllers
{
    public class RecordController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        public ActionResult SessionRecord()
        {
            //判斷是否是教練
            bool isCoach = User.IsInRole("Coach");

            var userId = User.Identity.GetUserId();

            // 查詢運動員的訓練紀錄，假設 SessionRecords 是紀錄表
            var query = _db.DetectionTrainingRecord.AsQueryable();

            if (isCoach)
            {
                // 假設教練可以選擇運動員，並從 ViewBag 選擇的運動員過濾紀錄
                var selectedAthleteId = ViewBag.SelectedAthleteId as string;
                if (int.TryParse(selectedAthleteId, out int athleteId)) // 確保選擇的運動員 ID 可以轉換為 int
                {
                    query = query.Where(x => x.AthleteID == athleteId);
                }
            }
            else
            {
                if (int.TryParse(userId, out int athleteId)) // 確保 userId 可以轉換為 int
                {
                    query = query.Where(x => x.AthleteID == athleteId);
                }
            }

            // 根據訓練日期排序
            var records = query.OrderBy(x => x.TrainingDate).ToList();

            return View(records);
        }
    }
}