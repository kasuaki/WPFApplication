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

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class BuySellShareBaseVM : MyViewModelBase, IDisposable
    {
        public Panel MyPanel { get; set; }
        public Uri MyUri { get; set; }
        public MyWebBrowser MyGrid { get; set; }
        public WebBrowser WB { get; set; }
        public Status MyStatus { get; set; }
        public Int32 Code { get; set; }
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
        protected virtual void LoadCompletedEvent(WebBrowser sender)
        {
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public BuySellShareBaseVM(Uri aUri, CommonVM aCommonVM, Int32 aColumn, Int32 aRow, Int32 aCode)
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
            Code = aCode;

            User = CVM.GetIniData("login.ini", "LOGIN", "USER");
            Pass = CVM.GetIniData("login.ini", "LOGIN", "PASS");
        }

        public void WebBrowserAdd(Panel aPanel)
        {
            aPanel.Children.Add(MyGrid);
            MyPanel = aPanel;
        }

        public void WebBrowserRemove()
        {
            MyPanel.Children.Remove(MyGrid);
        }
    
        public void Dispose()
        {
            WB.Dispose();
        }
    }
}