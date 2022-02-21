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

            if (tradeList.Count != countDays)
            {
                return false;
            }
            //(tradeList[0].Turnover < tradeList[1].Turnover && tradeList[1].Turnover > tradeList[2].Turnover) &&
            if ((tradeList.Where(o => o.Change > 0).ToList().Count == 5 || tradeList.Where(o => o.Change < 0).ToList().Count == 1))
            {
                isUp = true;
            }

            return fiveDays > 20 && isUp;
        }

        /// <summary>
        /// 低股价、低换手率
        /// </summary>
        /// <param name="tradeList"></param>
        /// <param name="countDays"></param>
        /// <returns></returns>
        public static bool Tactics2(List<StockHis> tradeList, StockHis lastItem)
        {
            bool isUp = false;
            
            if (!tradeList.Exists(o => o.Low <= lastItem.Low)//5日最低价
                && lastItem.Turnover > 5)//换手率小于5
            {
                isUp = true;
            }

            return isUp;
        }
        /// <summary>
        /// 量比大于3、换手率小于5%，13天内涨停过
        /// </summary>
        /// <param name="tradeList"></param>
        /// <param name="lastItem"></param>
        /// <returns></returns>
        public static bool Tactics3(List<StockHis> tradeList, StockHis lastItem)
        {
            bool isUp = false;

            if (tradeList.Exists(o => o.Change >= 10)
                && lastItem != null
                && lastItem.Turnover < 5//换手率小于5
                && lastItem.Volume_ratio > 3)//量比大于3
            {
                isUp = true;
            }

            return isUp;
        }

        /// <summary>
        /// 换手率大于3、流通股本小于6000万
        /// </summary>
        /// <param name="tradeList"></param>
        /// <param name="lastItem"></param>
        /// <returns></returns>
        public static bool Tactics4(List<StockHis> tradeList, StockHis lastItem)
        {
            bool isUp = false;

            if (tradeList.Exists(o => o.Change >= 10)
                && lastItem != null
                && lastItem.Turnover > 3//换手率大于3
                && lastItem.Float_share < 6000)//流通股本小于6000万
            {
                isUp = true;
            }

            return isUp;
        }

        /// <summary>
        /// 当日跌幅区间-1~-5%，开盘价减20日均价<0.85~1%
        /// 月平均成功率：5%
        /// </summary>
        /// <param name="lastItem"></param>
        /// <returns></returns>
        public static bool Tactics5(StockHis lastItem)
        {
            bool isUp = false;

            if (lastItem == null)
            {
                return isUp;
            }

            decimal diffPirce = (lastItem.Open / lastItem.Ma20);

            if (lastItem.Change < -1 && lastItem.Change > -5//当日跌幅区间-1~-5%
                && lastItem.Open > lastItem.Ma20
                && lastItem.Volume_ratio < 1
                && lastItem.Turnover < 2.8m
                && diffPirce > 0.85m
                && diffPirce <= 1.1m)//开盘价减20日均价<0.85~1%
            {
                isUp = true;
            }

            return isUp;
        }

        /// <summary>
        /// 打板战法1
        /// </summary>
        /// <param name="lastItem"></param>
        /// <returns></returns>
        public static bool Tactics6(List<StockHis> tradeList, StockHis lastItem,string startDate)
        {
            bool isUp = false;

            if (lastItem == null)
            {
                return isUp;
            }

            //if (tradeList.Exists(o => o.Change >= 10)
            //    && lastItem != null && lastItem.Volume < lastItem.V_ma60 && lastItem.Change < 0)
            //{
            //    isUp = true;
            //}
            //排除今日股价低于60日均线
            if (tradeList.Exists(o => o.Date == startDate && o.Change >= 10)
                && lastItem != null && lastItem.Close > lastItem.Ma60)
            {
                isUp = true;
            }

            return isUp;
        }

        /// <summary>
        /// 涨幅在3-5%
        /// 量比大于等于1
        /// 换手率在5%-10%
        /// 流通市值在50亿-200亿
        /// 收盘价大于60日均线
        /// 月平均成功率：5%
        /// </summary>
        /// <param name="lastItem"></param>
        /// <returns></returns>
        public static bool Tactics7(StockHis lastItem)
        {
            bool isUp = false;

            if (lastItem == null)
            {
                return isUp;
            }

            if (lastItem.Change > 3 && lastItem.Change < 5//当日涨幅在3-5%
                && lastItem.Volume_ratio >= 1//量比大于等于1
                && lastItem.Turnover > 5m && lastItem.Turnover < 10m//换手率在5%-10%
                && lastItem.Circ_mv > 500000 && lastItem.Circ_mv < 1000000//流通市值在50亿-200亿
                && lastItem.Close > lastItem.Ma60
            )
            {
                isUp = true;
            }

            return isUp;
        }

    }
}
