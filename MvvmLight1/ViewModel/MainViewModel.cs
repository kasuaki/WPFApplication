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
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

namespace MvvmLight1.ViewModel
{
    [Export]
    public class BuyShareVMFactory
    {
        [Import(typeof(IBuyShareVM))]
        public IBuyShareVM BuyShareVM { get; set; }
    }

    [Export]
    public class SellShareVMFactory
    {
        [Import(typeof(ISellShareVM))]
        public ISellShareVM SellShareVM { get; set; }
    }

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    [Export]
    public class MainViewModel : MyViewModelBase, IDisposable
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

        [ImportMany(typeof(ICheckPortfolioWBVM))]
        Collection<ICheckPortfolioWBVM> WBVMWatchCollection { get; set; }

//        [Import(typeof(IBuyShareVM))]
        public IBuyShareVM BuyShareVM { get; set; }
        //[Import(typeof(ISellShareVM))]
        public ISellShareVM SellShareVM { get; set; }
        [Import(typeof(ICheckShareWBVM))]
        public ICheckShareWBVM CheckShareWBVM { get; set; }
        public Collection<IWatchSpecificShareVM> WatchSpecificShareVMCollection { get; set; }
        public Lazy<GetMEF> MyGetMEF { get; set; }
        
        /// <summary>
        /// ボタン実行処理.
        /// </summary>
        /// <param name="window"></param>
        private void ButtonClickCommandEventHandler(MainWindow window)
        {
            MainViewModel aMainVM = window.DataContext as MainViewModel;
            ICheckPortfolioWBVM aMyWB = aMainVM.WBVMWatchCollection.First();

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
        private void ClosedCommandEventHandler(EventArgs e)
        {
            Dispose();
        }
        private RelayCommand<EventArgs> _ClosedCommand;
        public RelayCommand<EventArgs> ClosedCommand
        {
            get
            {
                return _ClosedCommand;
            }
            private set
            {
                _ClosedCommand = value;
                RaisePropertyChanged("ClosedCommand");
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

            CheckShareWBVM.WebBrowserAdd(TmpGrid);
            new System.Threading.Timer((state) =>
            {
                Task.Factory.StartNew((obj) =>
                {
                    CheckShareWBVM.TimerStart();
                }, CVM.UI);

                CVM.Dispatcher.BeginInvoke(new Action(() => { CheckShareWBVM.PageUpdate(); }));

            }, null, counter, System.Threading.Timeout.Infinite);

            WatchSpecificShareVMCollection.ToList().ForEach((element) => {
                element.WebBrowserAdd(TmpGrid);

                CVM.Dispatcher.BeginInvoke(new Action(() => { element.WB.Navigate(CVM.MyUri); }));
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
                RaisePropertyChanged("LoadedCommand");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(Dispatcher aDispatcher)
        {
            LoadedCommand = new RelayCommand<RoutedEventArgs>(LoadedCommandEventHandler);
            ClosedCommand = new RelayCommand<EventArgs>(ClosedCommandEventHandler);
            ButtonClickCommand = new RelayCommand<MainWindow>(ButtonClickCommandEventHandler);

            CVM = new CommonVM(aDispatcher);

            WBVMWatchCollection = new Collection<ICheckPortfolioWBVM>();

            // Catalogを１まとめに  
            CVM.AggregateCatalog = new AggregateCatalog();
            CVM.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(CVM.AggregateCatalog);

            // CatalogでContainerを作成  
            container.ComposeExportedValue("ICheckPortfolioWBVM.aUri", CVM.MyUri);
            container.ComposeExportedValue("ICheckPortfolioWBVM.aCommonVM", CVM);
            container.ComposeExportedValue("ICheckPortfolioWBVM.aColumn", 0);
            container.ComposeExportedValue("ICheckPortfolioWBVM.aRow", 1);
            container.ComposeExportedValue("ICheckPortfolioWBVM.aPortFolio", "角川ドワンゴ");

            container.ComposeExportedValue("ICheckShareWBVM.aUri", CVM.MyUri);
            container.ComposeExportedValue("ICheckShareWBVM.aCommonVM", CVM);
            container.ComposeExportedValue("ICheckShareWBVM.aColumn", 0);
            container.ComposeExportedValue("ICheckShareWBVM.aRow", 2);
          
            container.ComposeParts(this);

            WatchSpecificShareVMCollection = new Collection<IWatchSpecificShareVM>();
//            WatchSpecificShareVMCollection.Add(new WatchSpecificShareVM(CVM.MyUri, CVM, 0, 3, 3715));

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
            this.CheckShareWBVM.MyStatus = Status.Watching;

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
            this.CheckShareWBVM.MyStatus = Status.HaveShares;

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
            // CatalogでContainerを作成  
            BuyShareVMFactory aBuyShareVMFactory = new BuyShareVMFactory();
            var container = new CompositionContainer(CVM.AggregateCatalog);

            container.ComposeExportedValue("BuyShareVM.aUri", CVM.MyUri);
            container.ComposeExportedValue("BuyShareVM.aCommonVM", CVM);
            container.ComposeExportedValue("BuyShareVM.aColumn", 0);
            container.ComposeExportedValue("BuyShareVM.aRow", 3);
            container.ComposeExportedValue("BuyShareVM.aCode", e.Code);

            container.ComposeParts(aBuyShareVMFactory);

            BuyShareVM = aBuyShareVMFactory.BuyShareVM;
            BuyShareVM.WebBrowserAdd(TmpGrid);

            System.Threading.Timer timer = new System.Threading.Timer((state) =>
            {
                CVM.Dispatcher.BeginInvoke(new Action(() => { BuyShareVM.WB.Navigate(CVM.MyUri); }));

            }, null, 500, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// 株売る通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_SellShare(object sender, BuySellEventArgs e)
        {
            // CatalogでContainerを作成  
            SellShareVMFactory aSellShareVMFactory = new SellShareVMFactory();
            var container = new CompositionContainer(CVM.AggregateCatalog);

            container.ComposeExportedValue("SellShareVM.aUri", CVM.MyUri);
            container.ComposeExportedValue("SellShareVM.aCommonVM", CVM);
            container.ComposeExportedValue("SellShareVM.aColumn", 0);
            container.ComposeExportedValue("SellShareVM.aRow", 3);
            container.ComposeExportedValue("SellShareVM.aCode", e.Code);

            container.ComposeParts(aSellShareVMFactory);

            SellShareVM = aSellShareVMFactory.SellShareVM;
            SellShareVM.WebBrowserAdd(TmpGrid);

            System.Threading.Timer timer = new System.Threading.Timer((state) =>
            {
                CVM.Dispatcher.BeginInvoke(new Action(() => { SellShareVM.WB.Navigate(CVM.MyUri); }));

            }, null, 500, System.Threading.Timeout.Infinite);
        }

        public void Dispose()
        {
            WBVMWatchCollection.ToList().ForEach(o =>
            {
                o.Dispose();
            });
            WBVMWatchCollection.Clear();
            WBVMWatchCollection = null;

            if (BuyShareVM != null)
            {
                BuyShareVM.Dispose();
                BuyShareVM = null;
            }

            if (SellShareVM != null)
            {
                SellShareVM.Dispose();
                SellShareVM = null;
            }

            WatchSpecificShareVMCollection.ToList().ForEach(o =>
            {
                o.Dispose();
            });
            WatchSpecificShareVMCollection.Clear();
            WatchSpecificShareVMCollection = null;

            CVM.Dispose();
            CVM = null;
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}