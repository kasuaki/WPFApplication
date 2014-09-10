using System.Windows;
using MvvmLight1.ViewModel;
using System;
using System.Windows.Controls;
using mshtml;

namespace MvvmLight1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            MainViewModel aMainVM = this.DataContext as MainViewModel;
            aMainVM.mWebBrowser = this.WB;
            aMainVM.mWebBrowser.Navigate(new Uri(@"http://www.google.com"));
            aMainVM.mWebBrowser.LoadCompleted += mWebBrowser_LoadCompleted;
        }

        void mWebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            System.Windows.MessageBox.Show("mWebBrowser_LoadCompleted");
            WebBrowser aWB = sender as WebBrowser;
            HTMLDocument aHTMLDocument = aWB.Document as HTMLDocument;
            System.Windows.MessageBox.Show(aHTMLDocument.body.document.documentElement.outerHTML);


        }
    }
}