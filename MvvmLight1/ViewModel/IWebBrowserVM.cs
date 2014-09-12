using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MvvmLight1.ViewModel
{
    interface IWebBrowserVM
    {
        DispatcherTimer MyDispatcherTimer { get; }
        Uri MyUri { get; set; }
        WebBrowser WB { get; }
        Status MyStatus { get; set; }



        void WebBrowserAdd(Panel aPanel);
        void WebBrowserRemove(Panel aPanel);
        void TimerStart();
        void TimerStop();
        void WatchStart();
        void PageUpdate();
    }
}
