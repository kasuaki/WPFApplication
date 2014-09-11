using GalaSoft.MvvmLight;
using System;
using System.Windows.Controls;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class WebBrowserVM : ViewModelBase
    {

        public WebBrowser WB { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public WebBrowserVM(Uri aUri)
        {
            WB = new WebBrowser();
            WB.Navigate(aUri);
        }
    }
}