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
            this.DataContext = new MainViewModel(this.Dispatcher);

            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            MainViewModel aMainVM = this.DataContext as MainViewModel;
            aMainVM.TmpGrid = this.tmpGrid;
        }
    }
}