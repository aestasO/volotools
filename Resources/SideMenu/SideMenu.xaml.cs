using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;


namespace volotools
{
    public partial class SideMenu : System.Windows.Controls.UserControl
    {
        public required MainTabs MainTabs { get; set; }
        public SideMenu()
        {
            InitializeComponent();
        }

        private bool isMenuOpen = false;

        private void MenuIcon_Click(object sender, RoutedEventArgs e)
        {
            // 新しいアニメーションの作成
            var widthAnimation = new DoubleAnimation
            {
                From = isMenuOpen ? 200 : 40,  // 現在の状態から、逆の状態に遷移
                To = isMenuOpen ? 40 : 200,    // 開いているときは閉じる、閉じているときは開く
                Duration = new Duration(TimeSpan.FromSeconds(0.3))  // アニメーション時間
            };

            // アニメーションを適用する対象を設定
            Storyboard storyboard = new();

            // StackPanelのWidthにアニメーションを適用
            storyboard.Children.Add(widthAnimation);
            Storyboard.SetTarget(widthAnimation, sideMenu);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(StackPanel.WidthProperty));

            // アニメーションの開始
            storyboard.Begin();

            // メニューの状態をトグル
            isMenuOpen = !isMenuOpen;
        }

        private void ToolIcon_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.AddWindow(sender, e);
        }


    }
}
