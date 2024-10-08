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
        #region 計算訓練量

        public static int CalculateTrainingLoad(string trainingTime, int rpeScore, string trainingType)
        {
            try
            {
                if (trainingType == "RPE訓練紀錄")
                {
                    // RPE 訓練的計算方式
                    var match = Regex.Match(trainingTime, @"\d+");
                    if (match.Success)
                    {
                        var trTime = Convert.ToInt32(match.Value);
                        return trTime * rpeScore;
                    }
                    else
                    {
                        throw new ArgumentException("TrainingTime 格式無效");
                    }
                }
                else if (trainingType == "射箭訓練衝量")
                {
                    // 射箭訓練的計算方式，例如根據箭數和磅數計算
                    // 示例邏輯：箭數 * 磅數
                    int arrowCount = Convert.ToInt32(trainingTime); // 假設 trainingTime 存儲的是箭數
                    return arrowCount * rpeScore; // 射箭的負荷量可能根據箭數和自覺費力程度來計算
                }
                else if (trainingType == "射擊訓練衝量")
                {
                    // 射擊訓練的計算方式，例如根據子彈數來計算
                    int bulletCount = Convert.ToInt32(trainingTime); // 假設 trainingTime 存儲的是子彈數
                    return bulletCount * rpeScore; // 射擊的負荷量可能根據子彈數和自覺費力程度來計算
                }
                else
                {
                    throw new ArgumentException("未知的訓練類型");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算每日訓練量

        public static int CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            try
            {
                if (trainingType == "RPE訓練紀錄")
                {
                    // RPE 訓練紀錄的加總
                    var records = dbContext.SessionRPETrainingRecords
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                        .ToList(); // 拉取到內存中

                    return records.Sum(record => CalculateTrainingLoad(record.TrainingTime, record.RPEscore ?? 0, "RPE訓練紀錄"));
                }
                else if (trainingType == "射箭訓練衝量")
                {
                    // 射箭訓練的加總
                    var records = dbContext.ArcheryRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                        .ToList(); // 拉取到內存中

                    return records.Sum(record => CalculateTrainingLoad(record.ArrowCount.ToString(), record.RPEscore ?? 0, "射箭訓練衝量"));
                }
                else if (trainingType == "射擊訓練衝量")
                {
                    // 射擊訓練的加總
                    var records = dbContext.ShootingRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                        .ToList(); // 拉取到內存中

                    return records.Sum(record => CalculateTrainingLoad(record.BulletCount.ToString(), record.RPEscore ?? 0, "射擊訓練衝量"));
                }
                else
                {
                    throw new ArgumentException("未知的訓練類型");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算每週訓練量

        public static int CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            try
            {
                // 確定當前日期的週一日期
                var startOfWeek = date.Date;
                while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfWeek = startOfWeek.AddDays(-1);
                }

                // 計算週日的日期
                var endOfWeek = startOfWeek.AddDays(7);

                if (trainingType == "RPE訓練紀錄")
                {
                    // RPE 訓練的每週訓練量計算
                    return dbContext.SessionRPETrainingRecords
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                        .Sum(record => record.TrainingLoad ?? 0);
                }
                else if (trainingType == "射箭訓練衝量")
                {
                    // 射箭訓練的每週訓練量計算
                    return dbContext.ArcheryRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                        .Sum(record => record.EachTrainingLoad ?? 0);
                }
                else if (trainingType == "射擊訓練衝量")
                {
                    // 射擊訓練的每週訓練量計算
                    return dbContext.ShootingRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                        .Sum(record => record.EachTrainingLoad ?? 0);
                }
                else
                {
                    throw new ArgumentException("未知的訓練類型");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算標準差

        public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date)
        {
            try
            {
                // 確定當前日期的週一日期
                var startOfWeek = date.Date;
                while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfWeek = startOfWeek.AddDays(-1);
                }
                // 計算週日的日期
                var endOfWeek = startOfWeek.AddDays(7);

                // 獲取該週內的所有訓練記錄，並使用 DbFunctions.TruncateTime 進行日期比較
                var trainingRecords = dbContext.SessionRPETrainingRecords
                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                  && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                    .ToList();

                // 計算每天的總訓練量，依據 TruncateTime 過的日期進行分組
                var daTotals = trainingRecords
                    .GroupBy(record => DbFunctions.TruncateTime(record.TrainingDate))  // 按日期分組
                    .ToDictionary(
                        group => group.Key,  // 日期作為鍵
                        group => group.Sum(record => record.TrainingLoad.GetValueOrDefault())  // 計算每天的總和
                    );

                // 確保有足夠的數據進行標準差計算
                if (daTotals.Count < 1)
                {
                    throw new ArgumentException("當前週的數據點數不足");
                }

                // 計算一週的天數，這裡固定為7天
                int numberOfDays = 7;
                // 計算總訓練量
                double totalDailyLoad = daTotals.Values.Sum();
                // 計算平均值
                double mean = totalDailyLoad / numberOfDays;

                // 計算每一天的平方差和
                double sumOfSquares = daTotals.Values
                    .Select(dailyTotal => Math.Pow(dailyTotal - mean, 2))
                    .Sum();

                // 計算樣本方差
                double sampleVariance = sumOfSquares / (numberOfDays - 1);

                // 計算樣本標準差
                double standardDeviation = Math.Sqrt(sampleVariance);

                // 四捨五入取整數
                int roundedStandardDeviation = (int)Math.Round(standardDeviation, MidpointRounding.AwayFromZero);

                return roundedStandardDeviation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 計算訓練同質性(TM)

        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            var trainingRecords = new List<int>();

            // 根據訓練類型獲取不同表中的數據
            if (trainingType == "RPE訓練紀錄")
            {
                trainingRecords = dbContext.SessionRPETrainingRecords
                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                    .Select(record => record.TrainingLoad ?? 0)
                    .ToList();
            }
            else if (trainingType == "射箭訓練衝量")
            {
                trainingRecords = dbContext.ArcheryRecord
                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == DbFunctions.TruncateTime(date))
                    .Select(record => record.EachTrainingLoad ?? 0)
                    .ToList();
            }

            if (trainingRecords.Count == 0)
            {
                throw new ArgumentException("沒有找到對應的訓練記錄");
            }

            double mean = trainingRecords.Average();
            double standardDeviation = Math.Sqrt(trainingRecords.Select(x => Math.Pow(x - mean, 2)).Average());

            // 如果標準差為零，返回一個預設值
            if (standardDeviation == 0)
            {
                return 1; // 可根據需要設定一個合理的預設值
            }

            return Math.Round(mean / standardDeviation, 2);
        }

        #endregion

        #region 計算訓練張力值(TS)

        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            try
            {
                // 根據不同的訓練類型獲取每週總訓練量 (TL)
                int weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType);

                // 根據不同的訓練類型獲取訓練同質性 (TM)
                double trainingMonotony = CalculateTrainingMonotony(dbContext, date, trainingType);

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

        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            try
            {
                // 獲取當週總訓練量 (TL)
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType);

                // 計算上週的日期範圍
                var startOfPreviousWeek = date.Date;
                while (startOfPreviousWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfPreviousWeek = startOfPreviousWeek.AddDays(-1);
                }
                startOfPreviousWeek = startOfPreviousWeek.AddDays(-7); // 回到上一週的週一
                var endOfPreviousWeek = startOfPreviousWeek.AddDays(7);

                // 獲取上週總訓練量 (根據不同的訓練類型選擇不同的資料表)
                int previousWeekLoad = 0;
                if (trainingType == "RPE訓練紀錄")
                {
                    previousWeekLoad = dbContext.SessionRPETrainingRecords
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                        .ToList() // 將結果取到內存中
                        .Sum(record => record.TrainingLoad ?? 0); // 在內存中處理 nullable 值
                }
                else if (trainingType == "射箭訓練衝量")
                {
                    previousWeekLoad = dbContext.ArcheryRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                        .ToList() // 將結果取到內存中
                        .Sum(record => record.EachTrainingLoad ?? 0); // 在內存中處理 nullable 值
                }
                else if (trainingType == "射擊訓練衝量")
                {
                    previousWeekLoad = dbContext.ShootingRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfPreviousWeek)
                                      && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfPreviousWeek))
                        .ToList() // 將結果取到內存中
                        .Sum(record => record.EachTrainingLoad ?? 0); // 在內存中處理 nullable 值
                }
                else
                {
                    throw new ArgumentException("未知的訓練類型");
                }

                // 如果沒有上週的訓練記錄，則返回 0 或提示
                if (previousWeekLoad == 0)
                {
                    // 沒有上週的數據時，這裡可以選擇返回 0 或其他適當的值
                    // 也可以考慮拋出一個自定義例外來提示沒有足夠的數據
                    return 0; // 或 return currentWeekLoad;
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

        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime date, string trainingType)
        {
            try
            {
                // 計算當週運動訓練量
                int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date, trainingType);

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

                    // 根據不同的訓練類型選擇不同的資料表
                    if (trainingType == "RPE訓練紀錄")
                    {
                        pastFourWeeksLoad += dbContext.SessionRPETrainingRecords
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                            .ToList()  // 將資料取到內存中
                            .Sum(record => record.TrainingLoad ?? 0);  // 處理 nullable 值
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        pastFourWeeksLoad += dbContext.ArcheryRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                            .ToList()  // 將資料取到內存中
                            .Sum(record => record.EachTrainingLoad ?? 0);  // 處理 nullable 值
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        pastFourWeeksLoad += dbContext.ShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) >= DbFunctions.TruncateTime(startOfWeek)
                                          && DbFunctions.TruncateTime(record.TrainingDate) < DbFunctions.TruncateTime(endOfWeek))
                            .ToList()  // 將資料取到內存中
                            .Sum(record => record.EachTrainingLoad ?? 0);  // 處理 nullable 值
                    }
                    else
                    {
                        throw new ArgumentException("未知的訓練類型");
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
                    throw new InvalidOperationException("過往四週的運動訓練量為零，無法計算短長期訓練量比值。");
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