using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // 服务器地址；端口号；数据库；用户名；密码
            //string connectStr = "server=119.91.80.171;port=3306;database=test;user=gaofq;password=n!ElGgNs.3OT"; // 用户名和密码在MySQL定义的
            string connectStr = "server=127.0.0.1;port=3306;database=test;user=root;password=n!ElGgNs.3OT"; // 用户名和密码在MySQL定义的
            // 创建连接
            MySqlConnection conn = new MySqlConnection(connectStr);

            try
            {
                // 打开连接
                Console.WriteLine("已经建立连接");

                //计算当天涨副
                List<Allstock> allstocks = QueryAllStock(conn); //查询所有股票
                var tradeCalList = QueryAllTradeCal(conn);

                //DateTime countdate = new DateTime(2022,2,8);//计算时间
                DateTime countdate = DateTime.Now;//计算时间
                string date = countdate.ToString("yyyyMMdd");//结束时间
               
                bool isopen = false;//当天是否交易

                if (tradeCalList.Exists(o=>o.CalDate== date))
                {
                    isopen = tradeCalList.FirstOrDefault(o => o.CalDate == date).IsOpen;
                }

                if (isopen)
                {
                    CountFiveDaysTop(conn, allstocks, countdate, tradeCalList);//计算5日涨跌副
                }
                else
                {
                    Console.WriteLine("非交易日不执行");
                }


                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //List<Allstock> allstocks = QueryAllStock(conn); //查询所有股票
                //var tradeCalList = QueryAllTradeCal(conn);

                //for (int i = 26; i < 30; i++)
                //{
                //    DateTime countdate = new DateTime(2021, 11, i);//计算时间

                //    if (tradeCalList.Exists(o => o.CalDate == countdate.ToString("yyyyMMdd") && o.IsOpen))
                //    {
                //        string date = countdate.ToString("yyyyMMdd");//结束时间
                //        //var allstocks = QueryAllStockByTop100(conn, date);
                //        CountFiveDaysTop(conn, allstocks, countdate, tradeCalList);//计算5日涨跌副
                //    }
                //}

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //ageList.Clear();
                //List<Allstock>  allstocks = QueryAllStock(conn); //查询所有股票
                //var tradeCalList = QueryAllTradeCal(conn);

                //for (int i = 4; i < 31; i++)
                //{
                //    DateTime countdate = new DateTime(2021, 9, i);//计算时间

                //    if (tradeCalList.Exists(o => o.CalDate == countdate.ToString("yyyyMMdd") && o.IsOpen))
                //    {
                //        RunStock(conn, allstocks, countdate, tradeCalList);
                //    }
                //}

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                // 关闭连接
                conn.Close();
                Console.WriteLine("数据库已关闭");
            }
            
            System.Environment.Exit(0);
        }

        static List<decimal> ageList = new List<decimal>();
        private static void RunStock(MySqlConnection conn, List<Allstock> allstocks, DateTime countdate, List<TradeCal> tradeCalList)
        {
            int count = 0;

            int startDays = -5;//前几个交易日
            int endDays = 5;//后几个交易日

            

            string enddate = countdate.ToString("yyyyMMdd");//结束时间
            Console.WriteLine(string.Format("计算时间：{0}，往前{1}个交易日，往后{2}个交易日", enddate, startDays, endDays));

            int days1 = DiffWeekend3(countdate, startDays, tradeCalList);
            string startdate = countdate.AddDays(days1).ToString("yyyyMMdd");//开始时间
            
            
            //var hotIndustrList = QueryHotIndustry(conn, startdate, enddate);
            Console.WriteLine(string.Format("开始时间：{0}，结束时间：{1}", startdate, enddate));
            int days2 = DiffWeekend3(countdate, 1, tradeCalList);
            int days3 = DiffWeekend3(countdate.AddDays(days2), endDays, tradeCalList);


            ////Console.WriteLine(string.Format("交易时间：{0}", enddate1));
            //conn.Close();
            //conn.Open();
            string startdate1 = countdate.AddDays(days2).ToString("yyyyMMdd");//未来几天交易开始时间
            string enddate2 = countdate.AddDays(days3).ToString("yyyyMMdd");//未来几天交易结束时间
            Console.WriteLine(string.Format("开始时间1：{0}，结束时间1：{1}", startdate1, enddate2));
            var nextDayTradeList = QueryNextDayList(conn, startdate1, enddate2);//下3个交易日
            
            int count1 = 0;

            List<StockHis> countStocks = new List<StockHis>();

            List<StockHis> lastTradeList = new List<StockHis>();

            foreach (var item in allstocks)
            {
                //if (!hotIndustrList.Exists(o => o.Industry == item.Industry))
                //{
                //    continue;
                //}

                var tradeList = QueryAllStock(conn, item.Code, startdate, enddate);

                if (tradeList != null && tradeList.Count > 0)//&& tradeList.Count == countDays
                {

                    // bool isMatch = TacticsUtils.Tactics6(tradeList, countDays);//匹配策略

                    var lastItem = tradeList.FirstOrDefault(x => x.Date == enddate);

                    bool isMatch = false;
                    if (lastItem != null)
                    {
                        tradeList.Remove(lastItem);

                        //isMatch = TacticsUtils.Tactics6(tradeList, lastItem, startdate);//匹配策略
                        isMatch = TacticsUtils.Tactics7(lastItem);//匹配策略
                    }

                    if (lastItem != null && isMatch)
                    {
                        lastTradeList.Add(lastItem);
                        lastTradeList.AddRange(tradeList);
                        count++;
                        Console.WriteLine(string.Format("{0}符合条件：{1}\r\n{2}", count, item.ToString(), lastItem.ToString()));
                        //foreach (var item1 in tradeList)
                        //{
                        //    Console.WriteLine(string.Format("{0}", item1.ToString()));
                        //}
                        countStocks.Add(lastItem);

                        //if (nextDayTradeList.Exists(o => o.Code == item.Code))
                        //{
                        //    count1++;
                        //    var item1 = nextDayTradeList.FirstOrDefault(o => o.Code == item.Code);
                        //    var stockItem = allstocks.FirstOrDefault(o => o.Code == item.Code);
                        //    Console.WriteLine(string.Format("{0}已匹配：{1}", count1, item1.ToString()));
                        //    TradeMatchSuccess success = new TradeMatchSuccess();
                        //    success.Code = item.Code;
                        //    success.TradeDate = enddate;
                        //    success.StartTradeDate = startdate;
                        //    success.EndTradeDate = enddate;
                        //    success.StartTradeDate1 = startdate1;
                        //    success.EndTradeDate1 = enddate2;

                        //    AddTradeMatch(conn, success);
                        //    //var showLastList = nextDayTradeList.Where(o => o.Code == item.Code).OrderBy(o => o.Date).ToList();
                        //    //foreach (var item3 in showLastList)
                        //    //{
                        //    //    Console.WriteLine(string.Format("最近交易：{0}", item3.ToString()));
                        //    //}

                        //}
                    }
                }
            }


            if (count > 0)
            {
                ageList.Add(Convert.ToDecimal(((decimal)count1 / (decimal)count * 100).ToString("f2")));
                Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, ((decimal)count1 / (decimal)count * 100).ToString("f2"), startdate1, (ageList.Sum() / ageList.Count).ToString("f2")));
            }
            else
            {
                //ageList.Add(0);
                Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, 0, startdate1, 0));
            }


            //Insert(conn); // 测试插入
            //Update(conn); // 测试更新
            //Delete(conn); // 测试删除

        }

        private static void RunStock1(MySqlConnection conn, List<Allstock> allstocks, DateTime countdate)
        {
            int count = 0;

            int days = 5;//计算天数

            string enddate = countdate.ToString("yyyyMMdd");//结束时间

            int days1 = DiffWeekend1(countdate.AddDays(-days), countdate);
            int days2 = DiffWeekend(countdate, countdate.AddDays(1));

            string startdate = countdate.AddDays(-days1).ToString("yyyyMMdd");//开始时间
            string enddate1 = countdate.AddDays(days2).ToString("yyyyMMdd");//交易时间
            conn.Close();
            conn.Open();
            //var hotIndustrList = QueryHotIndustry(conn, startdate, enddate);
            Console.WriteLine(string.Format("开始时间：{0}，结束时间：{1}", startdate, enddate));

            ////Console.WriteLine(string.Format("交易时间：{0}", enddate1));
            //conn.Close();
            //conn.Open();
            var nextDayTradeList = QueryNextDayList(conn, enddate1);
            conn.Close();
            int count1 = 0;
            if (nextDayTradeList != null && nextDayTradeList.Count > 0)
            {
                foreach (var item in nextDayTradeList)
                {
                    conn.Close();
                    conn.Open();
                    count++;
                    var tradeList = QueryAllStock(conn, item.Code, startdate, enddate);
                    
                    if (tradeList != null && tradeList.Count == days)
                    {
                        tradeList = tradeList.OrderByDescending(x => x.Date).ToList();
                        var lastItem = allstocks.FirstOrDefault(x => x.Code == item.Code);
                        if (lastItem != null)
                        {
                            Console.WriteLine(string.Format("{0}符合条件：{1}\r\n{2}", count, lastItem.ToString(), item.ToString()));
                            foreach (var item1 in tradeList)
                            {
                                Console.WriteLine(string.Format("{0}", item1.ToString()));
                            }
                        }
                        //tradeList.Remove(lastItem);
                        

                        ////5日涨幅大于20%
                        //decimal fiveDays = tradeList.Sum(o => o.Change);
                        //var lastItem = tradeList.FirstOrDefault(x => x.Date == enddate);
                        //tradeList = tradeList.OrderByDescending(x => x.Date).ToList();
                        //bool isUp = false;

                        ////if ((tradeList[0].Turnover < tradeList[1].Turnover && tradeList[1].Turnover > tradeList[2].Turnover))
                        ////{
                        ////    isUp = true;
                        ////}

                        //if ((tradeList[0].Turnover < tradeList[1].Turnover && tradeList[1].Turnover > tradeList[2].Turnover)
                        //    && (tradeList.Where(o => o.Change > 0).ToList().Count == days || tradeList.Where(o => o.Change < 0).ToList().Count == 1))
                        //{
                        //    isUp = true;
                        //}


                    }
                }
            }

            


            //if (count > 0)
            //{
            //    ageList.Add(Convert.ToDecimal(((decimal)count1 / (decimal)count * 100).ToString("f2")));
            //    Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, ((decimal)count1 / (decimal)count * 100).ToString("f2"), enddate1, (ageList.Sum() / ageList.Count).ToString("f2")));
            //}
            //else
            //{
            //    //ageList.Add(0);
            //    Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, 0, enddate1, 0));
            //}


            //Insert(conn); // 测试插入
            //Update(conn); // 测试更新
            //Delete(conn); // 测试删除

        }

        //统计5日涨跌副
        private static void CountFiveDaysTop(MySqlConnection conn, List<Allstock> allstocks, DateTime countdate, List<TradeCal> tradeCalList)
        {
            int count = 0;

            int startDays = -5;//前几个交易日
            int endDays = 5;//后几个交易日

            string enddate = countdate.ToString("yyyyMMdd");//结束时间
            Console.WriteLine(string.Format("计算时间：{0}，往前{1}个交易日，往后{2}个交易日", enddate, startDays, endDays));

            int days1 = DiffWeekend3(countdate, startDays, tradeCalList);
            string startdate = countdate.AddDays(days1).ToString("yyyyMMdd");//开始时间

            //var hotIndustrList = QueryHotIndustry(conn, startdate, enddate);
            Console.WriteLine(string.Format("开始时间：{0}，结束时间：{1}", startdate, enddate));
            int days2 = DiffWeekend3(countdate, 1, tradeCalList);
            int days3 = DiffWeekend3(countdate.AddDays(days2), endDays, tradeCalList);


            ////Console.WriteLine(string.Format("交易时间：{0}", enddate1));
            //conn.Close();
            //conn.Open();
            string startdate1 = countdate.AddDays(days2).ToString("yyyyMMdd");//未来几天交易开始时间
            string enddate2 = countdate.AddDays(days2 + days3).ToString("yyyyMMdd");//未来几天交易结束时间
            Console.WriteLine(string.Format("开始时间1：{0}，结束时间1：{1}", startdate1, enddate2));
            //var nextDayTradeList = QueryNextDayList(conn, startdate1, enddate2);//下3个交易日



            List<StockHis> countStocks = new List<StockHis>();

            List<StockHis> lastTradeList = new List<StockHis>();

            foreach (var item in allstocks)
            {
                //if (!hotIndustrList.Exists(o => o.Industry == item.Industry))
                //{
                //    continue;
                //}

                var tradeList = QueryAllStock(conn, item.Code, startdate, enddate);

                if (tradeList != null && tradeList.Count >= 5)//&& tradeList.Count == countDays
                {
                    var nextDayTradeList = QueryAllStock(conn, item.Code, startdate1, enddate2);//下3个交易日


                    tradeList = tradeList.OrderByDescending(o => o.Date).ToList();

                    StockFiveDaysTop fiveDay = GetStockFiveDaysTopByCode(conn, enddate, item.Code);

                    if (fiveDay == null)
                    {
                        fiveDay = new StockFiveDaysTop();
                        fiveDay.Code = item.Code;
                        fiveDay.StockName = item.Name;
                        fiveDay.TradeDate = enddate;
                        Console.WriteLine(String.Format("正在刷新5日涨跌副,代码：{0},名称：{1},交易时间：{2}，时间：{3}", item.Code, item.Name, enddate,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        //计算涨跌副
                        decimal nowPrice = tradeList[0].Close;//今天收盘价
                        decimal fiveDaysAgoPrice = tradeList[4].Open;//5个交易日前开盘价

                        if (nowPrice > 0 && fiveDaysAgoPrice > 0)
                        {
                            fiveDay.Change = Math.Round((nowPrice - fiveDaysAgoPrice) / fiveDaysAgoPrice * 100, 2, MidpointRounding.AwayFromZero);
                        }

                        fiveDay.FDAPirce = fiveDaysAgoPrice;
                        fiveDay.Pirce = nowPrice;
                        fiveDay.DayChange = tradeList[0].Change;

                        AddStockFiveDaysTop(conn, fiveDay);
                    }

                    if (fiveDay.DayThreeOpenPrice == 0 && nextDayTradeList != null && nextDayTradeList.Count >= 2)
                    {
                        Console.WriteLine(String.Format("正在刷新第三天收益率,代码：{0},名称：{1},交易时间：{2}", item.Code, item.Name, enddate));
                        nextDayTradeList = nextDayTradeList.OrderBy(o => o.Date).ToList();

                        fiveDay.DayTwoOpenPrice = nextDayTradeList[0].Open;
                        fiveDay.DayTwoClosePrice = nextDayTradeList[0].Close;
                        var dayThreeHighPrice = nextDayTradeList[0].High;
                        var dayThreeLowPrice = nextDayTradeList[0].Low;


                        fiveDay.DayThreeOpenPrice = nextDayTradeList[1].Open;
                        fiveDay.DayThreeClosePrice = nextDayTradeList[1].Close;
                        fiveDay.DayThreeHighPrice = nextDayTradeList[1].High;
                        fiveDay.DayThreeLowPrice = nextDayTradeList[1].Low;
                        /*
                         *  收益率公式：（现价-上一个交易日收盘价）÷上一个交易日收盘价×100%。
                         */
                        //开盘买入开盘卖出收益率
                        fiveDay.DayThreeOPYieldRateOne = Math.Round((fiveDay.DayThreeOpenPrice - fiveDay.DayTwoOpenPrice) / fiveDay.DayTwoOpenPrice * 100, 2, MidpointRounding.AwayFromZero);
                        //开盘买入收盘卖出收益率
                        fiveDay.DayThreeCPYieldRateOne = Math.Round((fiveDay.DayThreeClosePrice - fiveDay.DayTwoOpenPrice) / fiveDay.DayTwoOpenPrice * 100, 2, MidpointRounding.AwayFromZero);
                        //开盘买入最高收益率
                        fiveDay.DayThreeMYieldRateOne = Math.Round((fiveDay.DayThreeHighPrice - fiveDay.DayTwoOpenPrice) / fiveDay.DayTwoOpenPrice * 100, 2, MidpointRounding.AwayFromZero);
                        
                        //收盘买入开盘卖出收益率
                        fiveDay.DayThreeOPYieldRateTwo = Math.Round((fiveDay.DayThreeOpenPrice - fiveDay.DayTwoClosePrice) / fiveDay.DayTwoClosePrice * 100, 2, MidpointRounding.AwayFromZero);
                        //收盘买入收盘卖出收益率
                        fiveDay.DayThreeCPYieldRateTwo = Math.Round((fiveDay.DayThreeClosePrice - fiveDay.DayTwoClosePrice) / fiveDay.DayTwoClosePrice * 100, 2, MidpointRounding.AwayFromZero);
                        //收盘买入最高收益率
                        fiveDay.DayThreeMYieldRateTwo = Math.Round((fiveDay.DayThreeHighPrice - fiveDay.DayTwoClosePrice) / fiveDay.DayTwoClosePrice * 100, 2, MidpointRounding.AwayFromZero);

                        //最低价买入开盘卖出收益率
                        fiveDay.DayThreeOPYieldRateThree = Math.Round((fiveDay.DayThreeOpenPrice - dayThreeLowPrice) / dayThreeLowPrice * 100, 2, MidpointRounding.AwayFromZero);
                        //最低价买入收盘卖出收益率
                        fiveDay.DayThreeCPYieldRateThree = Math.Round((fiveDay.DayThreeClosePrice - dayThreeLowPrice) / dayThreeLowPrice * 100, 2, MidpointRounding.AwayFromZero);
                        //最低价买入最高收益率
                        fiveDay.DayThreeMYieldRateThree = Math.Round((fiveDay.DayThreeHighPrice - dayThreeLowPrice) / dayThreeLowPrice * 100, 2, MidpointRounding.AwayFromZero);


                        UpdateStockThreeYieldRate(conn, fiveDay);
                    }

                    if (string.IsNullOrEmpty(fiveDay.MaxPriceDate) && nextDayTradeList != null && nextDayTradeList.Count >= 5)
                    {
                        Console.WriteLine(String.Format("正在刷新5日内收益率,代码：{0},名称：{1},交易时间：{2}", item.Code, item.Name, enddate));
                        nextDayTradeList = nextDayTradeList.OrderBy(o => o.Date).ToList();
                        var daytwo = nextDayTradeList[0];
                        fiveDay.DayTwoOpenPrice = daytwo.Open;
                        fiveDay.DayTwoClosePrice = daytwo.Close;

                        //最高价不能第二天
                        var daytwoDate = daytwo.Date;
                        var highPrice = nextDayTradeList.Where(o => o.Date != daytwo.Date).Max(o => o.High);//5日最高价
                        var highEntity = nextDayTradeList.Where(o => o.Date != daytwo.Date).FirstOrDefault(o => o.High == highPrice);
                        fiveDay.MaxPriceDate = highEntity.Date;
                        fiveDay.MaxPrice = highPrice;

                        //最低价与最高价不能为同一天
                        var lowPrice = nextDayTradeList.Where(o => o.Date != highEntity.Date).Min(o => o.Low);//5日最低价
                        var lowEntity = nextDayTradeList.Where(o => o.Date != highEntity.Date).FirstOrDefault(o => o.Low == lowPrice);

                        fiveDay.MinPrice = lowPrice;
                        fiveDay.MinPriceDate = lowEntity.Date;

                        fiveDay.OpenPriceYieldRate = Math.Round((fiveDay.MaxPrice - fiveDay.DayTwoOpenPrice) / fiveDay.DayTwoOpenPrice * 100, 2, MidpointRounding.AwayFromZero);
                        fiveDay.ClosePriceYieldRate = Math.Round((fiveDay.MaxPrice - fiveDay.DayTwoClosePrice) / fiveDay.DayTwoClosePrice * 100, 2, MidpointRounding.AwayFromZero);
                        fiveDay.MaxYieldRate = Math.Round((fiveDay.MaxPrice - fiveDay.MinPrice) / fiveDay.MinPrice * 100, 2, MidpointRounding.AwayFromZero);

                        UpdateStockFiveDaysTop(conn, fiveDay);
                    }
                }
            }
        }

        /// <summary>  
        /// 获取日期段里的工作日【日】  
        /// </summary>  
        internal static int DiffWeekend(DateTime startDate, DateTime endDate)
        {
            var totalDays = endDate.Subtract(startDate).Days;
            int result = 0;

            if (startDate.DayOfWeek == System.DayOfWeek.Saturday 
                || startDate.DayOfWeek == System.DayOfWeek.Sunday
                || endDate.DayOfWeek == System.DayOfWeek.Saturday
                || endDate.DayOfWeek == System.DayOfWeek.Sunday)
            {
                result = 1;
            }

            for (int i = 0; i <= totalDays; i++)
            {
                DateTime tempdt = startDate.Date.AddDays(i);

                if (tempdt.DayOfWeek == System.DayOfWeek.Saturday || tempdt.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    result++;
                }
            }

            return result + totalDays;
        }

        /// <summary>  
        /// 获取日期段里的工作日【日】  
        /// </summary>  
        internal static int DiffWeekend1(DateTime startDate, DateTime endDate)
        {
            var totalDays = endDate.Subtract(startDate).Days;
            int result = 0;

            for (int i = 0; i <= totalDays; i++)
            {
                DateTime tempdt = startDate.Date.AddDays(i);

                if (tempdt.DayOfWeek == System.DayOfWeek.Saturday || tempdt.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    result++;
                }
            }

            return result + totalDays - 1;
        }

        /// <summary>  
        /// 获取日期段里的工作日【日】  
        /// </summary>  
        internal static int DiffWeekend3(DateTime startDate, int days, List<TradeCal> tradeCalList)
        {
            int result = 0;//总天数
            int tradeDays = 0;//交易天数
            for (int i = 1; i <= 999; i++)
            {
                if (days>0)
                {
                    DateTime tempdt = startDate.Date.AddDays(i);

                    result++;

                    if (tradeCalList.Exists(o => o.CalDate == tempdt.ToString("yyyyMMdd") && o.IsOpen))
                    {
                        tradeDays++;

                        if (days == tradeDays)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    DateTime tempdt = startDate.Date.AddDays(-i);

                    result--;

                    if (tradeCalList.Exists(o => o.CalDate == tempdt.ToString("yyyyMMdd") && o.IsOpen))
                    {
                        tradeDays--;

                        if (days == tradeDays)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 查询股票交易信息
        /// </summary>
        /// <param name="conn"></param>
        private static List<StockHis> QueryNextDayList(MySqlConnection conn,string enddate)
        {
            List<StockHis> allstocks = new List<StockHis>();
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"SELECT stock_his.*,allstock.*,stock_dailyhis.volume_ratio,float_share FROM stock_his
                        LEFT JOIN allstock ON allstock.`code` = stock_his.s_code
                        LEFT JOIN stock_dailyhis ON stock_dailyhis.ts_code = stock_his.s_code AND stock_dailyhis.trade_date = date
                        WHERE date = '{0}'
                        AND p_change > 5 AND p_change <= 20
                        AND `open` < stock_his.`close`
                        AND market <> '科创板'
                        AND volume_ratio is not null
                        ORDER BY p_change ", enddate);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                // 读取数据
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) // true表示能读取该行数据
                {
                    //// 方式一：访问数组
                    //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                    //                                                                // 方式二：根据列数
                    //Console.WriteLine(reader.GetInt32(0));
                    //// 方式三：根据列名
                    ////Console.WriteLine(reader.GetInt32("user_id"));

                    StockHis allstock = new StockHis();
                    allstock.Code = reader[0].ToString();
                    allstock.Date = reader[1].ToString();
                    allstock.Open = reader.GetDecimal("open");
                    allstock.Close = reader.GetDecimal("close");
                    allstock.High = reader.GetDecimal("high");
                    allstock.Low = reader.GetDecimal("low");
                    allstock.Volume = reader.GetDecimal("volume");
                    allstock.Change = reader.GetDecimal("p_change");
                    allstock.Turnover = reader.GetDecimal("turnover");
                    allstock.Ma5 = reader.GetDecimal("ma5");
                    allstock.Ma10 = reader.GetDecimal("ma10");
                    allstock.Ma20 = reader.GetDecimal("ma20");
                    allstock.V_ma5 = reader.GetDecimal("v_ma5");
                    allstock.V_ma10 = reader.GetDecimal("v_ma10");
                    allstock.V_ma20 = reader.GetDecimal("v_ma20");
                    allstock.Volume_ratio = reader.GetDecimal("volume_ratio");
                    allstock.Float_share = reader.GetDecimal("float_share");

                    allstocks.Add(allstock);
                }
            }
            catch (Exception ex)
            {
                return null;
            }


            return allstocks;
        }

        /// <summary>
        /// 查询股票交易信息
        /// </summary>
        /// <param name="conn"></param>
        private static List<StockHis> QueryNextDayList(MySqlConnection conn, string startdate, string enddate)
        {

            List<StockHis> allstocks = new List<StockHis>();
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"SELECT stock_his.*,allstock.*,stock_dailyhis.volume_ratio,float_share FROM stock_his
                                            LEFT JOIN allstock ON allstock.`code` = stock_his.s_code
                                            LEFT JOIN stock_dailyhis ON stock_dailyhis.ts_code = stock_his.s_code AND stock_dailyhis.trade_date = date
                                            WHERE date BETWEEN '{0}' AND '{1}'
                                            AND s_code IN (
	                                            SELECT s_code FROM stock_his
	                                            LEFT JOIN allstock ON allstock.`code` = stock_his.s_code
	                                            WHERE date BETWEEN '{0}' AND '{1}'
	                                            AND `open` < stock_his.`close`
	                                            AND market <> '科创板'
	                                            GROUP BY s_code HAVING SUM(p_change) > 10
                                            )
                                            AND volume_ratio is not null
                                            ORDER BY p_change ", startdate, enddate);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                // 读取数据
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) // true表示能读取该行数据
                {
                    //// 方式一：访问数组
                    //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                    //                                                                // 方式二：根据列数
                    //Console.WriteLine(reader.GetInt32(0));
                    //// 方式三：根据列名
                    ////Console.WriteLine(reader.GetInt32("user_id"));

                    StockHis allstock = new StockHis();
                    allstock.Code = reader[0].ToString();
                    allstock.Date = reader[1].ToString();
                    allstock.Open = reader.GetDecimal("open");
                    allstock.Close = reader.GetDecimal("close");
                    allstock.High = reader.GetDecimal("high");
                    allstock.Low = reader.GetDecimal("low");
                    allstock.Volume = reader.GetDecimal("volume");
                    allstock.Change = reader.GetDecimal("p_change");
                    allstock.Turnover = reader.GetDecimal("turnover");
                    allstock.Ma5 = reader.GetDecimal("ma5");
                    allstock.Ma10 = reader.GetDecimal("ma10");
                    allstock.Ma20 = reader.GetDecimal("ma20");
                    allstock.Ma60 = reader.GetDecimal("ma60");
                    allstock.V_ma5 = reader.GetDecimal("v_ma5");
                    allstock.V_ma10 = reader.GetDecimal("v_ma10");
                    allstock.V_ma20 = reader.GetDecimal("v_ma20");
                    allstock.V_ma60 = reader.GetDecimal("v_ma60");
                    allstock.Volume_ratio = reader.GetDecimal("volume_ratio");
                    allstock.Float_share = reader.GetDecimal("float_share");

                    allstocks.Add(allstock);
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                return null;
            }


            return allstocks;
        }

        /// <summary>
        /// 查询股票交易信息
        /// </summary>
        /// <param name="conn"></param>
        private static List<StockHis> QueryAllStock(MySqlConnection conn,string code, string startdate,string enddate)
        {
            List<StockHis> allstocks = new List<StockHis>();
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"SELECT stock_{0}.*,stock_dailyhis.volume_ratio,float_share,circ_mv FROM stock_{0}
                                            LEFT JOIN stock_dailyhis ON stock_dailyhis.trade_date = stock_{0}.date
                                            AND stock_dailyhis.ts_code = '{0}' AND stock_dailyhis.trade_date = date
                                            WHERE date BETWEEN '{1}' AND '{2}'", code, startdate, enddate);
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                // 读取数据
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) // true表示能读取该行数据
                {
                    //// 方式一：访问数组
                    //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                    //                                                                // 方式二：根据列数
                    //Console.WriteLine(reader.GetInt32(0));
                    //// 方式三：根据列名
                    ////Console.WriteLine(reader.GetInt32("user_id"));

                    StockHis allstock = new StockHis();
                    allstock.Code = code;
                    allstock.Date = reader[0].ToString();
                    allstock.Open = reader.GetDecimal("open");
                    allstock.Close = reader.GetDecimal("close");
                    allstock.High = reader.GetDecimal("high");
                    allstock.Low = reader.GetDecimal("low");
                    allstock.Volume = reader.GetDecimal("volume");
                    allstock.Change = reader.GetDecimal("p_change");
                    allstock.Turnover = reader.GetDecimal("turnover");
                    allstock.Ma5 = reader.GetDecimal("ma5");
                    allstock.Ma10 = reader.GetDecimal("ma10");
                    allstock.Ma20 = reader.GetDecimal("ma20");
                    allstock.Ma60 = reader.GetDecimal("ma60");
                    allstock.V_ma5 = reader.GetDecimal("v_ma5");
                    allstock.V_ma10 = reader.GetDecimal("v_ma10");
                    allstock.V_ma20 = reader.GetDecimal("v_ma20");
                    allstock.V_ma60 = reader.GetDecimal("v_ma60");

                    decimal volume_ratio = 0;
                    if (!string.IsNullOrWhiteSpace(reader["Volume_ratio"].ToString())
                        && decimal.TryParse(reader["Volume_ratio"].ToString(), out volume_ratio))
                    {
                        allstock.Volume_ratio = volume_ratio;
                    }

                    if (!string.IsNullOrWhiteSpace(reader["float_share"].ToString()))
                    {
                        allstock.Float_share = reader.GetDecimal("float_share");
                    }

                    if (!string.IsNullOrWhiteSpace(reader["circ_mv"].ToString()))
                    {
                        allstock.Circ_mv = reader.GetDecimal("circ_mv");
                    }

                    allstocks.Add(allstock);
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                return null;
            }
            

            return allstocks;
        }

        /// <summary>
        /// 新增匹配成功记录
        /// </summary>
        /// <param name="conn"></param>
        private static bool AddTradeMatch(MySqlConnection conn, TradeMatchSuccess tradeMatch)
        {
            bool result = false;
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"INSERT INTO tradematchsuccess VALUES('{0}','{1}','{2}','{3}','{4}','{5}')",  tradeMatch.TradeDate, tradeMatch.Code, tradeMatch.StartTradeDate, tradeMatch.EndTradeDate, tradeMatch.StartTradeDate1, tradeMatch.EndTradeDate1);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery() > 0;
                conn.Close();
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }



        /// <summary>
        /// 新增5日涨跌榜
        /// </summary>
        /// <param name="conn"></param>
        private static bool AddStockFiveDaysTop(MySqlConnection conn, StockFiveDaysTop tradeMatch)
        {
            bool result = false;
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"INSERT INTO StockFiveDaysTop(TradeDate,`Code`,StockName,`Change`,FDAPirce,Pirce,DayChange) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", 
                    tradeMatch.TradeDate, tradeMatch.Code, tradeMatch.StockName, tradeMatch.Change, 
                    tradeMatch.FDAPirce, tradeMatch.Pirce, tradeMatch.DayChange);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery() > 0;
                conn.Close();
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// 更新第三天收益率
        /// </summary>
        /// <param name="conn"></param>
        private static bool UpdateStockThreeYieldRate(MySqlConnection conn, StockFiveDaysTop tradeMatch)
        {
            bool result = false;

            try
            {
                // 创建命令
                string sql = string.Format(@"UPDATE stockfivedaystop SET DayThreeOpenPrice ={0},DayThreeClosePrice= {1},DayThreeHighPrice= {2},
                                            DayThreeLowPrice= {3},DayThreeOPYieldRateOne= {4},DayThreeCPYieldRateOne= {5},DayThreeMYieldRateOne= {6},
                                            DayThreeOPYieldRateTwo= {7},DayThreeCPYieldRateTwo= {8},DayThreeMYieldRateTwo= {9},
                                            DayThreeOPYieldRateThree= {10},DayThreeCPYieldRateThree= {11},DayThreeMYieldRateThree= {12}
                                            WHERE TradeDate = '{13}'  AND `Code` = '{14}'",
                                            tradeMatch.DayThreeOpenPrice, tradeMatch.DayThreeClosePrice, tradeMatch.DayThreeHighPrice,
                                            tradeMatch.DayThreeLowPrice, tradeMatch.DayThreeOPYieldRateOne, tradeMatch.DayThreeCPYieldRateOne, tradeMatch.DayThreeMYieldRateOne,
                                            tradeMatch.DayThreeOPYieldRateTwo, tradeMatch.DayThreeCPYieldRateTwo, tradeMatch.DayThreeMYieldRateTwo, 
                                            tradeMatch.DayThreeOPYieldRateThree, tradeMatch.DayThreeCPYieldRateThree, tradeMatch.DayThreeMYieldRateThree,
                                            tradeMatch.TradeDate, tradeMatch.Code);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery() > 0;
                conn.Close();
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// 更新5日涨跌榜
        /// </summary>
        /// <param name="conn"></param>
        private static bool UpdateStockFiveDaysTop(MySqlConnection conn, StockFiveDaysTop tradeMatch)
        {
            bool result = false;
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"UPDATE stockfivedaystop SET DayTwoOpenPrice ={0},DayTwoClosePrice= {1},MaxPrice= {2},
                                            MaxPriceDate= {3},MinPrice= {4},MinPriceDate= {5},OpenPriceYieldRate= {6},
                                            ClosePriceYieldRate= {7},MaxYieldRate= {8}
                                            WHERE TradeDate = '{9}'  AND `Code` = '{10}'", 
                                            tradeMatch.DayTwoOpenPrice, tradeMatch.DayTwoClosePrice, tradeMatch.MaxPrice,
                                            tradeMatch.MaxPriceDate, tradeMatch.MinPrice, tradeMatch.MinPriceDate, tradeMatch.OpenPriceYieldRate,
                                            tradeMatch.ClosePriceYieldRate, tradeMatch.MaxYieldRate,
                                            tradeMatch.TradeDate, tradeMatch.Code);

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery() > 0;
                conn.Close();
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// 查询热门行业
        /// </summary>
        /// <param name="conn"></param>
        private static StockFiveDaysTop GetStockFiveDaysTopByCode(MySqlConnection conn, string tradeDate, string code)
        {
            StockFiveDaysTop stockFiveDaysTop = null;
            string sql = string.Format(@"SELECT * FROM stockfivedaystop
                                        WHERE TradeDate = '{0}'  AND `Code` = '{1}' ", tradeDate, code);
            //Console.WriteLine(sql);
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            else
            {
                conn.Close();
                conn.Open();
            }


            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 读取数据
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) // true表示能读取该行数据
            {
                //// 方式一：访问数组
                //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                //                                                                // 方式二：根据列数
                //Console.WriteLine(reader.GetInt32(0));
                //// 方式三：根据列名
                ////Console.WriteLine(reader.GetInt32("user_id"));

                stockFiveDaysTop = new StockFiveDaysTop();

                stockFiveDaysTop.TradeDate = reader.GetString("TradeDate");
                stockFiveDaysTop.Code = reader.GetString("Code");
                stockFiveDaysTop.StockName = reader.GetString("StockName");
                stockFiveDaysTop.Change = reader.GetDecimal("Change");
                stockFiveDaysTop.FDAPirce = reader.GetDecimal("FDAPirce");
                stockFiveDaysTop.Pirce = reader.GetDecimal("Pirce");
                stockFiveDaysTop.DayChange = reader.GetDecimal("DayChange");
                
                if (!string.IsNullOrWhiteSpace(reader["DayTwoOpenPrice"].ToString()))
                {
                    stockFiveDaysTop.DayTwoOpenPrice = reader.GetDecimal("DayTwoOpenPrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayTwoClosePrice"].ToString()))
                {
                    stockFiveDaysTop.DayTwoClosePrice = reader.GetDecimal("DayTwoClosePrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["MaxPrice"].ToString()))
                {
                    stockFiveDaysTop.MaxPrice = reader.GetDecimal("MaxPrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["MaxPriceDate"].ToString()))
                {
                    stockFiveDaysTop.MaxPriceDate = reader.GetString("MaxPriceDate");
                }

                if (!string.IsNullOrWhiteSpace(reader["MinPriceDate"].ToString()))
                {
                    stockFiveDaysTop.MinPriceDate = reader.GetString("MinPriceDate");
                }

                if (!string.IsNullOrWhiteSpace(reader["MinPrice"].ToString()))
                {
                    stockFiveDaysTop.MinPrice = reader.GetDecimal("MinPrice");
                }
                if (!string.IsNullOrWhiteSpace(reader["OpenPriceYieldRate"].ToString()))
                {
                    stockFiveDaysTop.OpenPriceYieldRate = reader.GetDecimal("OpenPriceYieldRate");
                }
                if (!string.IsNullOrWhiteSpace(reader["ClosePriceYieldRate"].ToString()))
                {
                    stockFiveDaysTop.ClosePriceYieldRate = reader.GetDecimal("ClosePriceYieldRate");
                }
                if (!string.IsNullOrWhiteSpace(reader["MaxYieldRate"].ToString()))
                {
                    stockFiveDaysTop.MaxYieldRate = reader.GetDecimal("MaxYieldRate");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeOpenPrice"].ToString()))
                {
                    stockFiveDaysTop.DayThreeOpenPrice = reader.GetDecimal("DayThreeOpenPrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeClosePrice"].ToString()))
                {
                    stockFiveDaysTop.DayThreeClosePrice = reader.GetDecimal("DayThreeClosePrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeHighPrice"].ToString()))
                {
                    stockFiveDaysTop.DayThreeHighPrice = reader.GetDecimal("DayThreeHighPrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeLowPrice"].ToString()))
                {
                    stockFiveDaysTop.DayThreeLowPrice = reader.GetDecimal("DayThreeLowPrice");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeOPYieldRateOne"].ToString()))
                {
                    stockFiveDaysTop.DayThreeOPYieldRateOne = reader.GetDecimal("DayThreeOPYieldRateOne");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeCPYieldRateOne"].ToString()))
                {
                    stockFiveDaysTop.DayThreeCPYieldRateOne = reader.GetDecimal("DayThreeCPYieldRateOne");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeMYieldRateOne"].ToString()))
                {
                    stockFiveDaysTop.DayThreeMYieldRateOne = reader.GetDecimal("DayThreeMYieldRateOne");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeOPYieldRateTwo"].ToString()))
                {
                    stockFiveDaysTop.DayThreeOPYieldRateTwo = reader.GetDecimal("DayThreeOPYieldRateTwo");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeCPYieldRateTwo"].ToString()))
                {
                    stockFiveDaysTop.DayThreeCPYieldRateTwo = reader.GetDecimal("DayThreeCPYieldRateTwo");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeMYieldRateTwo"].ToString()))
                {
                    stockFiveDaysTop.DayThreeMYieldRateTwo = reader.GetDecimal("DayThreeMYieldRateTwo");
                }

                if (!string.IsNullOrWhiteSpace(reader["DayThreeOPYieldRateThree"].ToString()))
                {
                    stockFiveDaysTop.DayThreeOPYieldRateThree = reader.GetDecimal("DayThreeOPYieldRateThree");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeCPYieldRateThree"].ToString()))
                {
                    stockFiveDaysTop.DayThreeCPYieldRateThree = reader.GetDecimal("DayThreeCPYieldRateThree");
                }
                if (!string.IsNullOrWhiteSpace(reader["DayThreeMYieldRateThree"].ToString()))
                {
                    stockFiveDaysTop.DayThreeMYieldRateThree = reader.GetDecimal("DayThreeMYieldRateThree");
                }
            }

            return stockFiveDaysTop;
        }

        /// <summary>
        /// 查询股票交易日历
        /// </summary>
        /// <param name="conn"></param>
        private static List<TradeCal> QueryAllTradeCal(MySqlConnection conn)
        {
            List<TradeCal> allstocks = new List<TradeCal>();
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                else
                {
                    conn.Close();
                    conn.Open();
                }

                // 创建命令
                string sql = string.Format(@"SELECT * FROM trade_cal");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                // 读取数据
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) // true表示能读取该行数据
                {
                    //// 方式一：访问数组
                    //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                    //                                                                // 方式二：根据列数
                    //Console.WriteLine(reader.GetInt32(0));
                    //// 方式三：根据列名
                    ////Console.WriteLine(reader.GetInt32("user_id"));

                    TradeCal allstock = new TradeCal();
                    allstock.Exchange = reader.GetString("exchange");
                    allstock.CalDate = reader.GetString("cal_date");
                    int isOpen = reader.GetInt32("is_open");
                    allstock.IsOpen = isOpen == 1;

                    allstocks.Add(allstock);
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                return null;
            }


            return allstocks;
        }

        /// <summary>
        /// 查询所有股票
        /// </summary>
        /// <param name="conn"></param>
        private static List<Allstock> QueryAllStock(MySqlConnection conn)
        {
            List<Allstock> allstocks = new List<Allstock>();
            // 创建命令
            string sql = "select * from allstock where `name` not like '%ST%'";
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 读取数据
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) // true表示能读取该行数据
            {
                //// 方式一：访问数组
                //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                //                                                                // 方式二：根据列数
                //Console.WriteLine(reader.GetInt32(0));
                //// 方式三：根据列名
                ////Console.WriteLine(reader.GetInt32("user_id"));

                Allstock allstock = new Allstock();
                allstock.Code = reader[0].ToString();
                allstock.Name = reader[1].ToString();
                allstock.Industry = reader[2].ToString();
                allstock.Area = reader[3].ToString();
                allstock.Market = reader[4].ToString();
                allstock.CreateDatetime = reader[5].ToString();

                allstocks.Add(allstock);
            }

            conn.Close();

            return allstocks;
        }

        /// <summary>
        /// 查询所有股票
        /// </summary>
        /// <param name="conn"></param>
        private static List<Allstock> QueryAllStockByTop100(MySqlConnection conn,string date)
        {
            List<Allstock> allstocks = new List<Allstock>();
            // 创建命令
            string sql = string.Format(@"select * from allstock 
                                        where `name` not like '%ST%' 
                                        AND `code` IN(
		                                        SELECT `Code` FROM(
				                                        SELECT stockfivedaystop.`Code` FROM stockfivedaystop
				                                        INNER JOIN allstock ON allstock.`code` = stockfivedaystop.`Code`
				                                        WHERE TradeDate = '{0}' AND market <> '科创板'
				                                        ORDER BY `Change` DESC
				                                        LIMIT 1, 100
		                                        ) AS a
                                        )", date);

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            else
            {
                conn.Close();
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 读取数据
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) // true表示能读取该行数据
            {
                //// 方式一：访问数组
                //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                //                                                                // 方式二：根据列数
                //Console.WriteLine(reader.GetInt32(0));
                //// 方式三：根据列名
                ////Console.WriteLine(reader.GetInt32("user_id"));

                Allstock allstock = new Allstock();
                allstock.Code = reader[0].ToString();
                allstock.Name = reader[1].ToString();
                allstock.Industry = reader[2].ToString();
                allstock.Area = reader[3].ToString();
                allstock.Market = reader[4].ToString();
                allstock.CreateDatetime = reader[5].ToString();

                allstocks.Add(allstock);
            }

            conn.Close();

            return allstocks;
        }

        /// <summary>
        /// 查询热门行业
        /// </summary>
        /// <param name="conn"></param>
        private static List<HotIndustry> QueryHotIndustry(MySqlConnection conn, string startdate, string enddate)
        {
            List<HotIndustry> hotIndustrList = new List<HotIndustry>();
            string sql = string.Format(@"SELECT industry,SUM(count) sumcount FROM hot_industry
                                        WHERE date BETWEEN '{0}' AND '{1}'
                                        and  count > 1
                                        GROUP BY industry
                                        ORDER BY industry,SUM(count) DESC ", startdate, enddate);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 读取数据
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) // true表示能读取该行数据
            {
                //// 方式一：访问数组
                //Console.WriteLine(reader[0].ToString() + reader[1].ToString()); // reader[0]是读出来的第一列属性
                //                                                                // 方式二：根据列数
                //Console.WriteLine(reader.GetInt32(0));
                //// 方式三：根据列名
                ////Console.WriteLine(reader.GetInt32("user_id"));

                HotIndustry allstock = new HotIndustry();
                allstock.Industry = reader[0].ToString();
                allstock.Count = reader.GetInt32("sumcount");

                hotIndustrList.Add(allstock);
            }

            return hotIndustrList;
        }


        /// <summary>
        /// 插入，增加数据
        /// </summary>
        /// <param name="conn"></param>
        private static void Insert(MySqlConnection conn)
        {
            // 创建命令
            string sql = "insert into user(user_name, user_pwd) values('asdAa','2345')";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 添加一条记录
            int result = cmd.ExecuteNonQuery();
            Console.WriteLine("数据库中受影响的行数 = " + result);
        }


        /// <summary>
        /// 更新，改数据
        /// </summary>
        /// <param name="conn"></param>
        private static void Update(MySqlConnection conn)
        {
            // 创建命令
            string sql = "update user set user_name = 'newName', user_pwd = '66777' where user_id = '3'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 更新记录
            int result = cmd.ExecuteNonQuery();
            Console.WriteLine("数据库中受影响的行数 = " + result);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="conn"></param>
        private static void Delete(MySqlConnection conn)
        {
            // 创建命令
            string sql = "delete from user where user_id = '4'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            // 删除记录
            int result = cmd.ExecuteNonQuery();
            Console.WriteLine("数据库中受影响的行数 = " + result);
        }
    }
}
