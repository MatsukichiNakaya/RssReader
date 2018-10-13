﻿using System;

namespace RSSReader
{
    internal class Define
    {
        /// <summary>
        /// 
        /// </summary>
        public const String TITLE = "RssReader";

        /// <summary>
        /// RSS 1.0
        /// </summary>
        public const String RSS_HEADER_10 = "rdf:RDF";
        /// <summary>
        /// RSS 2.0
        /// </summary>
        public const String RSS_HEADER_20 = "rss";
        /// <summary>
        /// atom
        /// </summary>
        public const String RSS_HEADER_ATOM = "feed";

        /// <summary>
        /// RSSフィードの履歴保存DB
        /// </summary>
        public const String MASTER_PATH = @".\rss_log.db";

        /// <summary>
        /// ソフト設定値ファイル
        /// </summary>
        public const String XML_PATH = @"Configure.xml";

        /// <summary>
        /// 表示状態保持用ファイル
        /// </summary>
        public const String PAGE_DAT = @".\page.dat";

        /// <summary>
        /// エディットモード
        /// </summary>
        public enum EditMode
        {
            None = 0,
            Editing,
        }

        /// <summary>
        /// ページ保持用のメッセージ
        /// </summary>
        public const Int32 CHANGE_MESSAGE = 32770;

        /// <summary>
        /// ウインドウ最小化メッセージ
        /// </summary>
        public const Int32 Window_MIN_MESSAGE = 32771;

        /// <summary>
        /// エラー時の返り値
        /// </summary>
        public const Int32 ERROR_RESULT = -1;

        /// <summary>
        /// 同じ場所にアクセスする場合の時間間隔
        /// </summary>
        public const Int32 INTERVAL_TIME = 5;
    }
}
