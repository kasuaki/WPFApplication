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
using System.Windows.Threading;
using System.Threading.Tasks;

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
        private Panel _TmpGrid;
        public Panel TmpGrid
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

        Collection<IWebBrowserVM> WBVMWatchCollection { get; set; }
        public IBuyShareVM BuyShareVM;
        public ISellShareVM SellShareVM;
        
        /// <summary>
        /// ボタン実行処理.
        /// </summary>
        /// <param name="window"></param>
        private void ButtonClickCommandEventHandler(MainWindow window)
        {
            MainViewModel aMainVM = window.DataContext as MainViewModel;
            IWebBrowserVM aMyWB = aMainVM.WBVMWatchCollection.First();

            CVM.DB.JudgeBuy();
        }
        private RelayCommand<MainWindow> _ButtonClickCommand;
        public RelayCommand<MainWindow> ButtonClickCommand
        {
            get
            {
                return _ButtonClickCommand;
            }
            set
            {
                _ButtonClickCommand = value;
                RaisePropertyChanged("ButtonClickCommand");
            }
        }

        /// <summary>
        /// WindowのLoaded完了時の処理.
        /// 登録されているWebBrowserコレクションを回してTimerStart.
        /// </summary>
        /// <param name="e"></param>
        private void LoadedCommandEventHandler(RoutedEventArgs e)
        {
            Int32 counter = 3000;

            WBVMWatchCollection.ToList().ForEach((element) => {

                element.WebBrowserAdd(TmpGrid);

                System.Threading.Timer timer = new System.Threading.Timer((state) =>
                {
                    Task.Factory.StartNew((obj) =>
                    {
                        element.TimerStart();
                    }, CVM.UI);

                    CVM.Dispatcher.BeginInvoke(new Action(() => { element.PageUpdate(); }));

                }, null, counter, System.Threading.Timeout.Infinite);

//                counter *= 2;
            });
        }
        private RelayCommand<RoutedEventArgs> _LoadedCommand;
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _LoadedCommand;
            }
            private set
            {
                _LoadedCommand = value;
                RaisePropertyChanged("LoadCompletedCommand");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(Dispatcher aDispatcher)
        {
            LoadedCommand = new RelayCommand<RoutedEventArgs>(LoadedCommandEventHandler);
            ButtonClickCommand = new RelayCommand<MainWindow>(ButtonClickCommandEventHandler);

            CVM = new CommonVM(aDispatcher);

            WBVMWatchCollection = new Collection<IWebBrowserVM>();
            WBVMWatchCollection.Add(new CheckPortfolioWBVM(CVM.MyUri, CVM, 0, 1, "角川ドワンゴ"));
            WBVMWatchCollection.Add(new CheckShareWBVM(CVM.MyUri, CVM, 0, 2));

            CVM.BuyShare += CVM_BuyShare;
            CVM.BuyShareEnd += CVM_BuyShareEnd;
            CVM.SellShare += CVM_SellShare;
            CVM.SellShareEnd += CVM_SellShareEnd;
        }

        /// <summary>
        /// 株売終了通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CVM_SellShareEnd(object sender, BuySellEventArgs e)
        {
            CheckShareWBVM aCheckShareWBVM = WBVMWatchCollection.OfType<CheckShareWBVM>().First();
            aCheckShareWBVM.MyStatus = Status.Watching;

            SellShareVM.WebBrowserRemove();
            SellShareVM.Dispose();
            SellShareVM = null;
        }

        /// <summary>
        /// 株買終了通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_BuyShareEnd(object sender, BuySellEventArgs e)
        {
            CheckShareWBVM aCheckShareWBVM = WBVMWatchCollection.OfType<CheckShareWBVM>().First();
            aCheckShareWBVM.MyStatus = Status.HaveShares;

            BuyShareVM.WebBrowserRemove();
            BuyShareVM.Dispose();
            BuyShareVM = null;
        }

        /// <summary>
        /// 株買う通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_BuyShare(object sender, BuySellEventArgs e)
        {
            BuyShareVM = new BuyShareVM(CVM.MyUri, CVM, 0, 3, e.Code);
            BuyShareVM.WebBrowserAdd(TmpGrid);

            System.Threading.Timer timer = new System.Threading.Timer((state) =>
            {
                CVM.Dispatcher.BeginInvoke(new Action(() => { BuyShareVM.WB.Navigate(CVM.MyUri); }));

            }, null, 3000, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// 株売る通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_SellShare(object sender, BuySellEventArgs e)
        {
            SellShareVM = new SellShareVM(CVM.MyUri, CVM, 0, 3, e.Code);
            SellShareVM.WebBrowserAdd(TmpGrid);

            System.Threading.Timer timer = new System.Threading.Timer((state) =>
            {
                CVM.Dispatcher.BeginInvoke(new Action(() => { SellShareVM.WB.Navigate(CVM.MyUri); }));

            }, null, 3000, System.Threading.Timeout.Infinite);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}