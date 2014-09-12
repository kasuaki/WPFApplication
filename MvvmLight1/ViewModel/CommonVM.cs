using GalaSoft.MvvmLight;
using MvvmLight1.Model;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CommonVM : MyViewModelBase
    {
        public TestDBWrap DB { get; set; }
        /// <summary>
        /// Initializes a new instance of the CommonVM class.
        /// </summary>
        public CommonVM()
        {
            CVM = this;
            DB = new TestDBWrap();
            //using(var db = new TestDBDB()) {

            //    db.users.ForEachAsync((users) =>
            //    {
            //        System.Windows.MessageBox.Show(users.id.ToString());
            //        System.Windows.MessageBox.Show(users.code.ToString());
            //    });
            //}
        }
    }
}