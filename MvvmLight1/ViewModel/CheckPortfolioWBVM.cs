using GalaSoft.MvvmLight;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using mshtml;
using CsQuery;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Net;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CheckPortfolioWBVM : MyViewModelBase, IWebBrowserVM
    {
        public DispatcherTimer _MyDispatcherTimer;
        public DispatcherTimer MyDispatcherTimer
        {
            get
            {
                return _MyDispatcherTimer;
            }
        }
        public Uri MyUri { get; set; }
        public MyWebBrowser MyGrid { get; set; }
        public WebBrowser WB { get; set; }
        public Status MyStatus { get; set; }
        public String User;
        public String Pass;
        public String Portfolio { get; set; }

        protected RelayCommand<WebBrowser> _LoadCompletedCommand;
        public RelayCommand<WebBrowser> LoadCompletedCommand
        {
            get
            {
                return _LoadCompletedCommand;
            }
            protected set
            {
                _LoadCompletedCommand = value;
                RaisePropertyChanged("LoadCompletedCommand");
            }
        }
        protected virtual void LoadCompletedEvent(WebBrowser sender)
        {
            WatchStart();
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public CheckPortfolioWBVM(Uri aUri, CommonVM aCommonVM, Int32 aColumn, Int32 aRow, String aPortfolio)
            : base(aCommonVM)
        {
            MyGrid = new MyWebBrowser();
            MyGrid.DataContext = this;
            Grid.SetRow(MyGrid, aRow);
            Grid.SetColumn(MyGrid, aColumn);
            Grid.SetColumnSpan(MyGrid, 3);

            MyUri = aUri;
            WB = this.MyGrid.Children.OfType<WebBrowser>().First();
            MyStatus = Status.Watching;
            Portfolio = aPortfolio;

            _MyDispatcherTimer = new DispatcherTimer(new TimeSpan(60 * TimeSpan.TicksPerSecond),
                                                     DispatcherPriority.Normal,
                                                     MyDispatcherTimer_Tick,
                                                     Dispatcher.CurrentDispatcher) { IsEnabled = false };

            LoadCompletedCommand = new RelayCommand<WebBrowser>(LoadCompletedEvent);

            User = CVM.GetIniData("login.ini", "LOGIN", "USER");
            Pass = CVM.GetIniData("login.ini", "LOGIN", "PASS");
        }

        public void WebBrowserAdd(Panel aPanel)
        {
            aPanel.Children.Add(MyGrid);
        }

        public void WebBrowserRemove(Panel aPanel)
        {
            aPanel.Children.Remove(MyGrid);
        }

        public void TimerStart()
        {
            MyDispatcherTimer.IsEnabled = true;
            MyDispatcherTimer.Start();
        }

        public void TimerStop()
        {
            MyDispatcherTimer.Stop();
            MyDispatcherTimer.IsEnabled = false;
        }

        /// <summary>
        /// ページ更新処理.
        /// </summary>
        public virtual void PageUpdate()
        {
            if (WB.Document != null)
            {
                HTMLDocument aHTMLDocument = WB.Document as HTMLDocument;
                var bList = aHTMLDocument.getElementsByTagName("b").Cast<IHTMLElement>();
                // ポートフォリオ画面(想定).
                if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"ポートフォリオ名")) != 0)
                {
                    var inputList = aHTMLDocument.getElementsByTagName("input").Cast<IHTMLElement>();
                    IHTMLElement button = inputList.First(element => (("button".Equals(element.getAttribute("type"))) &&
                                                                      ("情報更新".Equals(element.getAttribute("value")))
                                                                     )
                    );
                    button.click();
                }
                else
                {
                    WatchStart();
                }
            }
            else
            {
                WatchStart();
            }
        }

        /// <summary>
        /// 監視処理.
        /// </summary>
        public virtual void WatchStart()
        {
            HTMLDocument aHTMLDocument = WB.Document as HTMLDocument;

            if (aHTMLDocument == null)
            {
                WB.Navigate(MyUri);
                return;
            }

            var pList = aHTMLDocument.getElementsByTagName("p").Cast<IHTMLElement>();
            var bList = aHTMLDocument.getElementsByTagName("b").Cast<IHTMLElement>();
            var inputList = aHTMLDocument.getElementsByTagName("input").Cast<IHTMLElement>();

            // ログイン前ポータル画面(想定).
            if (inputList.Where(element => element.title != null).Count(element => Regex.IsMatch(element.title, @"ログイン")) != 0)
            {
                IHTMLElement user = inputList.First(element => "user_id".Equals(element.getAttribute("name")));
                IHTMLElement pass = inputList.First(element => "user_password".Equals(element.getAttribute("name")));
                user.setAttribute("value", User);
                pass.setAttribute("value", Pass);
                IHTMLElement button = inputList.First((element) => "ログイン".Equals(element.title));
                button.click();
            }
            // ログイン後ポータル画面(想定).
            else if (pList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"最終ログイン.*")) != 0)
            {
                var imgList = aHTMLDocument.getElementsByTagName("image").Cast<IHTMLElement>();
                IHTMLElement portfolio = imgList.FirstOrDefault(element => "ポートフォリオ".Equals(element.title));
                if (portfolio != null)
                {
                    IHTMLElement a = portfolio.parentElement;
                    a.click();
                }
            }
            // ポートフォリオ画面(想定).
            else if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"ポートフォリオ名")) != 0)
            {
                var optionList = aHTMLDocument.getElementsByTagName("option").Cast<IHTMLElement>();

                var value = optionList.Where(e => e.innerText != null).Where(e =>
                {
                    String str = e.innerText;
                    str = WebUtility.HtmlDecode(str);

                    return Portfolio.Equals(str);
                }).Select(e => e.getAttribute("value")).First();

                var selectList = aHTMLDocument.getElementsByTagName("select").Cast<IHTMLElement>();
                var select = selectList.Where(e => e.getAttribute("name") != null).First(e => "portforio_id".Equals(e.getAttribute("name")));

                var currentSelect = select.getAttribute("value");
                if (currentSelect != value)
                {
                    select.setAttribute("value", value);

                    var viewPfButton = inputList.Where(e => e.getAttribute("name") != null).First(e => "ACT_viewPf".Equals(e.getAttribute("name")));
                    viewPfButton.click();
                }
                else
                {

                    // テーブル取得.
                    var tdList = aHTMLDocument.getElementsByTagName("td").Cast<IHTMLElement>();
                    IHTMLElement tanka = tdList.First(element => ((element.innerText != null) && (element.innerText.Equals(@"参考単価"))));
                    IHTMLElement table = tanka.parentElement.parentElement.parentElement;

                    // 解析.
                    CVM.DB.AnalysisPortfolio(table);
                }
            }
        }

        /// <summary>
        /// Timer満了時の動作.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void MyDispatcherTimer_Tick(Object sender, EventArgs e)
        {
            PageUpdate();
        }
    }
}