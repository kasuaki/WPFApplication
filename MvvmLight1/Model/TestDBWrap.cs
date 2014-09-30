using CsQuery;
using DataModels;
using mshtml;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Linq;
using System.Net;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MvvmLight1.Model
{
    public class TestDBWrap : IDisposable
    {
//        private NpgsqlConnection mNpgsqlConnection;

        private TestDBDB _TestDB;
        public TestDBDB TestDB
        {
            get
            {
                if (_TestDB == null)
                {
                    _TestDB = new TestDBDB();
                }
                return _TestDB;
            }
        }

        public TestDBWrap()
        {
            using (TestDB)
            {
                lock (TestDB)
                {
                    TestDB.portfolios.Delete();
                }
            }
            //string conn = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;
            //mNpgsqlConnection = new NpgsqlConnection(conn);
            //mNpgsqlConnection.Open();

            //var insert = new NpgsqlDataAdapter(@"select * from users", mNpgsqlConnection);
            //var dataset = new DataSet();
            //insert.Fill(dataset);
            //System.Windows.MessageBox.Show(dataset.Tables[0].Rows[0]["id"].ToString());
        }

        /// <summary>
        /// 所有株解析.
        /// </summary>
        /// <param name="aElement"></param>
        public void AnalysisShare(IHTMLElement aElement)
        {
            using (TestDB)
            {
                lock (TestDB)
                {
                    TestDB.shares.Delete();
                }
            }

            CQ table = new CQ(aElement.innerHTML);

            table["tr"][0].Remove();    // 株式（現物/特定預り）.
            table["tr"][0].Remove();    // 保有株数.
            CQ trList = table["tr"];

            // 行全体.
            var index = 0;
            while (index < trList.Count())
            {
                share share = new share();

                // 上段.
                IDomObject trDom = trList[index];

                // 列個別.
                CQ tr = new CQ(trDom.InnerHTML);
                CQ tdList = tr["td"];
                tdList.Each((int i, IDomObject ele) =>
                {
                    // 文字列の取得と整形.
                    String str = SanitizeString(ele);

                    int? k = i;
                    // 格納.
                    switch (k)
                    {
                        case 0:
                            String codeStr = str.Substring(0, 4);
                            share.銘柄コード = Int32.Parse(codeStr);
                            String NameStr = str.Substring(4);
                            NameStr = NameStr.Trim();
                            share.銘柄 = NameStr;
                            break;
                        case 1:
                            CQ tdCQ = new CQ(ele.InnerHTML);
                            CQ aList = tdCQ["a"];
                            aList.Each((Int32 JetBrains, IDomObject e) =>
                            {
                                String s = SanitizeString(e);
                                if ("現売".Equals(s))
                                {
                                    share.現売リンク = @"https://site2.sbisec.co.jp" + e.GetAttribute("href");
                                }
                                else if ("現買".Equals(s))
                                {
                                    share.現買リンク = @"https://site2.sbisec.co.jp" + e.GetAttribute("href");
                                }
                            });
                            
                            break;
                        default:
                            break;
                    }
                });

                index++;

                // 下段.
                trDom = trList[index];

                // 列個別.
                tr = new CQ(trDom.InnerHTML);
                tdList = tr["td"];
                tdList.Each((int i, IDomObject ele) =>
                {
                    // 文字列の取得と整形.
                    String str = SanitizeString(ele);

                    int? k = i;
                    int tmpInteger;
                    // 格納.
                    switch (k)
                    {
                        case 0:
                            if (Int32.TryParse(str, out tmpInteger)) { share.保有株数 = tmpInteger; }
                            break;
                        case 1:
                            if (Int32.TryParse(str, out tmpInteger)) { share.取得単価 = tmpInteger; }
                            break;
                        case 2:
                            if (Int32.TryParse(str, out tmpInteger)) { share.現在値 = tmpInteger; }
                            break;
                        case 3:
                            if (Int32.TryParse(str, out tmpInteger)) { share.評価損益 = tmpInteger; }
                            break;
                        default:
                            break;
                    }
                });

                index++;

                share.更新日時 = DateTime.Now;

                using (TestDB)
                {
                    lock (TestDB)
                    {
                        // 保存.
                        TestDB.Insert(share);
                    }
                }
            }
        }

        /// <summary>
        /// ポートフォリオ解析.
        /// </summary>
        /// <param name="aElement"></param>
        public void AnalysisPortfolio(IHTMLElement aElement)
        {
            using (TestDB)
            {

                CQ table = new CQ(aElement.innerHTML);

                table["tr"][0].Remove();
                CQ trList = table["tr"];

                // 行全体.
                trList.Each((IDomObject element) =>
                {
                    portfolio pf = new portfolio();

                    // 列個別.
                    CQ tr = new CQ(element.InnerHTML);
                    CQ tdList = tr["td"];
                    tdList.Each((int i, IDomObject ele) =>
                    {
                        // 文字列の取得と整形.
                        String str = SanitizeString(ele);

                        int? k = i;
                        String[] strArr = null;
                        int tmpInteger;
                        // 格納.
                        switch (k)
                        {
                            case 1:
                                String codeStr = str.Substring(0, 4);
                                if (Int32.TryParse(codeStr, out tmpInteger)) { pf.銘柄コード = tmpInteger; }
                                String NameStr = str.Substring(4);
                                NameStr = NameStr.Trim();
                                pf.銘柄 = NameStr;
                                break;
                            case 2:
                                strArr = str.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                                Int32 aYear, aMonth, aDay;
                                if ((Int32.TryParse("20" + strArr[0], out aYear)) &&
                                    (Int32.TryParse(strArr[1], out aMonth)) &&
                                    (Int32.TryParse(strArr[2], out aDay)))
                                {
                                    pf.買付日 = new DateTime(aYear, aMonth, aDay);
                                }
                                break;
                            case 3:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.数量 = tmpInteger; }
                                break;
                            case 4:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.参考単価 = tmpInteger; }
                                break;
                            case 5:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.現在値 = tmpInteger; }
                                break;
                            case 6:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.前日比 = tmpInteger; }
                                break;
                            case 7:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.損益 = tmpInteger; }
                                break;
                            case 8:
                                float tmpFloat;
                                if (float.TryParse(str, out tmpFloat)) { pf.損益パーセント = tmpFloat; }
                                break;
                            case 9:
                                if (Int32.TryParse(str, out tmpInteger)) { pf.評価額 = tmpInteger; }
                                break;
                            case 0:
                                CQ anchorCQ = new CQ(ele.InnerHTML);
                                CQ anchorList = anchorCQ["a"];
                                KeyValuePair<String, String> anchor;
                                anchor = anchorList[0].Attributes.First((v) => Regex.IsMatch(v.Key, @"href", RegexOptions.IgnoreCase));
                                pf.現買リンク = @"https://site2.sbisec.co.jp" + anchor.Value;
                                anchor = anchorList[1].Attributes.First((v) => Regex.IsMatch(v.Key, @"href", RegexOptions.IgnoreCase));
                                pf.現売リンク = @"https://site2.sbisec.co.jp" + anchor.Value;
                                break;
                            case 10:
                            default:
                                break;
                        }
                    });
                    pf.更新日時 = DateTime.Now;

                    lock (TestDB)
                    {
                        // 保存.
                        TestDB.Insert(pf);
                    }
                });
            }
        }

        /// <summary>
        /// 特定の株式画面解析.
        /// </summary>
        /// <param name="aElement"></param>
        public void AnalysisSpecificShare(IHTMLElement aElement, Int32 Code)
        {
            CQ table = new CQ(aElement.innerHTML);
            CQ spanCQ = table["#MTB0_0 span[class=fxx01]"];

            portfolio pf = new portfolio();
            pf.銘柄コード = Code;
            Int32 tmpIndex;
            if (Int32.TryParse(spanCQ[0].InnerText, out tmpIndex))
            {
                pf.現在値 = tmpIndex;

                pf.更新日時 = DateTime.Now;

                using (TestDB)
                {
                    lock (TestDB)
                    {
                        // 保存.
                        TestDB.Insert(pf);
                    }
                }
            }
        }

        /// <summary>
        /// 購入是非判断.
        /// </summary>
        public  Tuple<Boolean, Int32> JudgeBuy()
        {
            Boolean aCanBuySell = false;
            Int32 code = 0;
            Int32 checkCount = 6;   // 何回前までさかのぼってチェックするか.

            using (TestDB)
            {
                List<int?> shareList = TestDB.shares.OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                // ユニークな銘柄コレクションを取得.
                List<int?> codeList = TestDB.portfolios
                    .Where(p => shareList.Contains(p.銘柄コード) == false)    // 既に購入している株は対象にしない.
                    .OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                foreach(int? tmpcode in codeList)
                {
                    code = tmpcode ?? 0;

                    IEnumerable<portfolio> latest = TestDB.portfolios.Where(p => tmpcode.Equals(p.銘柄コード))
                                                  .OrderByDescending((portfolio p) => p.更新日時)
                                                  .Take(checkCount).Select(p => p).ToList();
                    latest = latest.Reverse();

                    Int32 first現在値 = latest.First().現在値 ?? 0;
                    Int32 last現在値 = latest.Last().現在値 ?? 0;
                    Int32 差分 = last現在値 - first現在値;
                    if (差分 > 0)
                    {
                        portfolio beforeP = null;
                        foreach (portfolio p in latest)
                        {
                            if (beforeP == null)
                            {
                                beforeP = p;
                                continue;
                            }

                            if (beforeP.現在値 <= p.現在値)
                            {
                                aCanBuySell = true;
                            }
                            else
                            {
                                aCanBuySell = false;
                                break;
                            }

                            beforeP = p;
                        }
                    }

                    if (aCanBuySell)
                    {
                        break;
                    }
                }
            }

            /* ※※※※※※※※※※※※※※※※※ */
//            aCanBuySell = true;
            /* ※※※※※※※※※※※※※※※※※ */
            Tuple<Boolean, Int32> returnObj = new Tuple<bool, int>(aCanBuySell, code);
            return returnObj;
        }

        /// <summary>
        /// 売却是非判断.
        /// </summary>
        public  Tuple<Boolean, Int32> JudgeSell()
        {
            Boolean aCanBuySell = false;
            Int32 code = 0;

            using (TestDB)
            {
                // ユニークな持株コレクションを取得.
                List<int?> shareList = TestDB.shares.OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                foreach (int? tmpcode in shareList)
                {
                    code = (Int32)tmpcode;
                    IEnumerable<portfolio> latest = TestDB.portfolios.Where(p => tmpcode.Equals(p.銘柄コード))
                                                  .OrderByDescending((portfolio p) => p.更新日時)
                                                  .Take(3).Select(p => p).ToList();
                    latest = latest.Reverse();

                    portfolio beforeP = null;
                    foreach (portfolio p in latest)
                    {
                        if (beforeP == null)
                        {
                            beforeP = p;
                            continue;
                        }

                        if (beforeP.現在値 > p.現在値)
                        {
                            aCanBuySell = true;
                        }
                        else
                        {
                            aCanBuySell = false;
                            break;
                        }

                        beforeP = p;
                    }

                    if (aCanBuySell)
                    {
                        break;
                    }
                }
            }

            /* ※※※※※※※※※※※※※※※※※ */
//            aCanBuySell = true;
            /* ※※※※※※※※※※※※※※※※※ */
            Tuple<Boolean, Int32> returnObj = new Tuple<bool, int>(aCanBuySell, code);
            return returnObj;
        }

        /// <summary>
        /// DOM(td)から、tdとその子要素のinnerTestを取得してデコード.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public String SanitizeString(IDomObject ele)
        {
            // tdの文字列.
            String str = ele.InnerText;

            // 子要素の文字列.
            CQ tdCQ = new CQ(ele);
            CQ tdAll = new CQ("*", tdCQ);
            tdAll.Each((int index, IDomObject e) =>
            {
                if (tdAll[index].InnerText is String)
                {
                    str += tdAll[index].InnerText;
                }
            });

            // デコード.
            str = WebUtility.HtmlDecode(str);

            // 整形.
            str = str.Replace(",", "");
            //                        System.Windows.MessageBox.Show(str);

            return str;
        }

        public void Dispose()
        {
            if (_TestDB != null)
            {
                _TestDB.Dispose();
            }
        }    
    }
}
