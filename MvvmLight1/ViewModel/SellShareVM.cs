using GalaSoft.MvvmLight;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using mshtml;
using CsQuery;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    [Export(typeof(ISellShareVM))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SellShareVM : BuySellShareBaseVM, ISellShareVM
    {
        protected override void LoadCompletedEvent(WebBrowser sender)
        {
            SellShare();
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        [ImportingConstructor]
        public SellShareVM([Import("SellShareVM.aUri")] Uri aUri,
                           [Import("SellShareVM.aCommonVM")] CommonVM aCommonVM,
                           [Import("SellShareVM.aColumn")] Int32 aColumn,
                           [Import("SellShareVM.aRow")] Int32 aRow,
                           [Import("SellShareVM.aCode")] Int32 aCode)
            : base(aUri, aCommonVM, aColumn, aRow, aCode)
        {
            LoadCompletedCommand = new RelayCommand<WebBrowser>(LoadCompletedEvent);
        }

        public void SellShare()
        {
            HTMLDocument aHTMLDocument = WB.Document as HTMLDocument;

            if (aHTMLDocument == null)
            {
                WB.Navigate(MyUri);
                return;
            }

            var pList = aHTMLDocument.getElementsByTagName("p").Cast<IHTMLElement>();
            var bList = aHTMLDocument.getElementsByTagName("b").Cast<IHTMLElement>();
            var aList = aHTMLDocument.getElementsByTagName("a").Cast<IHTMLElement>();
            var inputList = aHTMLDocument.getElementsByTagName("input").Cast<IHTMLElement>();
            var imageList = aHTMLDocument.getElementsByTagName("image").Cast<IHTMLElement>();

            // ログイン前ポータル画面(想定).
            if (inputList.Where(element => element.title != null).Count(element => Regex.IsMatch(element.title, @"ログイン")) != 0)
            {
                IHTMLElement user = inputList.First(element => "user_id".Equals(element.getAttribute("name")));
                IHTMLElement pass = inputList.First(element => "user_password".Equals(element.getAttribute("name")));
                user.setAttribute("value", User);
                pass.setAttribute("value", Pass);
                IHTMLElement button = inputList.First((element) => "ログイン".Equals(element.title));
                button.click();
            }
            // ログイン後ポータル画面(想定).
            else if (imageList.Where(e => e.getAttribute("alt") != null).Count(element => Regex.IsMatch(element.getAttribute("alt"), @"口座状況")) != 0)
            {
                // DBから現売リンクを取得.
                String str = CVM.DB.TestDB.shares.Where((e) => Code.Equals(e.銘柄コード)).OrderByDescending(e => e.更新日時).Select(e => e.現売リンク).First();
                WB.Navigate(str);
            }
            else if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"注文を受")) != 0)
            {
                CVM.OnSellShareEnd(new BuySellEventArgs(Code, true));
            }
            // ポートフォリオ画面(想定).
            else if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"注文入力（現物売）")) != 0)
            {
                var image = imageList.First((e) => Regex.IsMatch(e.getAttribute("src"), @"b_plus.gif"));
                var plusButton = image.parentElement;
                plusButton.click();

                var nariButton = inputList.First(e => "in_sasinari_kbn".Equals(e.getAttribute("name")) && "N".Equals(e.getAttribute("value")));
                nariButton.click();

                var pass = inputList.First(e => "trade_pwd".Equals(e.getAttribute("name")));
                pass.setAttribute("value", Pass);

                HTMLInputElement checkbox = (HTMLInputElement)(inputList.First(e => "skip_estimate".Equals(e.getAttribute("name"))));
                checkbox.@checked = true;

                var submitButton = inputList.First(e => @"注文発注".Equals(e.getAttribute("value")));
                // TODO:注文コメントアウトしている.
                submitButton.click();
            }
        }
    }
}