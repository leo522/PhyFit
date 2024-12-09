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
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7; // 計算距離週一的天數
            return date.AddDays(-diff).Date;
            // 回到該週的週一
            //while (date.DayOfWeek != DayOfWeek.Monday)
            //{
            //    date = date.AddDays(-1);
            //}
            //return date;
        }
    }

    #endregion

    public class TrainingRecordHelper
    {
        #region 計算每日訓練量

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

                return (double)Math.Round(dailySum, 2); // 保留兩位小數
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算每日訓練量時發生錯誤: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 計算每週訓練量
        public static double CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime startOfWeek, DateTime endOfWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                DateTime StartDate = startOfWeek.Date; // 保留開始日期的 00:00
                DateTime EndDate = endOfWeek.Date.AddDays(1).AddTicks(-1);
                // 驗證傳入範圍
                if (startOfWeek == default(DateTime) || endOfWeek == default(DateTime))
                {
                    throw new ArgumentException("傳入的日期範圍無效，請提供正確的日期範圍。");
                }

                IQueryable<double?> query;

                // 根據 trainingType 查詢相應資料
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

                // 返回計算結果
                return query.Any() ? query.Sum(load => load ?? 0f) : 0f;
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

                List<decimal?> loads;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        loads = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.GeneralTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射箭訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ArcheryRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射擊訓練衝量":
                        loads = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ShootingRecord
                                .Where(record =>
                                    record.AthleteID == athleteID && // 加入 AthleteID 篩選條件
                                    DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek) &&
                                    DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    default:
                        throw new ArgumentException("無效的訓練類型");
                }


                if (!loads.Any()) return 0;

                var average = loads.Average(); //計算平均值

                //var variance = loads.Sum(load => (load - average)) / (loads.Count - 1);  //計算方差
                var variance = loads.Sum(load => (load - average) * (load - average)) / (loads.Count - 1);

                return Math.Sqrt((double)variance);
                //return Math.Sqrt(variance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算訓練同質性(TM)
        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime startDate, DateTime endDate, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 根據訓練類型和角色選擇正確的記錄集合
                List<decimal?> trainingRecords;

                switch (trainingType)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteGeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.GeneralTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射箭訓練衝量":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteArcheryTrainingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ArcheryRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
                                .ToList();
                        break;

                    case "射擊訓練衝量":
                        trainingRecords = isAthlete
                            ? dbContext.AthleteShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
                                .ToList()
                            : dbContext.ShootingRecord
                                .Where(record => record.AthleteID == athleteID &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) >= startDate &&
                                                 DbFunctions.TruncateTime(record.TrainingDate) < endDate)
                                .Select(record => record.EachTrainingLoad)
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
                decimal? mean = trainingRecords.Average();

                // 計算標準差
                decimal? variance = trainingRecords.Sum(x => (x - mean) * (x - mean)) / trainingRecords.Count;
                double standardDeviation = Math.Sqrt((double)variance);

                // 避免標準差為零
                return standardDeviation == 0 ? 1 : Math.Round((double)(mean / (decimal)standardDeviation), 2);

                // 計算平均值和標準差
                //double mean = trainingRecords.Average();
                //double standardDeviation = Math.Sqrt(trainingRecords.Sum(x => Math.Pow(x - mean, 2)) / trainingRecords.Count);

                // 避免標準差為零
                //return standardDeviation == 0 ? 1 : Math.Round(mean / standardDeviation, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"計算同質性時發生錯誤: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 計算訓練張力值(TS)
        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime startOfWeek, DateTime endOfWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 每週總訓練量
                double weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, startOfWeek, endOfWeek, trainingType, isAthlete, athleteID);

                // 訓練同質性
                double trainingMonotony = CalculateTrainingMonotony(dbContext, startOfWeek, endOfWeek, trainingType, isAthlete, athleteID);

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
        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime startOfCurrentWeek, DateTime endOfCurrentWeek, DateTime startOfPreviousWeek, DateTime endOfPreviousWeek, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 當週與上週的日期範圍
                double currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, startOfCurrentWeek, endOfCurrentWeek, trainingType, isAthlete, athleteID);
                double previousWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, startOfPreviousWeek, endOfPreviousWeek, trainingType, isAthlete, athleteID);

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
        public static double CalculateACWR(PhFitnessEntities dbContext, List<(DateTime startOfWeek, DateTime endOfWeek)> weekRanges, string trainingType, bool isAthlete, int? athleteID)
        {
            try
            {
                // 當週總訓練量
                double currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, weekRanges[0].startOfWeek, weekRanges[0].endOfWeek, trainingType, isAthlete, athleteID);

                // 計算過往四週總訓練量
                double pastFourWeeksLoad = 0;
                for (int i = 0; i < 4; i++)
                {
                    pastFourWeeksLoad += CalculateWeeklyTrainingLoadSum(dbContext, weekRanges[i].startOfWeek, weekRanges[i].endOfWeek, trainingType, isAthlete, athleteID);
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