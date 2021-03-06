﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Project.IO;
using Project.Windows;
using RSSReader.Model;

using static RSSReader.Define;

namespace RSSReader.Pages
{
    /// <summary>
    /// FeedViewPage.xaml の相互作用ロジック
    /// </summary>
    public partial class FeedViewPage : Page
    {
        #region プロパティ・定数定義
        /// <summary>
        /// google chrome ブラウザパス
        /// </summary>
        /// <remarks>
        /// Todo : デフォルトブラウザとの切替
        /// </remarks>
        private String ChromePath { get; set; }

        /// <summary>データ自動更新用のタイマ</summary>
        private DispatcherTimer AutoUpdateTimer;

        /// <summary>アイテムの項目数</summary>
        public Int32 SiteItemCount { get { return this.SiteSelectBox.Items.Count; } }

        ///// <summary>動作設定</summary>
        //public RssConfigure Config { get; set; }

        /// <summary>ページ指定</summary>
        private Int32 PageID { get;  set; }
        #endregion

        #region コンストラクタ
        public FeedViewPage()
        {
            InitializeComponent();

            InitializeControls(-1);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeedViewPage(Int32 pageID)
        {
            InitializeComponent();

            InitializeControls(pageID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        private void InitializeControls(Int32 pageID)
        {
            // RSSフィード登録サイトの読み込み
            ReLoadSiteItems();

            // Chromeのパス読み込み
            this.ChromePath = Registry.GetValue(ChromeRegKey, null) as String;

            // データ自動更新タイマの初期化
            this.AutoUpdateTimer = new DispatcherTimer(DispatcherPriority.Normal) {
                Interval = new TimeSpan(1, 0, 0),
            };
            this.AutoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            this.AutoUpdateTimer.Start();

            // フィルタ用選択項目
            this.IsReadComboBox.ItemsSource = Enum.GetNames(typeof(ReadState));
            this.IsReadComboBox.SelectedIndex = 0;

            // 設定値の読み込み
            ReadBackground(App.Configure?.BackgroundImagePath, App.Configure?.ImagePosition);

            this.PageID = pageID;
        }
        #endregion

        #region イベント
        /// <summary>
        /// Loadedイベント
        /// </summary>
        private void Page_Loaded(Object sender, RoutedEventArgs e)
        {
            // コンボボックスは最初の項目を選択する
            if (0 < this.SiteSelectBox.Items.Count) {
                // 前回のページ保持オプション
                if (App.Configure?.IsKeepPage ?? false) {
                    Int32 id = 0;
                    // メインウインドウからの指定があれば優先する
                    if (this.PageID < 0) {
                        if ( ! Int32.TryParse(TextFile.Read(PAGE_DAT), out id)) {
                            id = 0;
                        }
                    }
                    else {
                        id = this.PageID;
                    }
                    Int32 page = GetIndexFromMasterID(id);
                    if (0 <= page && page < this.SiteSelectBox.Items.Count) {
                        // 正常にデータが読めたら値を設定。
                        this.SiteSelectBox.SelectedIndex = page;
                        return;
                    }
                }
                this.SiteSelectBox.SelectedIndex = 0;
            }
            DispOfflineMode(App.Configure);
        }

        /// <summary>
        /// データ自動更新タイマ
        /// </summary>
        private void AutoUpdateTimer_Tick(Object sender, EventArgs e)
        {
            // 別タスクで実行する。

        }

        /// <summary>
        /// RSSフィードのサイト選択変更
        /// </summary>
        private void SiteSelectBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb) {
                if (!(cmb.SelectedItem is RssSiteInfo item)) { return; }

                UpdateListBox(item, LISTBOX_UPDATE);

                this.FeedList.SelectedIndex = 0;
                this.FeedList.ScrollIntoView(this.FeedList.SelectedItem);

                FilterClear();

                // メインウインドウにサイト変更メッセージを送信する
                var bgw = WindowInfo.FindWindowByName(null, TITLE);
                WinMessage.Send(bgw, CHANGE_MESSAGE, (IntPtr)item.ID, IntPtr.Zero);
            }
        }

        /// <summary>
        /// RSSフィードの再読み込みボタン
        /// </summary>
        private void SiteReloadButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            UpdateListBox(item, LISTBOX_UPDATE);
        }

#if false
        // Todo : RSSエディット画面へ引っ越しする。
        /// <summary>
        /// すべてのサイトデータを取得してDBを更新する
        /// </summary>
        private void AllDownloadButton_Click(Object sender, RoutedEventArgs e)
        {
            if (0 < this.SiteSelectBox.Items.Count) { return; }

            for (Int32 index = 0; index < this.SiteSelectBox.Items.Count; index++) {
                if (this.SiteSelectBox.Items[index] is RssSiteInfo site) {
                    // サイト別に更新、リストボックスの更新は行わない
                    UpdateListBox(site, !LISTBOX_UPDATE);
                }
            }
        }

