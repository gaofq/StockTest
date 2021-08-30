using MySql.Data.MySqlClient;
using System;
using System.Text;

namespace StockTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // 服务器地址；端口号；数据库；用户名；密码
            string connectStr = "server=127.0.0.1;port=3306;database=test;user=root;password=n!ElGgNs.3OT"; // 用户名和密码在MySQL定义的
            // 创建连接
            MySqlConnection conn = new MySqlConnection(connectStr);

            try
            {
                // 打开连接
                conn.Open();
                Console.WriteLine("已经建立连接");

                List<Allstock>  allstocks = QueryAllStock(conn);  //查询所有股票

                ageList.Clear();

                //DateTime countdate = new DateTime(2021, 8, 5);//计算时间
                //RunStock(conn, allstocks, countdate);

                for (int i = 1; i < 31; i++)
                {
                    DateTime countdate = new DateTime(2021, 8, i);//计算时间

                    if (countdate.DayOfWeek == System.DayOfWeek.Saturday || countdate.DayOfWeek == System.DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    RunStock(conn, allstocks, countdate);
                }

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

            Console.ReadKey();
        }

        static List<decimal> ageList = new List<decimal>();
        private static void RunStock(MySqlConnection conn, List<Allstock> allstocks, DateTime countdate)
        {
            int count = 0;

            int countDays = 7;//计算天数
            
            string enddate = countdate.ToString("yyyyMMdd");//结束时间

            int days1 = DiffWeekend1(countdate.AddDays(-countDays), countdate);
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
            //if (nextDayTradeList != null && nextDayTradeList.Count > 0
            //    && countStocks != null && countStocks.Count > 0)
            //{
            //    foreach (var item in nextDayTradeList)
            //    {
            //        if (countStocks.Exists(o => o.Code == item.Code))
            //        {
            //            count1++;
            //            var stockItem = allstocks.FirstOrDefault(o => o.Code == item.Code);
            //            Console.WriteLine(string.Format("{0}已匹配：{2}\r\n最近交易：{1}", count1, item.ToString(), stockItem.ToString()));
            //            var showLastList = lastTradeList.Where(o => o.Code == item.Code).OrderByDescending(o => o.Date).ToList();
            //            foreach (var item1 in showLastList)
            //            {
            //                Console.WriteLine(string.Format("最近交易：{0}", item1.ToString()));
            //            }

            //        }
            //    }
            //}

            List<StockHis> countStocks = new List<StockHis>();

            List<StockHis> lastTradeList = new List<StockHis>();

            foreach (var item in allstocks)
            {
                //if (!hotIndustrList.Exists(o => o.Industry == item.Industry))
                //{
                //    continue;
                //}

                conn.Close();
                conn.Open();

                var tradeList = QueryAllStock(conn, item.Code, startdate, enddate);

                if (tradeList != null)//&& tradeList.Count == countDays
                {
                    var lastItem = tradeList.FirstOrDefault(x => x.Date == enddate);
                    tradeList.Remove(lastItem);

                    //bool isMatch = TacticsUtils.Tactics1(tradeList, countDays);//匹配策略
                    bool isMatch = TacticsUtils.Tactics5(lastItem);//匹配策略

                    //&& lastItem.V_ma5 >= 9000//5日均量大于9000万
                    
                    if (lastItem != null && isMatch)
                    {
                        //lastTradeList.Add(lastItem);
                        //lastTradeList.AddRange(tradeList);
                        count++;
                        //Console.WriteLine(string.Format("{0}符合条件：{1}\r\n{2}", count, item.ToString(), lastItem.ToString()));
                        //foreach (var item1 in tradeList)
                        //{
                        //    Console.WriteLine(string.Format("{0}", item1.ToString()));
                        //}
                        countStocks.Add(lastItem);

                        if (nextDayTradeList.Exists(o => o.Code == item.Code))
                        {
                            count1++;
                            //var item1 = nextDayTradeList.FirstOrDefault(o => o.Code == item.Code);
                            //var stockItem = allstocks.FirstOrDefault(o => o.Code == item.Code);
                            //Console.WriteLine(string.Format("{0}已匹配：{1}", count1, item1.ToString()));
                            //var showLastList = lastTradeList.Where(o => o.Code == item.Code).OrderByDescending(o => o.Date).ToList();
                            //foreach (var item1 in showLastList)
                            //{
                            //    Console.WriteLine(string.Format("最近交易：{0}", item1.ToString()));
                            //}

                        }
                    }
                }
            }
            

            if (count > 0)
            {
                ageList.Add(Convert.ToDecimal(((decimal)count1 / (decimal)count * 100).ToString("f2")));
                Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, ((decimal)count1 / (decimal)count * 100).ToString("f2"), enddate1, (ageList.Sum() / ageList.Count).ToString("f2")));
            }
            else
            {
                //ageList.Add(0);
                Console.WriteLine(string.Format("交易时间:{3},找到数量：{0}，匹配数量：{1}，百分比：{2}，平均百分比：{4}", count, count1, 0, enddate1,0));
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
        /// 查询股票交易信息
        /// </summary>
        /// <param name="conn"></param>
        private static List<StockHis> QueryNextDayList(MySqlConnection conn, string enddate)
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
        private static List<StockHis> QueryAllStock(MySqlConnection conn,string code, string startdate,string enddate)
        {
            List<StockHis> allstocks = new List<StockHis>();
            //string startdate = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            //string enddate = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");

            try
            {
                // 创建命令
                string sql = string.Format(@"SELECT stock_{0}.*,stock_dailyhis.volume_ratio,float_share FROM stock_{0}
                                            LEFT JOIN stock_dailyhis ON stock_dailyhis.trade_date = stock_{0}.date
                                            AND stock_dailyhis.ts_code = '{0}' AND stock_dailyhis.trade_date = date
                                            WHERE date BETWEEN '{1}' AND '{2}'", code, startdate, enddate);
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
                    allstock.V_ma5 = reader.GetDecimal("v_ma5");
                    allstock.V_ma10 = reader.GetDecimal("v_ma10");
                    allstock.V_ma20 = reader.GetDecimal("v_ma20");
                    allstock.Volume_ratio = reader.GetDecimal("Volume_ratio");
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
        /// 查询所有股票
        /// </summary>
        /// <param name="conn"></param>
        private static List<Allstock> QueryAllStock(MySqlConnection conn)
        {
            List<Allstock> allstocks = new List<Allstock>();
            // 创建命令
            string sql = "select * from allstock where `name` not like '%ST%'";
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
