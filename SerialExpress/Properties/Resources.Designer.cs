﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SerialExpress.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SerialExpress.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   command に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string CommandDirName {
            get {
                return ResourceManager.GetString("CommandDirName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   command_history.txt に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string CommandHistoryFileName {
            get {
                return ResourceManager.GetString("CommandHistoryFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   config.json に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ConfigurationFileName {
            get {
                return ResourceManager.GetString("ConfigurationFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The MIT License (MIT)
        ///
        ///Copyright (c) 2015 Microsoft
        ///
        ///Permission is hereby granted, free of charge, to any person obtaining a copy
        ///of this software and associated documentation files (the &quot;Software&quot;), to deal
        ///in the Software without restriction, including without limitation the rights
        ///to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        ///copies of the Software, and to permit persons to whom the Software is
        ///furnished to do so, subject to the following conditions:
        ///
        ///The above copy [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LICENSE {
            get {
                return ResourceManager.GetString("LICENSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   log に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogDirName {
            get {
                return ResourceManager.GetString("LogDirName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   received_data.bin に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string RxDataBinFileName {
            get {
                return ResourceManager.GetString("RxDataBinFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   received_data_log.txt に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string RxDataTextFileName {
            get {
                return ResourceManager.GetString("RxDataTextFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   sent_data.bin に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string TxDataBinFileName {
            get {
                return ResourceManager.GetString("TxDataBinFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   sent_data_log.txt に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string TxDataTextFileName {
            get {
                return ResourceManager.GetString("TxDataTextFileName", resourceCulture);
            }
        }
    }
}
