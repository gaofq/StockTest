using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTest
{
    /// <summary>
    /// 策略工具类
    /// </summary>
    public class TacticsUtils
    {
        /// <summary>
        /// 策略1:5日涨幅大于20%
        /// 月平均成功率：15%
        /// </summary>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public static bool Tactics1(List<StockHis> tradeList,int countDays)
        {
            //5日涨幅大于20%
            decimal fiveDays = tradeList.Sum(o => o.Change);

            tradeList = tradeList.OrderByDescending(x => x.Date).ToList();
            bool isUp = false;

            if ((tradeList[0].Turnover < tradeList[1].Turnover && tradeList[1].Turnover > tradeList[2].Turnover)
                && (tradeList.Where(o => o.Change > 0).ToList().Count == countDays || tradeList.Where(o => o.Change < 0).ToList().Count == 1))
            {
                isUp = true;
            }

            return fiveDays > 20 && isUp;
        }

        public static bool Tactics2()
        {
            return false;
        }
        public static bool Tactics3()
        {
            return false;
        }

        public static bool Tactics4()
        {
            return false;
        }

        public static bool Tactics5()
        {
            return false;
        }

    }
}
