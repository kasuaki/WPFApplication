using GalaSoft.MvvmLight;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MyViewModelBase : ViewModelBase
    {
        public CommonVM CVM { get; set; }

        public MyViewModelBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the MyViewModelBase class.
        /// </summary>
        public MyViewModelBase(CommonVM aCVM)
        {
            CVM = aCVM;
        }
    }
}