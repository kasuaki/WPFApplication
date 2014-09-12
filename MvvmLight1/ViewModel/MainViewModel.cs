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
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : MyViewModelBase
    {
        private Grid _TmpGrid;
        public Grid TmpGrid
        {
            get
            {
                return _TmpGrid;
            }
            set
            {
                _TmpGrid = value;
                RaisePropertyChanged("TmpGrid");
            }
        }

        Collection<IWebBrowserVM> WBVMCollection { get; set; }
        
        public RelayCommand<MainWindow> ButtonClickCommand
        {
            get
            {
                return _ButtonClickCommand;
            }
            set
            {
                _ButtonClickCommand = value;
            }
        }
        private RelayCommand<MainWindow> _ButtonClickCommand = new RelayCommand<MainWindow>((window) =>
        {
            MainViewModel aMainVM = window.DataContext as MainViewModel;
            IWebBrowserVM aMyWB = aMainVM.WBVMCollection.First();

            aMyWB.WB.Navigate(aMyWB.MyUri);
        });

        public RelayCommand<RoutedEventArgs> LoadedCommand {
            get {
                return _LoadedCommand;
            }
            private set {
                _LoadedCommand = value;
            }
        }
        private RelayCommand<RoutedEventArgs> _LoadedCommand = new RelayCommand<RoutedEventArgs>((e) =>
        {
            MainWindow aMainWindow = e.Source as MainWindow;
            MainViewModel aMainVM = aMainWindow.DataContext as MainViewModel;
            IWebBrowserVM aMyWB = aMainVM.WBVMCollection.First();

            aMainVM.TmpGrid.Children.Add(aMyWB.Grid);

//            System.Windows.MessageBox.Show("LoadedCommand");
        });

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            CommonVM aCVM = new CommonVM();
            CVM = aCVM;
            WBVMCollection = new Collection<IWebBrowserVM>();
            WBVMCollection.Add(new MyWebBrowserVM(new Uri(@"https://site2.sbisec.co.jp/ETGate/"), CVM));

        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}