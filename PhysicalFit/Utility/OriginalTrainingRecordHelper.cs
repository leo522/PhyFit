using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit
{
    public class OriginalTrainingRecordHelper
    {
        #region 計算訓練量
        public static int CalculateTrainingLoad(string trainingTime, int rpeScore)
        {
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
            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            var endOfWeek = startOfWeek.AddDays(7);

            return dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .Sum(record => record.TrainingLoad ?? 0);
        }
        #endregion

        #region 計算標準差
        public static double CalculateSampleStandardDeviation(PhFitnessEntities dbContext, DateTime date)
        {
            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            var endOfWeek = startOfWeek.AddDays(7);

            var trainingRecords = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .ToList();

            var daTotals = trainingRecords
                .GroupBy(record => record.TrainingDate)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(record => record.TrainingLoad.GetValueOrDefault())
                );

            if (daTotals.Count < 1)
            {
                throw new ArgumentException("當前週的數據點數不足");
            }

            int numberOfDays = 7;

            double totalDailyLoad = daTotals.Values.Sum();

            double mean = totalDailyLoad / numberOfDays;

            double sumOfSquares = daTotals.Values
                .Select(dailyTotal => Math.Pow(dailyTotal - mean, 2))
                .Sum();

            double sampleVariance = sumOfSquares / (numberOfDays - 1);

            double standardDeviation = Math.Sqrt(sampleVariance);

            int roundedStandardDeviation = (int)Math.Round(standardDeviation, MidpointRounding.AwayFromZero);

            return roundedStandardDeviation;
        }
        #endregion

        #region 計算訓練同質性(TM)
        public static double CalculateTrainingMonotony(PhFitnessEntities dbContext, DateTime date)
        {
            double standardDeviation = CalculateSampleStandardDeviation(dbContext, date);

            var startOfWeek = date.Date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            var endOfWeek = startOfWeek.AddDays(7);

            var trainingRecords = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                .ToList();

            var daTotals = trainingRecords
                .GroupBy(record => record.TrainingDate)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(record => record.TrainingLoad.GetValueOrDefault())
                );

            if (daTotals.Count < 1)
            {
                throw new ArgumentException("當前週的數據點數不足");
            }

            int numberOfDays = 7;

            double totalDailyLoad = daTotals.Values.Sum();

            double mean = totalDailyLoad / numberOfDays;

            double trainingHomogeneity = mean / standardDeviation;

            return Math.Round(trainingHomogeneity, 2);
        }
        #endregion

        #region 計算訓練張力值(TS)
        public static double CalculateTrainingStrain(PhFitnessEntities dbContext, DateTime date)
        {
            int weeklyTrainingLoadSum = CalculateWeeklyTrainingLoadSum(dbContext, date);

            double trainingMonotony = CalculateTrainingMonotony(dbContext, date);

            double trainingStrain = weeklyTrainingLoadSum * trainingMonotony;

            return Math.Round(trainingStrain, 2);
        }
        #endregion

        #region 計算週間訓練變化
        public static double CalculateWeekToWeekChange(PhFitnessEntities dbContext, DateTime date)
        {
            int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date);

            var startOfPreviousWeek = date.Date;

            while (startOfPreviousWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfPreviousWeek = startOfPreviousWeek.AddDays(-1);
            }
            startOfPreviousWeek = startOfPreviousWeek.AddDays(-7);

            var endOfPreviousWeek = startOfPreviousWeek.AddDays(7);

            int previousWeekLoad = dbContext.SessionRPETrainingRecords
                .Where(record => record.TrainingDate >= startOfPreviousWeek && record.TrainingDate < endOfPreviousWeek)
                .Sum(record => record.TrainingLoad ?? 0);

            double variability = Math.Abs(currentWeekLoad - previousWeekLoad);

            return variability;
        }
        #endregion

        #region 計算短長期訓練量比值
        public static double CalculateACWR(PhFitnessEntities dbContext, DateTime date)
        {
            int currentWeekLoad = CalculateWeeklyTrainingLoadSum(dbContext, date);

            double pastFourWeeksLoad = 0;

            for (int i = 0; i < 4; i++)
            {
                var startOfWeek = date.Date.AddDays(-7 * i);

                while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfWeek = startOfWeek.AddDays(-1);
                }
                var endOfWeek = startOfWeek.AddDays(7);

                pastFourWeeksLoad += dbContext.SessionRPETrainingRecords
                    .Where(record => record.TrainingDate >= startOfWeek && record.TrainingDate < endOfWeek)
                    .Sum(record => record.TrainingLoad ?? 0);
            }

            double pastFourWeeksAverage = pastFourWeeksLoad / 4;

            if (pastFourWeeksAverage == 0)
            {
                throw new InvalidOperationException("過往四週的運動訓練量為零，無法計算短長期訓練量比值。");
            }

            double acwr = currentWeekLoad / pastFourWeeksAverage;

            return acwr;
        }
        #endregion
    }
}