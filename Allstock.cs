using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTest
{
    public class Allstock
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 行业
        /// </summary>
        public string Industry { get; set; }


        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }


        /// <summary>
        /// 板块
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// 上市时间
        /// </summary>
        public string CreateDatetime { get; set; }


        public override string ToString()
        {
            if (this != null)
            {
                return string.Format(@"股票代码:{0},股票名称:{1},行业:{2},地区:{3},板块:{4},上市时间:{5}",
                    this.Code,
                    this.Name,
                    this.Industry,
                    this.Area,
                    this.Market,
                    this.CreateDatetime);
            }

            return "Allstock is null";
        }
    }
}
