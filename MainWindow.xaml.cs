using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using MahApps.Metro.Controls;
using System.IO;

namespace volotools
{
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            SideMenu.MainTabs = MainTabs;

        }

        private void ChangeLanguage_Click(object sender, RoutedEventArgs e) //言語切替（仮）
        {
            // 日本語に切り替え
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ja");

            // 画面を再読み込みして言語を更新
            Application.Current.MainWindow.Content = null;
            InitializeComponent(); // 言語変更後に画面を再初期化
        }
    }
}