using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit
{
    #region 計算一週的起始日期
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }

    #endregion

    public class TrainingRecordHelper
    {
        #region 計算單一項目的每日訓練量
        public static double CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, string date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                    throw new ArgumentException("日期參數為空");

                if (!DateTime.TryParse(date, out DateTime parsedDate))
                    throw new ArgumentException($"無效的日期格式: {date}");

                if (athleteID == null)
                    throw new ArgumentNullException(nameof(athleteID), "AthleteID 不能為空");

                IQueryable<decimal?> trainingLoadQuery;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        trainingLoadQuery = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad);
                        break;

                    case "射箭訓練衝量":
                        trainingLoadQuery = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad);
                        break;

                    case "射擊訓練衝量":
                        trainingLoadQuery = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) == parsedDate.Date)
                                .Select(record => record.EachTrainingLoad);
                        break;

                    default:
                        throw new ArgumentException("未知的訓練類型");
                }

                var loads = trainingLoadQuery.ToList();
                Console.WriteLine($"查詢結果: {string.Join(", ", loads)}");

                decimal dailySum = trainingLoadQuery.Sum() ?? 0.0m;
                Console.WriteLine($"每日總訓練量: {dailySum}");

                return (double)Math.Round(dailySum, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每日訓練量時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算所有項目的每日訓練量加總
        public static double CalculateDailyTrainingLoadSumForAllTypes(
            PhFitnessEntities dbContext, string date, bool isAthlete, int? athleteID)
        {
            try
            {
                string[] types = {
                    "一般訓練衝量監控 (session-RPE)",
                    "射箭訓練衝量",
                    "射擊訓練衝量"
                };

                return types
                    .Select(type => CalculateDailyTrainingLoadSum(dbContext, date, type, isAthlete, athleteID))
                    .Sum();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每日總訓練量(加總全部項目)時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算單一項目的每週訓練量
        public static double CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime startOfWeek, DateTime endOfWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                DateTime StartDate = startOfWeek.Date;
                DateTime EndDate = endOfWeek.Date.AddDays(1).AddTicks(-1);

                if (startOfWeek == default(DateTime) || endOfWeek == default(DateTime))
                {
                    throw new ArgumentException("傳入的日期範圍無效，請提供正確的日期範圍。");
                }

                IQueryable<double?> query;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        query = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad)
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad);
                        break;

                    case "射箭訓練衝量":
                        query = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad)
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad);
                        break;

                    case "射擊訓練衝量":
                        query = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad)
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= StartDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) <= EndDate)
                                .Select(record => (double?)record.EachTrainingLoad);
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }

                return query.Any() ? query.Sum(load => load ?? 0f) : 0f;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每週訓練量時發生錯誤: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 計算所有項目的每週訓練量
        public static double CalculateWeeklyTrainingLoadSumForAllTypes(
            PhFitnessEntities dbContext, DateTime startOfWeek, DateTime endOfWeek, bool isAthlete, int? athleteID)
        {
            try
            {
                if (startOfWeek == default(DateTime) || endOfWeek == default(DateTime))
                    throw new ArgumentException("傳入的日期範圍無效，請提供正確的日期範圍。");

                string[] types = { "一般訓練衝量監控 (session-RPE)", "射箭訓練衝量", "射擊訓練衝量" };

                double total = types
                    .Select(type => CalculateWeeklyTrainingLoadSum(dbContext, startOfWeek, endOfWeek, type, isAthlete, athleteID))
                    .Sum();

                return Math.Round(total, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每週訓練量(加總三項目)時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算標準差
        public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                List<decimal?> loads;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        loads = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.GeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射箭訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ArcheryRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射擊訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID &&
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }

                if (!loads.Any()) return 0;

                var average = loads.Average();

                var variance = loads.Sum(load => (load - average) * (load - average)) / (loads.Count - 1);

                return Math.Sqrt((double)variance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 計算單一項目的訓練同質性(TM)
        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime startDate, DateTime endDate, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                List<(DateTime TrainingDate, decimal? EachTrainingLoad)> trainingRecords;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        trainingRecords = (isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                        ).ToList()
                        .Select(x => (x.TrainingDate.Value, x.EachTrainingLoad))
                        .ToList();
                        break;

                    case "射箭訓練衝量":
                        trainingRecords = (isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                        ).ToList()
                        .Select(x => (x.TrainingDate.Value, x.EachTrainingLoad))
                        .ToList();
                        break;

                    case "射擊訓練衝量":
                        trainingRecords = (isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => new { record.TrainingDate, record.EachTrainingLoad })
                        ).ToList()
                        .Select(x => (x.TrainingDate.Value, x.EachTrainingLoad))
                        .ToList();
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }

                var dailyLoads = trainingRecords
                    .GroupBy(r => r.TrainingDate.Date)
                    .Select(g => g.Sum(x => x.EachTrainingLoad ?? 0))  // 每天加總
                    .ToList();

                if (!dailyLoads.Any())
                {
                    return 0;
                }

                decimal? mean = dailyLoads.Average();

                decimal? variance = dailyLoads.Sum(x => (x - mean) * (x - mean)) / dailyLoads.Count;
                double standardDeviation = Math.Sqrt((double)variance);

                return standardDeviation == 0 ? 1 : Math.Round((double)(mean / (decimal)standardDeviation), 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算同質性時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算所有項目的訓練同質性 (TM)
        public static double CalculateTrainingMonotonyForAllTypes(PhFitnessEntities dbContext, DateTime startDate, DateTime endDate, bool isAthlete, int? athleteID)
        {
            try
            {
                var dailyLoads = CombinedTrainingHelper
                    .GetAllTrainingLoads(dbContext, startDate.Date, endDate.Date, isAthlete, athleteID)
                    .Select(x => x.load)
                    .ToList();

                if (!dailyLoads.Any())
                    return 0;

                double mean = dailyLoads.Average();
                double variance = dailyLoads.Sum(x => Math.Pow(x - mean, 2)) / dailyLoads.Count;
                double stddev = Math.Sqrt(variance);

                return stddev == 0 ? 1 : Math.Round(mean / stddev, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算總訓練同質性時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算單一項目的訓練張力值(TS)
        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime startOfWeek, DateTime endOfWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                double weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, startOfWeek, endOfWeek, trainingType, isAthlete, athleteID);

                double trainingMonotony = CalculateTrainingMonotony(dbContext, startOfWeek, endOfWeek, trainingType, isAthlete, athleteID);

                return Math.Round(weeklyTrainingLoadSum * trainingMonotony, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算訓練張力值時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算所有項目的訓練張力值(TS)
        public static double CalculateTrainingStrainForAllTypes(PhFitnessEntities db, DateTime start, DateTime end, bool isAthlete, int? athleteID)
        {
            try
            {
                var weeklySum = CalculateWeeklyTrainingLoadSumForAllTypes(db, start, end, isAthlete, athleteID);
                var monotony = CalculateTrainingMonotonyForAllTypes(db, start, end, isAthlete, athleteID);
                return Math.Round(weeklySum * monotony, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算訓練張力值時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算單一項目的週間訓練變化
        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime startOfCurrentWeek, DateTime endOfCurrentWeek, DateTime startOfPreviousWeek, DateTime endOfPreviousWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                double currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, startOfCurrentWeek, endOfCurrentWeek, trainingType, isAthlete, athleteID);
                double previousWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, startOfPreviousWeek, endOfPreviousWeek, trainingType, isAthlete, athleteID);

                return Math.Abs(currentWeekLoad - previousWeekLoad);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算週間訓練變化時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算所有項目的週間訓練變化
        public static double CalculateWeekToWeekChangeForAllTypes(PhFitnessEntities db, DateTime startCurrent, DateTime endCurrent, DateTime startPrev, DateTime endPrev, bool isAthlete, int? athleteID)
        {
            try
            {
                var currentWeek = CalculateWeeklyTrainingLoadSumForAllTypes(db, startCurrent, endCurrent, isAthlete, athleteID);
                var previousWeek = CalculateWeeklyTrainingLoadSumForAllTypes(db, startPrev, endPrev, isAthlete, athleteID);
                return Math.Abs(currentWeek - previousWeek);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算週間變化時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算單一項目的短長期訓練量比值_ACWR
        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime queryDate, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                var ewmaData = new List<(double load, double weight)>();

                for (int i = 1; i <= 28; i++) // i=1 為當天，i=28 為前27天
                {
                    var targetDate = queryDate.AddDays(-(i - 1)); // 第 i 天為查詢日往前 i-1 天
                    string dateString = targetDate.ToString("yyyy-MM-dd");

                    double load = TrainingRecordHelper.CalculateDailyTrainingLoadSum(
                        dbContext, dateString, trainingType, isAthlete, athleteID);

                    double ratio = 2.0 / (i + 1);
                    double weight = 1 - Math.Pow(1 - ratio, i + 1);

                    ewmaData.Add((load, weight));
                }

                // acute = 前 7 天（i = 1~7）
                //var acuteSegment = ewmaData.Take(7);
                var acuteSegment = ewmaData.Take(7).Where(d => d.load > 0);
                double acuteNumerator = acuteSegment.Sum(d => d.load * d.weight);
                double acuteDenominator = acuteSegment.Sum(d => d.weight);
                double acute = acuteDenominator == 0 ? 0 : acuteNumerator / acuteDenominator;

                // chronic = 前 28 天（i = 1~28）
                //var chronicSegment = ewmaData;
                var chronicSegment = ewmaData.Where(d => d.load > 0);
                double chronicNumerator = chronicSegment.Sum(d => d.load * d.weight);
                double chronicDenominator = chronicSegment.Sum(d => d.weight);
                double chronic = chronicDenominator == 0 ? 0 : chronicNumerator / chronicDenominator;

                if (chronic == 0)
                {
                    Console.WriteLine("Chronic 為零，無法計算 ACWR");
                    return 0;
                }

                return Math.Round(acute / chronic, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算 ACWR 時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算所有項目的短長期訓練量比值_ACWR
        public static double CalculateACWRForAllTypes(PhFitnessEntities db, DateTime queryDate, bool isAthlete, int? athleteID)
        {
            try
            {
                var ewmaData = new List<(double load, double weight)>();

                for (int i = 1; i <= 28; i++)
                {
                    var targetDate = queryDate.AddDays(-(i - 1));
                    string dateStr = targetDate.ToString("yyyy-MM-dd");

                    double load = CalculateDailyTrainingLoadSumForAllTypes(db, dateStr, isAthlete, athleteID);

                    double ratio = 2.0 / (i + 1);
                    double weight = 1 - Math.Pow(1 - ratio, i + 1);

                    ewmaData.Add((load, weight));
                }

                var acuteSegment = ewmaData.Take(7).Where(d => d.load > 0);
                var chronicSegment = ewmaData.Where(d => d.load > 0);

                double acute = acuteSegment.Sum(d => d.load * d.weight) / Math.Max(acuteSegment.Sum(d => d.weight), 1);
                double chronic = chronicSegment.Sum(d => d.load * d.weight) / Math.Max(chronicSegment.Sum(d => d.weight), 1);

                return chronic == 0 ? 0 : Math.Round(acute / chronic, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算 ACWR 時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion
    }

    #region 一週內每日對應的每日訓練量
    public static class CombinedTrainingHelper
    {
        public static List<(DateTime date, double load)> GetAllTrainingLoads(PhFitnessEntities dbContext, DateTime startDate, DateTime endDate, bool isAthlete, int? athleteID)
        {
            var result = new List<(DateTime, double)>();

            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                string dateString = date.ToString("yyyy-MM-dd");

                double dailySum = TrainingRecordHelper.CalculateDailyTrainingLoadSumForAllTypes(dbContext, dateString, isAthlete, athleteID);

                if (dailySum > 0)  // 只加入有訓練的那一天
                {
                    result.Add((date, dailySum));
                }
            }

            return result;
        }
    }
    #endregion

}