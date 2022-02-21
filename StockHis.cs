using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTest
{
    /// <summary>
    /// 交易信息
    /// </summary>
    public class StockHis
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public decimal Change { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public decimal Turnover { get; set; }

        /// <summary>
        /// 5日均价
        /// </summary>
        public decimal Ma5 { get; set; }

        /// <summary>
        /// 10日均价
        /// </summary>
        public decimal Ma10 { get; set; }

        /// <summary>
        /// 20日均价
        /// </summary>
        public decimal Ma20 { get; set; }

        /// <summary>
        /// 60日均价
        /// </summary>
        public decimal Ma60 { get; set; }

        /// <summary>
        /// 5日均量
        /// </summary>
        public decimal V_ma5 { get; set; }

        /// <summary>
        /// 10日均量
        /// </summary>
        public decimal V_ma10 { get; set; }

        /// <summary>
        /// 20日均量
        /// </summary>
        public decimal V_ma20 { get; set; }

        /// <summary>
        /// 60日均量
        /// </summary>
        public decimal V_ma60 { get; set; }

        /// <summary>
        /// 量比
        /// </summary>
        public decimal Volume_ratio { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public decimal Float_share { get; set; }

        /// <summary>
        /// 流通市值
        /// </summary>
        public decimal Circ_mv { get; set; }

        public override string ToString()
        {
            if (this != null)
            {
                return string.Format(@"股票代码:{0},交易日期:{1},开盘价:{2},收盘价:{3},最高价:{4},最低价:{5},成交量:{6},涨跌幅:{7},换手率:{8},量比:{15},5日均价:{9},10日均价:{10},20日均价:{11},60日均价:{12},5日均量:{13},10日均量:{14},20日均量:{15},60日均量:{16}",
                    this.Code,
                    this.Date,
                    this.Open,
                    this.Close,
                    this.High,
                    this.Low,
                    this.Volume,
                    this.Change,
                    this.Turnover,
                    this.Ma5,
                    this.Ma10,
                    this.Ma20,
                    this.Ma60,
                    this.V_ma5,
                    this.V_ma10,
                    this.V_ma20,
                    this.V_ma60,
                    this.Volume_ratio);
            }

            return "Allstock is null";
        }

    }
}
