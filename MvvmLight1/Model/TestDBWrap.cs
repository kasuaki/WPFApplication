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

namespace MvvmLight1.Model
{
    public class TestDBWrap
    {
        private NpgsqlConnection mNpgsqlConnection;

        
        public TestDBDB TestDB { get; set; }

        public TestDBWrap()
        {
            TestDB = new TestDBDB();

            string conn = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;
            mNpgsqlConnection = new NpgsqlConnection(conn);
            mNpgsqlConnection.Open();

            //var insert = new NpgsqlDataAdapter(@"select * from users", mNpgsqlConnection);
            //var dataset = new DataSet();
            //insert.Fill(dataset);
            //System.Windows.MessageBox.Show(dataset.Tables[0].Rows[0]["id"].ToString());
        }

        /// <summary>
        /// ポートフォリオ解析.
        /// </summary>
        public void AnalysisPortfolio(IHTMLElement aElement)
        {
            CQ dom = new CQ(aElement.innerHTML);
            var aList = dom["a"];
            dom["tr"][0].Remove();
            var trList = dom["tr"];
            aList.Each((element) => {
                System.Windows.MessageBox.Show(element.ToString());
            });
            System.Windows.MessageBox.Show(trList.ToString());
        }
    }
}
