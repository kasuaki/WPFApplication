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
    public class CheckShareWBVM : MyWebBrowserVM, IWebBrowserVM
    {
        protected override void LoadCompletedEvent(WebBrowser sender)
        {
            WatchStart();
        }

        /// <summary>
        /// Initializes a new instance of the WebBrowserVM class.
        /// </summary>
        public CheckShareWBVM(Uri aUri, CommonVM aCommonVM, Int32 aColumn, Int32 aRow)
            : base(aUri, aCommonVM, aColumn, aRow)
        {
            LoadCompletedCommand = new RelayCommand<WebBrowser>(LoadCompletedEvent);
            CVM.JudgeBuyShare += CVM_JudgeBuyShare;
            CVM.BuyShare += CVM_BuyShare;
            CVM.JudgeSellShare += CVM_JudgeSellShare;
            CVM.SellShare += CVM_SellShare;
        }

        /// <summary>
        /// ページ更新処理.
        /// </summary>
        public override void PageUpdate()
        {
            if (WB.Document != null)
            {
                HTMLDocument aHTMLDocument = WB.Document as HTMLDocument;
                var bList = aHTMLDocument.getElementsByTagName("b").Cast<IHTMLElement>();
                // 口座管理画面(想定).
                if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"口座サマリー")) != 0)
                {
                    var imgList = aHTMLDocument.getElementsByTagName("image").Cast<IHTMLElement>();
                    IHTMLElement portfolio = imgList.FirstOrDefault(element => @"口座管理".Equals(element.title));
                    if (portfolio != null)
                    {
                        IHTMLElement a = portfolio.parentElement;
                        a.click();
                    }
                }
                else
                {
                    WatchStart();
                }
            }
            else
            {
                WatchStart();
            }
        }

        /// <summary>
        /// 監視処理.
        /// </summary>
        public override void WatchStart()
        {
            HTMLDocument aHTMLDocument = WB.Document as HTMLDocument;

            if (aHTMLDocument == null)
            {
                WB.Navigate(MyUri);
                return;
            }

            var pList = aHTMLDocument.getElementsByTagName("p").Cast<IHTMLElement>();
            var bList = aHTMLDocument.getElementsByTagName("b").Cast<IHTMLElement>();
            var inputList = aHTMLDocument.getElementsByTagName("input").Cast<IHTMLElement>();

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
            else if (pList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"最終ログイン.*")) != 0)
            {
                var imgList = aHTMLDocument.getElementsByTagName("image").Cast<IHTMLElement>();
                IHTMLElement portfolio = imgList.FirstOrDefault(element => @"口座管理".Equals(element.title));
                if (portfolio != null)
                {
                    IHTMLElement a = portfolio.parentElement;
                    a.click();
                }
            }
            // 口座管理画面(想定).
            else if (bList.Where(element => element.innerText != null).Count(element => Regex.IsMatch(element.innerText, @"口座サマリー")) != 0)
            {
                // テーブル取得.
                var tdList = aHTMLDocument.getElementsByTagName("font").Cast<IHTMLElement>();
                IHTMLElement tanka = tdList.First(element => ((element.innerText != null) && (element.innerText.Equals(@"保有株数"))));
                IHTMLElement table = tanka.parentElement.parentElement.parentElement;

                // 保存.
                CVM.DB.AnalysisShare(table);

                if (Status.Watching.Equals(MyStatus))
                {
                    CVM.OnJudgeBuyShare(new EventArgs());
                }
                else if (Status.HaveShares.Equals(MyStatus))
                {
                    CVM.OnJudgeSellShare(new EventArgs());
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Timer満了時の動作.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void MyDispatcherTimer_Tick(Object sender, EventArgs e)
        {
            PageUpdate();
        }


        /// <summary>
        /// 株買う通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_BuyShare(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 買い判断通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CVM_JudgeBuyShare(object sender, EventArgs e)
        {
            // 判断.
            Tuple<Boolean, Int32> result = await CVM.DB.JudgeBuy();

            // 購入依頼.
            if (result.Item1)
            {
                CVM.OnBuyShare(new EventArgs());
            }
        }

        /// <summary>
        /// 株売る通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CVM_SellShare(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 売り判断通知処理.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CVM_JudgeSellShare(object sender, EventArgs e)
        {
            // 判断.
            Tuple<Boolean, Int32> result = await CVM.DB.JudgeSell();

            // 売却依頼.
            if (result.Item1)
            {
                CVM.OnSellShare(new EventArgs());
            }
        }

    }
}