﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MvvmLight1.ViewModel
{
    public interface IBuyShareVM : IDisposable
    {
        Panel MyPanel { get; set; }
        Uri MyUri { get; set; }
        WebBrowser WB { get; }
        Status MyStatus { get; set; }
        Int32 Code { get; set; }



        void WebBrowserAdd(Panel aPanel);
        void WebBrowserRemove();
        void BuyShare();
        void Dispose();
    }
}