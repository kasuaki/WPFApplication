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
    [Export(typeof(IBuyShareVM))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuyShareVM : BuySellShareBaseVM, IBuyShareVM
    {
        protected override void LoadCompletedEvent(WebBrowser sender)
        {
            BuyShare();
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        [ImportingConstructor]
        public BuyShareVM([Import("BuyShareVM.aUri")] Uri aUri,
                          [Import("BuyShareVM.aCommonVM")] CommonVM aCommonVM,
                          [Import("BuyShareVM.aColumn")] Int32 aColumn,
                          [Import("BuyShareVM.aRow")] Int32 aRow,
                          [Import("BuyShareVM.aCode")] Int32 aCode)
            : base(aUri, aCommonVM, aColumn, aRow, aCode)
        {
            LoadCompletedCommand = new RelayCommand<WebBrowser>(LoadCompletedEvent);
        }

        public void BuyShare()
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
                // DBから現買リンクを取得.
                String str = CVM.DB.TestDB.portfolios.Where((e) => Code.Equals(e.銘柄コード)).OrderByDescending(e => e.更新日時).Select(e => e.現買リンク).First();
                WB.Navigate(str);
            }
            // ポートフォリオ画面(想定).
            else if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"注文入力（現物買）")) != 0)
            {
                // 株数インクリメント.
                var image = imageList.First((e) => Regex.IsMatch(e.getAttribute("src"), @"b_plus.gif"));
                var plusButton = image.parentElement;
                plusButton.click();

                // 成行に変更.
                var nariButton = inputList.First(e => "in_sasinari_kbn".Equals(e.getAttribute("name")) && "N".Equals(e.getAttribute("value")));
                nariButton.click();

                // 特定預かりに変更.
                var tokuteiButton = inputList.First(e => "hitokutei_trade_kbn".Equals(e.getAttribute("name")) && "0".Equals(e.getAttribute("value")));
                tokuteiButton.click();

                // パスワード入力.
                var pass = inputList.First(e => "trade_pwd".Equals(e.getAttribute("name")));
                pass.setAttribute("value", Pass);

                // 注文確認画面を省略.
                HTMLInputElement checkbox = (HTMLInputElement)(inputList.First(e => "skip_estimate".Equals(e.getAttribute("name"))));
                checkbox.@checked = true;

                var submitButton = inputList.First(e => @"注文発注".Equals(e.getAttribute("value")));
                // TODO:注文コメントアウトしている.
                //submitButton.click();

                CVM.OnBuyShareEnd(new BuySellEventArgs(Code));
            }
        }
    }
}