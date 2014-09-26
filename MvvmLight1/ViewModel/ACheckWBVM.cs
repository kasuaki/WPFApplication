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
    public abstract class ACheckWBVM : MyViewModelBase
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
        protected abstract void LoadCompletedEvent(WebBrowser sender);

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public ACheckWBVM(Uri aUri, CommonVM aCommonVM, Int32 aColumn, Int32 aRow)
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
        public abstract void PageUpdate();

        /// <summary>
        /// 監視処理.
        /// </summary>
        public abstract void WatchStart();

        /// <summary>
        /// Timer満了時の動作.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void MyDispatcherTimer_Tick(Object sender, EventArgs e)
        {
            PageUpdate();
        }

        public virtual void Dispose()
        {
            WB.Dispose();
        }
    }
}