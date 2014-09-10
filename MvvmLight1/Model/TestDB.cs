using DataModels;
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

        
        private TestDBDB mTestDBDB;
        public TestDBDB TestDB
        {
            get {
                return mTestDBDB;
            }
            private set
            {
                mTestDBDB = value;
            }
        }

        public TestDBWrap()
        {
            mTestDBDB = new TestDBDB();
            TestDB = mTestDBDB;

            string conn = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;
            mNpgsqlConnection = new NpgsqlConnection(conn);
            mNpgsqlConnection.Open();
            var insert = new NpgsqlDataAdapter(@"select * from users", mNpgsqlConnection);
            var dataset = new DataSet();
            insert.Fill(dataset);
            System.Windows.MessageBox.Show(dataset.Tables[0].Rows[0]["id"].ToString());
        }

        public IQueryable select() {

            return mTestDBDB.users.Select((user) => true);
        }
    }
}
