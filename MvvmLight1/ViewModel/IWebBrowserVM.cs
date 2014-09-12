using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MvvmLight1.ViewModel
{
    interface IWebBrowserVM
    {
        Uri MyUri { get; set; }
        MyWebBrowser Grid { get; set; }
        WebBrowser WB { get; }

    }
}
