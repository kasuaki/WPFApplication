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

        Collection<IWebBrowserVM> WBVMWatchCollection { get; set; }
        Collection<IWebBrowserVM> WBVMBuyCollection { get; set; }
        
        /// <summary>
        /// ボタン実行処理.
        /// </summary>
        /// <param name="window"></param>
        private async void ButtonClickCommandEventHandler(MainWindow window)
        {
            MainViewModel aMainVM = window.DataContext as MainViewModel;
            IWebBrowserVM aMyWB = aMainVM.WBVMWatchCollection.First();

            await CVM.DB.JudgeBuyCell();
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
            WBVMWatchCollection.ToList().ForEach((element) => {

                element.WebBrowserAdd(TmpGrid);

                System.Threading.Timer timer = new System.Threading.Timer((state) =>
                {
                    Task.Factory.StartNew((obj) =>
                    {
                        element.TimerStart();
                    }, CVM.UI);

                    CVM.Dispatcher.BeginInvoke(new Action(() => { element.PageUpdate(); }));

                }, null, 3000, System.Threading.Timeout.Infinite);
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
            CVM.JudgeShare += CVM_AnalysisEnd;
            CVM.BuyShare += CVM_BuyEquity;

            WBVMBuyCollection = new Collection<IWebBrowserVM>();
            WBVMWatchCollection = new Collection<IWebBrowserVM>();
            WBVMWatchCollection.Add(new MyWebBrowserVM(new Uri(@"https://site2.sbisec.co.jp/ETGate/"), CVM));

        }

        /// <summary>
        /// 株買う通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CVM_BuyEquity(object sender, EventArgs e)
        {
            WBVMBuyCollection.Add(new MyWebBrowserVM(new Uri(@"https://site2.sbisec.co.jp/ETGate/"), CVM));
        }

        /// <summary>
        /// 解析完了通知後処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CVM_AnalysisEnd(object sender, EventArgs e)
        {
            // 判断.
            Tuple<Boolean, Int32> result = await CVM.DB.JudgeBuyCell();

            // 購入依頼.
            if (result.Item1)
            {
                CVM.OnBuyShare(new EventArgs());
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}