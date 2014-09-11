using DataModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LinqToDB;
using mshtml;
using MvvmLight1.Model;
using Npgsql;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private WebBrowser _WebBrowser;
        public WebBrowser mWebBrowser
        {
            get
            {
                return _WebBrowser;
            }
            set {
                _WebBrowser = value;
                RaisePropertyChanged("mWebBrowser");
            }
        }

        /*
        public RelayCommand<NavigationEventArgs> LoadCompletedCommand {
            get {
                return _LoadCompletedCommand;
            }
            private set {
                _LoadCompletedCommand = value;
            }
        }
        private RelayCommand<NavigationEventArgs> _LoadCompletedCommand = new RelayCommand<NavigationEventArgs>((e) =>
        {
            WebBrowser sender = e.Navigator as WebBrowser;
            System.Windows.MessageBox.Show("LoadCompletedCommand");
            WebBrowser aWB = sender as WebBrowser;
            HTMLDocument aHTMLDocument = aWB.Document as HTMLDocument;
            System.Windows.MessageBox.Show(aHTMLDocument.body.document.documentElement.outerHTML);

        });
        */
        public RelayCommand<WebBrowser> LoadCompletedCommand
        {
            get
            {
                return _LoadCompletedCommand;
            }
            private set
            {
                _LoadCompletedCommand = value;
            }
        }
        private RelayCommand<WebBrowser> _LoadCompletedCommand = new RelayCommand<WebBrowser>((sender) => {
            System.Windows.MessageBox.Show("LoadCompletedCommand");
            WebBrowser aWB = sender as WebBrowser;
            HTMLDocument aHTMLDocument = aWB.Document as HTMLDocument;
            System.Windows.MessageBox.Show(aHTMLDocument.body.document.documentElement.outerHTML);
        });


        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
            var testdb = new TestDBWrap();
            using(var db = new TestDBDB()) {
                
                db.users.ForEachAsync((users) =>
                {
                    System.Windows.MessageBox.Show(users.id.ToString());
                    System.Windows.MessageBox.Show(users.code.ToString());
                });
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}