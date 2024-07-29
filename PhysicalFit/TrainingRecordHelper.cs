using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit
{
    public class TrainingRecordHelper
    {
        #region 計算訓練量

        public static int CalculateTrainingLoad(string trainingTime, int rpeScore)
        {
            // 使用正則表達式提取字串中的數字部分
            var match = Regex.Match(trainingTime, @"\d+");
            if (match.Success)
            {
                // 將提取出的數字部分轉換為整數
                var trTime = Convert.ToInt32(match.Value);
                return trTime * rpeScore;
            }
            else
            {
                // 如果無法提取數字部分，返回 0 或處理錯誤
                throw new ArgumentException("TrainingTime 格式無效");
            }
        }

        #endregion

        #region 計算每日訓練量

        public static int CalculateDailyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date)
        {
            return dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate == date)
                .Sum(record => record.TrainingLoad ?? 0);
        }

        #endregion

        #region 計算每週訓練量

        public static int CalculateWeeklyTrainingLoadSum(PhFitnessEntities dbContext, DateTime date)
        {
            // 確定當前日期的週一日期
            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            // 計算週日的日期
            var endOfWeek = startOfWeek.AddDays(7);

            // 獲取該週內的所有訓練記錄並計算總訓練量
            return dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .Sum(record => record.TrainingLoad ?? 0);
        }

        #endregion

        #region 計算標準差

        public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date)
        {
            // 確定當前日期的週一日期
            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            // 計算週日的日期
            var endOfWeek = startOfWeek.AddDays(7);

            // 獲取該週內的所有訓練記錄
            var trainingRecords = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .ToList();

            // 計算每天的總訓練量
            var daTotals = trainingRecords
                .GroupBy(record => record.TrainingDate)  // 按日期分組
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

            //計算樣本方差
            double sampleVariance = sumOfSquares / (numberOfDays - 1);

            //計算樣本標準差
            double standardDeviation = Math.Sqrt(sampleVariance);

            //四捨五入取整數
            int roundedStandardDeviation = (int)Math.Round(standardDeviation, MidpointRounding.AwayFromZero);

            return roundedStandardDeviation;
        }

        #endregion

        #region 計算訓練同質性(TM)

        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime date)
        {
            // 獲取樣本標準差
            double standardDeviation = CalculateSampleStandardDeviation(dbContext, date);

            // 確定當前日期的週一日期
            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            // 計算週日的日期
            var endOfWeek = startOfWeek.AddDays(7);

            // 獲取該週內的所有訓練記錄
            var trainingRecords = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .ToList();

            // 計算每天的總訓練量
            var daTotals = trainingRecords
                .GroupBy(record => record.TrainingDate)  // 按日期分組
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

            // 計算訓練同質性
            double trainingHomogeneity = mean / standardDeviation;

            // 四捨五入到小數點後兩位
            return Math.Round(trainingHomogeneity, 2);
        }

        #endregion

        #region 計算訓練張力值(TS)

        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime date)
        {
            // 獲取每週總訓練量 (TL)
            int weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, date);

            // 獲取訓練同質性 (TM)
            double trainingMonotony = CalculateTrainingMonotony(dbContext, date);

            // 計算張力值 (TS)
            double trainingStrain = weeklyTrainingLoadSum * trainingMonotony;

            // 四捨五入到小數點後兩位
            return Math.Round(trainingStrain, 2);
        }

        #endregion

        #region 計算週間訓練變化

        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime date)
        {
            // 獲取每週總訓練量 (TL)
            int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date);

            // 計算上週的日期範圍
            var startOfPreviousWeek = date.Date;
            while (startOfPreviousWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfPreviousWeek = startOfPreviousWeek.AddDays(-1);
            }
            startOfPreviousWeek = startOfPreviousWeek.AddDays(-7); // 回到上一週的週一
            var endOfPreviousWeek = startOfPreviousWeek.AddDays(7);

            // 獲取上週總訓練量
            int previousWeekLoad = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfPreviousWeek && record.TrainingDate < endOfPreviousWeek)
                .Sum(record => record.TrainingLoad ?? 0);

            // 計算當週和上週總訓練量的絕對差異性
            double variability = Math.Abs(currentWeekLoad - previousWeekLoad);

            return variability;
        }

        #endregion

        #region 計算短長期訓練量比值

        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime date)
        {
            // 計算當週運動訓練量
            int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date);

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

                // 計算該週的總訓練量並累加
                pastFourWeeksLoad += dbContext.SessionRPETrainingRecords
                    .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                    .Sum(record => record.TrainingLoad ?? 0);
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
        
        #endregion
    }
}