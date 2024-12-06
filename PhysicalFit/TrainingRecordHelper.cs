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
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }
    }

    #endregion

    public class TrainingRecordHelper
    {
        #region 計算每日訓練量

        public static int CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, string date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                    throw new ArgumentException("日期參數為空");

                if (!DateTime.TryParse(date, out DateTime parsedDate))
                    throw new ArgumentException($"無效的日期格式: {date}");

                DateTime startOfDay = parsedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                IQueryable<int?> trainingLoadQuery;

                if (isAthlete)
                {
                    switch (trainingType)
                    {
                        case "一般訓練衝量監控 (session-RPE)":
                            trainingLoadQuery = dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        case "射箭訓練衝量":
                            trainingLoadQuery = dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        case "射擊訓練衝量":
                            trainingLoadQuery = dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        default:
                            throw new ArgumentException("未知的訓練類型");
                    }
                }
                else
                {
                    switch (trainingType)
                    {
                        case "一般訓練衝量監控 (session-RPE)":
                            trainingLoadQuery = dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        case "射箭訓練衝量":
                            trainingLoadQuery = dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        case "射擊訓練衝量":
                            trainingLoadQuery = dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID && record.TrainingDate >= startOfDay && record.TrainingDate < endOfDay)
                                .Select(record => (int?)record.EachTrainingLoad);
                            break;
                        default:
                            throw new ArgumentException("未知的訓練類型");
                    }
                }

                return trainingLoadQuery?.Sum() ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每日訓練量時發生錯誤: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 計算每週訓練量
        public static int CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                DateTime startOfWeek = date.AddDays(-(int)date.DayOfWeek);
                DateTime endOfWeek = startOfWeek.AddDays(7);

                IQueryable<int?> query;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        query = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad);
                        break;
                    case "射箭訓練衝量":
                        query = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad);
                        break;
                    case "射擊訓練衝量":
                        query = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad)
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID && DbFunctions.TruncateTime(record.TrainingDate) >= startOfWeek && DbFunctions.TruncateTime(record.TrainingDate) < endOfWeek)
                                .Select(record => record.EachTrainingLoad);
                        break;
                    default:
                        throw new ArgumentException("無效的訓練類型");
                }

                return query.Any() ? query.Sum(load => load ?? 0) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每週訓練量時發生錯誤: {ex.Message}");
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

                List<int> loads;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        loads = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.GeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    case "射箭訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.ArcheryRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    case "射擊訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.ShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }


                if (!loads.Any()) return 0;

                var average = loads.Average();
                var variance = loads.Sum(load => Math.Pow(load - average, 2)) / (loads.Count - 1);

                return Math.Sqrt(variance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion

        #region 計算訓練同質性(TM)
        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 計算當天的開始與結束時間
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                // 根據訓練類型和角色選擇正確的記錄集合
                List<int> trainingRecords;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    case "射箭訓練衝量":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    case "射擊訓練衝量":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList()
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startDate) &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endDate))
                                .Select(record => record.EachTrainingLoad ?? 0)
                                .ToList();
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }

                // 如果沒有數據，返回 0
                if (!trainingRecords.Any())
                {
                    return 0;
                }

                // 計算平均值
                double mean = trainingRecords.Average();

                // 計算標準差
                double standardDeviation = Math.Sqrt(trainingRecords.Sum(x => Math.Pow(x - mean, 2)) / trainingRecords.Count);

                // 避免標準差為零的情況
                return standardDeviation == 0 ? 1 : Math.Round(mean / standardDeviation, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算同質性時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算訓練張力值(TS)
        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 每週總訓練量
                int weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete, athleteID);

                // 訓練同質性
                double trainingMonotony = CalculateTrainingMonotony(dbContext, date, trainingType, isAthlete, athleteID);

                // 計算張力值
                return Math.Round(weeklyTrainingLoadSum * trainingMonotony, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算訓練張力值時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算週間訓練變化
        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 當週與上週的日期範圍
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete, athleteID);

                var startOfPreviousWeek = date.AddDays(-7 * 2).StartOfWeek();
                var endOfPreviousWeek = startOfPreviousWeek.AddDays(7);
                int previousWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, startOfPreviousWeek, trainingType, isAthlete, athleteID);

                // 返回絕對差異值
                return Math.Abs(currentWeekLoad - previousWeekLoad);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算週間訓練變化時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算短長期訓練量比值_ACWR
        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime date, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 當週總訓練量
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType, isAthlete, athleteID);

                // 計算過往四週總訓練量
                double pastFourWeeksLoad = 0;

                for (int i = 0; i < 4; i++)
                {
                    // 計算每週起始日期
                    var weekDate = date.AddDays(-7 * i);

                    // 累加每週訓練量
                    pastFourWeeksLoad += CalculateWeeklyTrainingLoadSum(dbContext, weekDate, trainingType, isAthlete, athleteID);
                }

                // 避免分母為零
                double pastFourWeeksAverage = pastFourWeeksLoad / 4;
                if (pastFourWeeksAverage == 0)
                {
                    Console.WriteLine("過往四週的訓練量為零，無法計算 ACWR");
                    return 0;
                }

                // 計算短長期訓練量比值
                return Math.Round(currentWeekLoad / pastFourWeeksAverage, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算 ACWR 時發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}