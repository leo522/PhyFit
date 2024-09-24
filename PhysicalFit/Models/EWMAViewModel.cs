using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class EWMAViewModel
    {
        public int N { get; set; } // 樣本數
        public double Alpha => 2.0 / (N + 1); // 根據 N 計算 Alpha
        public double EWMAWeighting => 1 - Math.Pow(1 - Alpha, N + 1); // 計算 EWMA 權重
        public double Result { get; set; } // 計算結果，這裡可以直接使用 EWMA 權重

        public void Calculate()
        {
            // 直接計算結果使用 EWMA 權重
            Result = EWMAWeighting;
        }
    }
}