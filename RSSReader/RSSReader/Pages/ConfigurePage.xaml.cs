﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.DataBase;
using Project.Serialization.Xml;
using RSSReader.Model;
using static RSSReader.Define;

namespace RSSReader.Pages
{
    /// <summary>
    /// ConfigurePage.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigurePage : Page
    {
        private FeedViewPage InnerViewPage { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="inner"></param>
        public ConfigurePage(FeedViewPage inner)
        {
            InitializeComponent();
            this.ConfGrid.DataContext = inner.Config;
            this.InnerViewPage = inner;
        }

        /// <summary>
        /// 戻るボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(this.InnerViewPage);
        }

        /// <summary>
        /// 設定の保存ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            try {
                var conf = this.ConfGrid.DataContext as RssConfigure;

                // 内部で定義している間隔以下は設定できない。
                if (conf.UpdateSpan < INTERVAL_TIME) {
                    conf.UpdateSpan = INTERVAL_TIME;
                }

                XmlSerializer.Save(conf, XML_PATH);

                this.InnerViewPage.Config = conf;
            }
            catch (Exception) {
                MessageBox.Show("setting error!", "error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// キャッシュしている画像の削除
        /// </summary>
        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Cancel
                 == MessageBox.Show("Delete unnecessary files?", "message",
                                    MessageBoxButton.OKCancel)) {
                return;
            }

            if (!File.Exists(MASTER_PATH)) {
                MessageBox.Show("DB file not found", "error", MessageBoxButton.OK);
                return;
            }

            // 画像ファイル削除
            CashFileDelete();

            // DBの最適化
            DBMaintenance();
        }

        /// <summary>
        /// 不要になった画像ファイルを削除する。
        /// </summary>
        private void CashFileDelete()
        {
            try {
                using (var db = new SQLite(MASTER_PATH)) {

                    db.Open();
                    var masterIDs = db.Select("select id from rss_master")["id"];

                    foreach (var id in masterIDs) {
                        // URLからファイル名だけを抜き出したリストにする
                        var thumbs = new HashSet<String>(
                            db.Select($"select thumb_url from log where master_id={id}")["thumb_url"]
                            .Select(p => System.IO.Path.GetFileName(p)));
                        // 個別のディレクトリからファイル名を取得
                        var files = System.IO.Directory.GetFiles($@"{FeedItem.CHASH_DIR}\{id}", "*",
                            System.IO.SearchOption.TopDirectoryOnly);

                        foreach (var f in files) {
                            // DBへの登録なしのサムネを削除する
                            if (!thumbs.Contains(System.IO.Path.GetFileName(f))) {
                                System.IO.File.Delete(f);
                            }
                        }
                    }
                }
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// DBファイルのテンポラリ領域の解放
        /// </summary>
        private void DBMaintenance()
        {
            try {
                using (var db = new SQLite(MASTER_PATH)) {
                    db.Open();
                    db.Update("VACUUM");
                }
            }
            catch (Exception) {
            }
        }
    }
}
