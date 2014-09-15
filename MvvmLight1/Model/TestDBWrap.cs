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

namespace MvvmLight1.Model
{
    public class TestDBWrap
    {
        private NpgsqlConnection mNpgsqlConnection;

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
                TestDB.shares.Delete();
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
                    // 格納.
                    switch (k)
                    {
                        case 0:
                            share.保有株数 = Int32.Parse(str);
                            break;
                        case 1:
                            share.取得単価 = Int32.Parse(str);
                            break;
                        case 2:
                            share.現在値 = Int32.Parse(str);
                            break;
                        case 3:
                            share.評価損益 = Int32.Parse(str);
                            break;
                        default:
                            break;
                    }
                });

                index++;

                share.更新日時 = DateTime.Now;

                using (TestDB)
                {
                    // 保存.
                    TestDB.Insert(share);
                }
            }
        }

        /// <summary>
        /// ポートフォリオ解析.
        /// </summary>
        public void AnalysisPortfolio(IHTMLElement aElement)
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
                    // 格納.
                    switch (k)
                    {
                        case 1:
                            String codeStr = str.Substring(0, 4);
                            pf.銘柄コード = Int32.Parse(codeStr);
                            String NameStr = str.Substring(4);
                            NameStr = NameStr.Trim();
                            pf.銘柄 = NameStr;
                            break;
                        case 2:
                            strArr = str.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                            pf.買付日 = new DateTime(Int32.Parse("20" + strArr[0]), Int32.Parse(strArr[1]), Int32.Parse(strArr[2]));
                            break;
                        case 3:
                            pf.数量 = Int32.Parse(str);
                            break;
                        case 4:
                            pf.参考単価 = Int32.Parse(str);
                            break;
                        case 5:
                            pf.現在値 = Int32.Parse(str);
                            break;
                        case 6:
                            pf.前日比 = Int32.Parse(str);
                            break;
                        case 7:
                            pf.損益 = Int32.Parse(str);
                            break;
                        case 8:
                            pf.損益パーセント = float.Parse(str);
                            break;
                        case 9:
                            pf.評価額 = Int32.Parse(str);
                            break;
                        case 0:
                        case 10:
                        default:
                            break;
                    }
                });
                pf.更新日時 = DateTime.Now;

                using (TestDB)
                {
                    // 保存.
                    TestDB.Insert(pf);
                }
            });

        }

        /// <summary>
        /// 購入是非判断.
        /// </summary>
        public async Task<Tuple<Boolean, Int32>> JudgeBuy()
        {
            Boolean aCanBuySell = true;
            Int32 code = 0;

            using (TestDB)
            {
                // ユニークな銘柄コレクションを取得.
                List<int?> codeList = TestDB.portfolios.OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                foreach(int? tmpcode in codeList)
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

                        if (beforeP.現在値 < p.現在値)
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

            Tuple<Boolean, Int32> returnObj = new Tuple<bool, int>(aCanBuySell, code);
            return returnObj;
        }

        /// <summary>
        /// 売却是非判断.
        /// </summary>
        public async Task<Tuple<Boolean, Int32>> JudgeSell()
        {
            Boolean aCanBuySell = true;
            Int32 code = 0;

            using (TestDB)
            {
                // ユニークな銘柄コレクションを取得.
                List<int?> shareList = TestDB.shares.OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                // ユニークな銘柄コレクションを取得.
                List<int?> codeList = TestDB.portfolios.OrderBy(p => p.銘柄コード).Select(p => p.銘柄コード).Distinct().ToList();

                foreach (int? tmpcode in codeList)
                {
                    if (!(shareList.Contains(tmpcode)))
                    {
                        continue;
                    }

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

            Tuple<Boolean, Int32> returnObj = new Tuple<bool, int>(aCanBuySell, code);
            return returnObj;
        }

        /// <summary>
        /// DOM(td)から、tdとその子要素のinnerTestを取得してデコード.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        protected String SanitizeString(IDomObject ele)
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
    }
}
