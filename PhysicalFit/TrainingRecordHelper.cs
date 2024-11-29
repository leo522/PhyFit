using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit
{
    public class TrainingRecordHelper
    {
        #region 訓練類型枚舉
        public enum TrainingType
        {
            GeneralSessionRPE,
            ArcheryLoad,
            ShootingLoad
        }
        #endregion

        #region 通用方法
        private static IQueryable<object> GetTrainingRecords(
        PhFitnessEntities dbContext,
        DateTime startDate,
        DateTime endDate,
        bool isAthlete,
        string trainingType)
        {
            if (isAthlete)
            {
                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        return dbContext.AthleteGeneralTrainingRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    case "射箭訓練衝量":
                        return dbContext.AthleteArcheryTrainingRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    case "射擊訓練衝量":
                        return dbContext.AthleteShootingRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    default:
                        throw new ArgumentException("無效的訓練類型");
                }
            }
            else
            {
                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        return dbContext.GeneralTrainingRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    case "射箭訓練衝量":
                        return dbContext.ArcheryRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    case "射擊訓練衝量":
                        return dbContext.ShootingRecord
                            .Where(record =>
                                DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                            .Cast<object>();
                    default:
                        throw new ArgumentException("無效的訓練類型");
                }
            }
        }

        #endregion

        #region 計算訓練量
        public static int CalculateTrainingLoad(string trainingTime, int rpeScore, string trainingType)
        {
            if (rpeScore <= 0)
            {
                Console.WriteLine("RPE 分數無效，應大於 0");
                return 0;
            }

            switch (trainingType)
            {
                case "一般訓練衝量監控 (session-RPE)":
                    return CalculateSessionRPE(trainingTime, rpeScore);
                case "射箭訓練衝量":
                    return CalculateArcheryLoad(trainingTime, rpeScore);
                case "射擊訓練衝量":
                    return CalculateShootingLoad(trainingTime, rpeScore);
                default:
                    throw new ArgumentException("無效的訓練類型");
            }
        }

        private static int CalculateSessionRPE(string trainingTime, int rpeScore)
        {
            var match = Regex.Match(trainingTime, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int trTime))
            {
                return trTime * rpeScore;
            }
            Console.WriteLine("訓練時間格式無效");
            return 0;
        }

        private static int CalculateArcheryLoad(string trainingTime, int rpeScore)
        {
            return int.TryParse(trainingTime, out int arrowCount) ? arrowCount * rpeScore : 0;
        }

        private static int CalculateShootingLoad(string trainingTime, int rpeScore)
        {
            return int.TryParse(trainingTime, out int bulletCount) ? bulletCount * rpeScore : 0;
        }

        //public static int CalculateTrainingLoad(string trainingTime, int rpeScore, string trainingType)
        //{
        //    try
        //    {
        //        // 檢查 RPE 分數是否有效
        //        if (rpeScore <= 0)
        //        {
        //            Console.WriteLine("RPE 分數無效，應大於 0");
        //            return 0;
        //        }

        //        if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //        {
        //            var match = Regex.Match(trainingTime, @"\d+");
        //            if (match.Success)
        //            {
        //                var trTime = Convert.ToInt32(match.Value);
        //                return trTime * rpeScore;
        //            }
        //            else
        //            {
        //                throw new ArgumentException("訓練時間格式無效");
        //            }
        //        }
        //        else if (trainingType == "射箭訓練衝量")
        //        {
        //            int arrowCount = Convert.ToInt32(trainingTime); // 假設 trainingTime 存儲的是箭數
        //            return arrowCount * rpeScore; // 射箭的負荷量可能根據箭數和自覺費力程度來計算
        //        }
        //        else if (trainingType == "射擊訓練衝量")
        //        {
        //            int bulletCount = Convert.ToInt32(trainingTime); // 假設 trainingTime 存儲的是子彈數
        //            return bulletCount * rpeScore; // 射擊的負荷量可能根據子彈數和自覺費力程度來計算
        //        }
        //        else
        //        {
        //            throw new ArgumentException("訓練量計算錯誤");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"計算訓練量時發生錯誤: {ex.Message}");
        //        return 0;
        //    }
        //}

        #endregion

        #region 計算每日訓練量
        public static int CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            var records = GetTrainingRecords(dbContext, date.Date, date.Date.AddDays(1), isAthlete, trainingType)
                .AsEnumerable() // 本地化處理，避免 LINQ-to-Entities 限制
                .Select(record => new
                {
                    TrainingTime = record.GetType().GetProperty("TrainingTime") != null
                        ? record.GetType().GetProperty("TrainingTime").GetValue(record)?.ToString()
                        : null,
                    RpeScore = record.GetType().GetProperty("RPEscore") != null
                        ? record.GetType().GetProperty("RPEscore").GetValue(record) as int?
                        : null,
                    ArrowCount = record.GetType().GetProperty("ArrowCount") != null
                        ? record.GetType().GetProperty("ArrowCount").GetValue(record)?.ToString()
                        : null,
                    BulletCount = record.GetType().GetProperty("BulletCount") != null
                        ? record.GetType().GetProperty("BulletCount").GetValue(record)?.ToString()
                        : null
                }).ToList();

            return records.Any()
                ? records.Sum(record => CalculateTrainingLoad(
                    !string.IsNullOrEmpty(record.TrainingTime) ? record.TrainingTime :
                    (!string.IsNullOrEmpty(record.ArrowCount) ? record.ArrowCount :
                    (!string.IsNullOrEmpty(record.BulletCount) ? record.BulletCount : "0")),
                    record.RpeScore ?? 0,
                    trainingType))
                : 0;
        }
        //public static int CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        //{
        //    try
        //    {
        //        if (isAthlete)
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                // 運動員的 RPE 訓練紀錄的加總
        //                var records = dbContext.AthleteGeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //            ? records.Sum(record => CalculateTrainingLoad(record.TrainingTime, record.RPEscore ?? 0, trainingType))
        //            : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.TrainingTime, record.RPEscore ?? 0, trainingType));
        //            }

        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                // 運動員的射箭訓練紀錄的加總
        //                var records = dbContext.AthleteArcheryTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //           ? records.Sum(record => CalculateTrainingLoad(record.ArrowCount?.ToString() ?? "0", record.RPEscore ?? 0, trainingType))
        //           : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.ArrowCount.ToString(), record.RPEscore ?? 0, trainingType));
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                // 運動員的射擊訓練紀錄的加總
        //                var records = dbContext.AthleteShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //            ? records.Sum(record => CalculateTrainingLoad(record.BulletCount?.ToString() ?? "0", record.RPEscore ?? 0, trainingType))
        //            : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.BulletCount.ToString(), record.RPEscore ?? 0, trainingType));
        //            }
        //            else
        //            {
        //                throw new ArgumentException("運動員的每日訓練量計算錯誤");
        //            }
        //        }
        //        else
        //        {
        //            if ((trainingType == "一般訓練衝量監控 (session-RPE)"))
        //            {
        //                // 教練的 RPE 訓練紀錄的加總
        //                var records = dbContext.GeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //            ? records.Sum(record => CalculateTrainingLoad(record.TrainingTime, record.RPEscore ?? 0, trainingType))
        //            : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.TrainingTime, record.RPEscore ?? 0, trainingType));
        //            }

        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                // 教練的射箭訓練紀錄的加總
        //                var records = dbContext.ArcheryRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //            ? records.Sum(record => CalculateTrainingLoad(record.ArrowCount?.ToString() ?? "0", record.RPEscore ?? 0, trainingType))
        //            : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.ArrowCount.ToString(), record.RPEscore ?? 0, trainingType));
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                // 教練的射擊訓練紀錄的加總
        //                var records = dbContext.ShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
        //                    .ToList();
        //                return records.Any()
        //            ? records.Sum(record => CalculateTrainingLoad(record.BulletCount?.ToString() ?? "0", record.RPEscore ?? 0, trainingType))
        //            : 0;
        //                //return records.Sum(record => CalculateTrainingLoad(record.BulletCount.ToString(), record.RPEscore ?? 0, trainingType));
        //            }
        //            else
        //            {
        //                throw new ArgumentException("教練的每日訓練量計算錯誤");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        #region 計算每週訓練量
        public static int CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return GetTrainingRecords(dbContext, startOfWeek, endOfWeek, isAthlete, trainingType)
                .AsEnumerable()
                .Sum(record => record.GetType().GetProperty("EachTrainingLoad") != null
                    ? (record.GetType().GetProperty("EachTrainingLoad").GetValue(record) as int? ?? 0)
                    : 0);
        }

        //public static int CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        //{
        //    try
        //    {
        //        // 確定當前日期的週一日期
        //        var startOfWeek = date.Date;
        //        while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
        //        {
        //            startOfWeek = startOfWeek.AddDays(-1);
        //        }

        //        // 計算週日的日期
        //        var endOfWeek = startOfWeek.AddDays(7);

        //        if (isAthlete)
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                // 運動員的 RPE 訓練的每週訓練量計算
        //                return dbContext.AthleteGeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                // 運動員的射箭訓練的每週訓練量計算
        //                return dbContext.AthleteArcheryTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                // 運動員的射擊訓練的每週訓練量計算
        //                return dbContext.AthleteShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else
        //            {
        //                throw new ArgumentException("運動員的每週訓練量計算錯誤");
        //            }
        //        }
        //        else
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                // 教練的 RPE 訓練的每週訓練量計算
        //                return dbContext.GeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                // 教練的射箭訓練的每週訓練量計算
        //                return dbContext.ArcheryRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                // 教練的射擊訓練的每週訓練量計算
        //                return dbContext.ShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Sum(record => record.EachTrainingLoad ?? 0);
        //            }
        //            else
        //            {
        //                throw new ArgumentException("教練的每週訓練量計算錯誤");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        #region 計算標準差
        public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var loads = GetTrainingRecords(dbContext, startOfWeek, endOfWeek, isAthlete, trainingType)
                .AsEnumerable()
                .Select(record => record.GetType().GetProperty("EachTrainingLoad") != null
                    ? (record.GetType().GetProperty("EachTrainingLoad").GetValue(record) as int? ?? 0)
                    : 0)
                .ToList();

            if (!loads.Any()) return 0;

            var average = loads.Average();
            var variance = loads.Sum(load => Math.Pow(load - average, 2)) / (loads.Count - 1);

            return Math.Sqrt(variance);
        }
        //public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        //{
        //    try
        //    {
        //        // 確定當前日期的週一日期
        //        var startOfWeek = date.Date;
        //        while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
        //        {
        //            startOfWeek = startOfWeek.AddDays(-1);
        //        }

        //        // 計算週日的日期
        //        var endOfWeek = startOfWeek.AddDays(7);

        //        // 根據用戶身份和訓練類型選擇相應的資料表進行查詢
        //        List<int> trainingLoads = new List<int>();

        //        if (isAthlete)
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                trainingLoads = dbContext.AthleteGeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                trainingLoads = dbContext.AthleteArcheryTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                trainingLoads = dbContext.AthleteShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else
        //            {
        //                throw new ArgumentException("運動員的標準差計算錯誤");
        //            }
        //        }
        //        else
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                trainingLoads = dbContext.GeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                trainingLoads = dbContext.ArcheryRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                trainingLoads = dbContext.ShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
        //                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
        //                    .Select(record => record.EachTrainingLoad ?? 0)
        //                    .ToList();
        //            }
        //            else
        //            {
        //                throw new ArgumentException("教練的標準差計算錯誤");
        //            }
        //        }

        //        // 確保有足夠的數據進行標準差計算
        //        if (trainingLoads.Count < 1)
        //        {
        //            throw new ArgumentException("當前週數的數據量不足");
        //        }

        //        // 計算一週的天數，這裡固定為7天
        //        int numberOfDays = 7;
        //        // 計算總訓練量
        //        double totalDailyLoad = trainingLoads.Sum();
        //        // 計算平均值
        //        double mean = totalDailyLoad / numberOfDays;

        //        // 計算每一天的平方差和
        //        double sumOfSquares = trainingLoads
        //            .Select(dailyTotal => Math.Pow(dailyTotal - mean, 2))
        //            .Sum();

        //        // 計算樣本方差
        //        double sampleVariance = sumOfSquares / (numberOfDays - 1);

        //        // 計算樣本標準差
        //        double standardDeviation = Math.Sqrt(sampleVariance);

        //        // 四捨五入取整數
        //        int roundedStandardDeviation = (int)Math.Round(standardDeviation, MidpointRounding.AwayFromZero);

        //        return roundedStandardDeviation;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        #region 計算訓練同質性(TM)

        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            try
            {
                var trainingRecords = new List<int>();

                // 根據用戶身份和訓練類型獲取不同表中的數據
                if (isAthlete)
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        trainingRecords = dbContext.AthleteGeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        trainingRecords = dbContext.AthleteArcheryTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        trainingRecords = dbContext.AthleteShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else
                    {
                        throw new ArgumentException("運動員的同質性計算錯誤");
                    }
                }
                else
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        trainingRecords = dbContext.GeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        trainingRecords = dbContext.ArcheryRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        trainingRecords = dbContext.ShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                            .Select(record => record.EachTrainingLoad ?? 0)
                            .ToList();
                    }
                    else
                    {
                        throw new ArgumentException("教練的同質性計算錯誤");
                    }
                }

                // 確保有足夠的數據進行同質性計算
                if (trainingRecords.Count == 0)
                {
                    throw new ArgumentException("尚無有足夠的數據計算同質性結果");
                }

                // 計算平均值
                double mean = trainingRecords.Average();
                // 計算標準差
                double standardDeviation = Math.Sqrt(trainingRecords.Select(x => Math.Pow(x - mean, 2)).Average());

                // 如果標準差為零，返回一個預設值（同質性為1）
                if (standardDeviation == 0)
                {
                    return 1;
                }

                // 返回同質性
                return Math.Round(mean / standardDeviation, 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算訓練張力值(TS)

        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            try
            {
                // 根據不同的訓練類型與角色獲取每週總訓練量 (TL)
                int weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete);

                // 根據不同的訓練類型與角色獲取訓練同質性 (TM)
                double trainingMonotony = CalculateTrainingMonotony(dbContext, date, trainingType, isAthlete);

                // 計算張力值 (TS)
                double trainingStrain = weeklyTrainingLoadSum * trainingMonotony;

                // 四捨五入到小數點後兩位
                return Math.Round(trainingStrain, 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算週間訓練變化

        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            try
            {
                // 獲取當週總訓練量 (TL)
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete);

                // 計算上週的日期範圍
                var startOfPreviousWeek = date.Date;
                while (startOfPreviousWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfPreviousWeek = startOfPreviousWeek.AddDays(-1);
                }
                startOfPreviousWeek = startOfPreviousWeek.AddDays(-7); // 回到上一週的週一
                var endOfPreviousWeek = startOfPreviousWeek.AddDays(7);

                // 獲取上週總訓練量 (根據不同的訓練類型和身份選擇不同的資料表)
                int previousWeekLoad = 0;

                if (isAthlete)
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        previousWeekLoad = dbContext.AthleteGeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0); //處理 nullable 值
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        previousWeekLoad = dbContext.AthleteArcheryTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        previousWeekLoad = dbContext.AthleteShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0);
                    }
                    else
                    {
                        throw new ArgumentException("運動員的週間訓練變化計算錯誤");
                    }
                }
                else
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        previousWeekLoad = dbContext.GeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0); //處理 nullable 值
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        previousWeekLoad = dbContext.ArcheryRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        previousWeekLoad = dbContext.ShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                            .ToList().Sum(record => record.EachTrainingLoad ?? 0);
                    }
                    else
                    {
                        throw new ArgumentException("教練的週間訓練變化計算錯誤");
                    }
                }

                // 如果沒有上週的訓練記錄，則返回 0
                if (previousWeekLoad == 0)
                {
                    return 0; // 或者也可以返回 currentWeekLoad
                }

                // 計算當週和上週總訓練量的絕對差異性
                double variability = Math.Abs(currentWeekLoad - previousWeekLoad);

                return variability;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region 計算短長期訓練量比值_ACWR

        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete)
        {
            try
            {
                // 計算當週運動訓練量
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete);

                // 初始化變數來儲存過往四週的總訓練量
                double pastFourWeeksLoad = 0;

                // 從當週往前推四週，計算每週的運動訓練量
                for (int i = 0; i < 4; i++)
                {
                    // 計算當前處理的週的開始和結束日期
                    var startOfWeek = date.Date.AddDays(-7 * i);
                    while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                    {
                        startOfWeek = startOfWeek.AddDays(-1);
                    }
                    var endOfWeek = startOfWeek.AddDays(7);

                    // 根據用戶身份和訓練類型選擇不同的資料表
                    if (isAthlete)
                    {
                        if (trainingType == "一般訓練衝量監控 (session-RPE)")
                        {
                            pastFourWeeksLoad += dbContext.AthleteGeneralTrainingRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);  // 處理 nullable 值
                        }
                        else if (trainingType == "射箭訓練衝量")
                        {
                            pastFourWeeksLoad += dbContext.AthleteArcheryTrainingRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);
                        }
                        else if (trainingType == "射擊訓練衝量")
                        {
                            pastFourWeeksLoad += dbContext.AthleteShootingRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);
                        }
                        else
                        {
                            throw new ArgumentException("運動員的ACWR計算錯誤");
                        }
                    }
                    else
                    {
                        if (trainingType == "一般訓練衝量監控 (session-RPE)")
                        {
                            pastFourWeeksLoad += dbContext.GeneralTrainingRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);  // 處理 nullable 值
                        }
                        else if (trainingType == "射箭訓練衝量")
                        {
                            pastFourWeeksLoad += dbContext.ArcheryRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);
                        }
                        else if (trainingType == "射擊訓練衝量")
                        {
                            pastFourWeeksLoad += dbContext.ShootingRecord
                                .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                              && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .ToList()  // 將資料取到內存中
                                .Sum(record => record.EachTrainingLoad ?? 0);
                        }
                        else
                        {
                            throw new ArgumentException("教練的ACWR計算錯誤");
                        }
                    }
                }

                // 確保至少有一週的數據
                if (pastFourWeeksLoad == 0)
                {
                    throw new InvalidOperationException("過往四週的運動訓練量為零，無法計算短長期訓練量比值。");
                }

                // 計算過往四週的平均值
                double pastFourWeeksAverage = pastFourWeeksLoad / 4;

                // 避免分母為零的情況
                if (pastFourWeeksAverage == 0)
                {
                    throw new InvalidOperationException("分母為零，過往四週的運動訓練量為零，無法計算短長期訓練量比值。");
                }

                // 計算短長期訓練量比值 (ACWR)
                double acwr = currentWeekLoad / pastFourWeeksAverage;

                return acwr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}