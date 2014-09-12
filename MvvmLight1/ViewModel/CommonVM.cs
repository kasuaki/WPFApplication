using GalaSoft.MvvmLight;
using MvvmLight1.Model;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace MvvmLight1.ViewModel
{
    public enum Status
    {
        Watching,
        HaveShares,
        NowBuying,
        NowSelling,
    }
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CommonVM : MyViewModelBase
    {
        public TaskScheduler UI { get; set; }
        public TestDBWrap DB { get; set; }
        public Dispatcher Dispatcher { get; set; }

        // 判断イベント.
        public event EventHandler<EventArgs> JudgeShare;
        public virtual void OnJudgeShare(EventArgs e)
        {
            EventHandler<EventArgs> h = JudgeShare;
            if (h != null)
            {
                h(this, e);
            }
        }

        // 買イベント.
        public event EventHandler<EventArgs> BuyShare;
        public virtual void OnBuyShare(EventArgs e)
        {
            EventHandler<EventArgs> h = BuyShare;
            if (h != null)
            {
                h(this, e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommonVM class.
        /// </summary>
        public CommonVM(Dispatcher aDispatcher)
        {
            CVM = this;
            DB = new TestDBWrap();
            Dispatcher = aDispatcher;
            UI = TaskScheduler.FromCurrentSynchronizationContext();
            //using(var db = new TestDBDB()) {

            //    db.users.ForEachAsync((users) =>
            //    {
            //        System.Windows.MessageBox.Show(users.id.ToString());
            //        System.Windows.MessageBox.Show(users.code.ToString());
            //    });
            //}
        }

        // Win32APIの GetPrivateProfileString を使う宣言
        [DllImport("KERNEL32.DLL")]
        public static extern uint
          GetPrivateProfileString(string lpAppName,
          string lpKeyName, string lpDefault,
          StringBuilder lpReturnedString, uint nSize,
          string lpFileName);

        public String GetIniData(String aFileName, String aSection, String aKey)
        {
            // iniファイル名を決める（実行ファイルが置かれたフォルダと同じ場所）
            string iniFileName = AppDomain.CurrentDomain.BaseDirectory + aFileName;

            // iniファイルから文字列を取得
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(
                aSection,      // セクション名
                aKey,          // キー名    
                "ありません",   // 値が取得できなかった場合に返される初期値
                sb,             // 格納先
                Convert.ToUInt32(sb.Capacity), // 格納先のキャパ
                iniFileName);   // iniファイル名

            return sb.ToString();
        }
    }
}