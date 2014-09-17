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

namespace MvvmLight1.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class WatchSpecificShareVM : BuySellShareBaseVM, IWatchSpecificShareVM
    {
        protected override void LoadCompletedEvent(WebBrowser sender)
        {
            WatchShare();
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public WatchSpecificShareVM(Uri aUri, CommonVM aCommonVM, Int32 aColumn, Int32 aRow, Int32 aCode)
            : base(aUri, aCommonVM, aColumn, aRow, aCode)
        {
            LoadCompletedCommand = new RelayCommand<WebBrowser>(LoadCompletedEvent);
        }

        public void WatchShare()
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
            var spanList = aHTMLDocument.getElementsByTagName("span").Cast<IHTMLElement>();

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
                var codeInput = inputList.Where(e => e.getAttribute("id") != null).First(e => "codeSearch".Equals(e.getAttribute("id")));
                codeInput.setAttribute("value", Code);

                var searchButton = inputList.Where(e => e.getAttribute("alt") != null).First(e => "検索".Equals(e.getAttribute("alt")));
                searchButton.click();
            }
            // 特定の証券画面(想定).
            else if (spanList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"（" + Code + @"）")) != 0)
            {

                // 自動更新ボタン押下(毎回押しちゃう).
                var image = imageList.Where(e => e.getAttribute("id") != null).First((e) => Regex.IsMatch(e.getAttribute("id"), @"imgRefArea_MTB0_on"));
                var updateButton = image.parentElement;
                updateButton.click();

                var tdList = aHTMLDocument.getElementsByTagName("td").Cast<IHTMLElement>();
                IHTMLElement tanka = tdList.First(element => ((element.getAttribute("id") != null) && ("MTB0_0".Equals(element.getAttribute("id")))));
                IHTMLElement table = tanka.parentElement.parentElement.parentElement;

                // 解析.
                CVM.DB.AnalysisSpecificShare(table, Code);
            }
        }
    }
}