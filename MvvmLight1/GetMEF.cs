using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight1
{
    [Export]
    public class GetMEF
    {
        // 拡張を取り込む
        //[ImportMany]
        //public IEnumerable<ViewModel.IBuyShareVM> ExtensionPoints { get; set; }

        [Import(typeof(ViewModel.IBuyShareVM))]
        public ViewModel.IBuyShareVM BuyShareVM { get; set; }
    }
}