        /// <summary>
        /// アプリケーション設定画面遷移ボタン
        /// </summary>
        private void SettingButton_Click(Object sender, RoutedEventArgs e)
        {
			if (this.SiteSelectBox.Items.Count != 0) {
				Int32 index = (this.SiteSelectBox.SelectedItem as RssSiteInfo)?.ID ?? ERROR_RESULT;
				if (index < 0) { return; }
				TextFile.Write(PAGE_DAT, $"{index}", TextFile.OVER_WRITE);
			}

            this.NavigationService.Navigate(new ConfigurePage(this));
        }

        /// <summary>
        /// RSSフィード情報編集画面遷移ボタン
        /// </summary>
        private void FabButton_Click(Object sender, RoutedEventArgs e)
        {
			if (this.SiteSelectBox.Items.Count != 0) {
				Int32 index = (this.SiteSelectBox.SelectedItem as RssSiteInfo)?.ID ?? ERROR_RESULT;
				if (index < 0) { return; }

				TextFile.Write(PAGE_DAT, $"{index}", TextFile.OVER_WRITE);
			}
            this.NavigationService.Navigate(new RSSEditPage(this, GetSiteInfo()));
        }
#endif
        /// <summary>
        /// 記事ページ ブラウザ起動処理 ダブルクリック
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item) {
                if (item.Content is FeedItem feed) {
                    //CommFunc.StartBrowser(this.ChromePath, feed);
                    CommFunc.NavigateMyBrowser(feed);
                }
            }
        }

        /// <summary>
        /// 記事ページ ブラウザ起動処理 エンターキー
        /// </summary>
        private void FeedList_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) { return; }

            if (sender is ListBox box) {
                if (box.SelectedValue is FeedItem feed) {
                    //CommFunc.StartBrowser(this.ChromePath, feed);
                    CommFunc.NavigateMyBrowser(feed);
                }
            }
        }

        /// <summary>
        /// 日付フィルタ解除ボタン
        /// </summary>
        private void DateClearButton_Click(Object sender, RoutedEventArgs e)
        {
            this.DatePick.SelectedDate = null;

            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }
            // フィルタ関係なく最新の状態にする
            UpdateListBox(item, LISTBOX_UPDATE);
        }

        /// <summary>
        /// キーワードフィルタ解除ボタン
        /// </summary>
        private void KeywordClearButton_Click(Object sender, RoutedEventArgs e)
        {
            this.KeywordBox.Text = String.Empty;

            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }
            // フィルタ関係なく最新の状態にする
            UpdateListBox(item, LISTBOX_UPDATE);
        }

        /// <summary>
        /// カレンダーを閉じたときにフィルタをかける
        /// </summary>
        private void DatePick_CalendarClosed(Object sender, RoutedEventArgs e)
        {
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            FilteringItems(item, this.KeywordBox.Text,
                this.DatePick.SelectedDate, this.IsReadComboBox.SelectedItem as String);
        }

        /// <summary>
        /// カレンダーでエンターキーを押したときにフィルタをかける
        /// </summary>
        private void DatePick_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) { return; }
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            if (DateTime.TryParse(this.DatePick.Text, out DateTime editDate)) {
                FilteringItems(item, this.KeywordBox.Text,
                            editDate, this.IsReadComboBox.SelectedItem as String);
            }
        }

        /// <summary>
        /// エンターキーを押したときにフィルタをかける
        /// </summary>
        private void KeywordBox_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) { return; }
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            FilteringItems(item, this.KeywordBox.Text,
                this.DatePick.SelectedDate, this.IsReadComboBox.SelectedItem as String);
        }

        /// <summary>
        /// 選択肢を変更したときにフィルタをかける
        /// </summary>
        private void IsReadComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            FilteringItems(item, this.KeywordBox.Text,
                this.DatePick.SelectedDate, this.IsReadComboBox.SelectedItem as String);
        }

        /// <summary>
        /// ピックアップアイテムメニューの選択
        /// </summary>
        private void MenuItemPickup_Click(Object sender, RoutedEventArgs e)
        {
            if( this.FeedList.SelectedItem is FeedItem feedItem) {
                RegistPickup(feedItem.ID);
            }
        }
        #endregion


    }
}