using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using static volotools.Tools.VideoDownloader;
using System.Collections.ObjectModel;

namespace volotools
{
    public partial class MainTabs : UserControl
    {
        public MainTabs()
        {
            InitializeComponent();
        }
        public void CloseTab(object sender, RoutedEventArgs e) // タブを閉じる
        {
            if (sender is Button closeButton)
            {
                // ボタンが属するタブアイテムを取得
                TabItem? tabItem = FindAncestor<TabItem>(closeButton);
                if (tabItem != null)
                {
                    // タブコントロールを取得
                    if (tabItem.Parent is TabControl tabControl)
                    {
                        // タブのインデックスを取得
                        int tabIndex = tabControl.Items.IndexOf(tabItem);

                        // タブを閉じる
                        tabControl.Items.Remove(tabItem);
                        // addタブの選択を回避する
                        if (((TabItem)tabControl.Items[tabIndex]).Tag?.ToString() == "addTab")
                        {
                            tabControl.SelectedIndex = 0;
                        }
                    }
                }
            }
        }
        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                // GetParent も null 返す可能性があるので対策
                DependencyObject? parent = VisualTreeHelper.GetParent(current);
                parent ??= LogicalTreeHelper.GetParent(current);

                current = parent;
            }

            return null;
        }
        public void AddTab(object sender, RoutedEventArgs e) // 新しいタブを追加
        {
            var newTab = new TabItem
            {
                Style = (Style)this.FindResource("NewTab"),
                Content = new TextBlock { Text = "This is a new tab." }
            };
            tabControl.Items.Insert(tabControl.Items.Count - 1, newTab);
            tabControl.SelectedItem = newTab;

        }
        public void AddWindow(object sender, RoutedEventArgs e)
        {
            var newTab = new TabItem
            {
                Style = (Style)this.FindResource("NewTab"),
                Content = new TextBlock { Text = "VideoDownloader" }
            };
            tabControl.Items.Insert(tabControl.Items.Count - 1, newTab);
            tabControl.SelectedItem = newTab;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) //キー操作
        {
            bool isControlTab = e.Key == Key.Tab && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            bool isControlShiftTab = e.Key == Key.Tab && e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

            if (isControlTab || isControlShiftTab)
            {
                // 次のタブへ（正数で循環するようにする）
                int switchDirection = (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)) ? -1 : 1;
                int nextIndex = (tabControl.SelectedIndex + switchDirection + tabControl.Items.Count) % tabControl.Items.Count;

                // addTabをスキップ
                if (((TabItem)tabControl.Items[nextIndex]).Tag?.ToString() == "addTab")
                {
                    nextIndex = (tabControl.SelectedIndex + switchDirection + tabControl.Items.Count) % tabControl.Items.Count;
                }

                tabControl.SelectedIndex = nextIndex;
                // Ctrl+Tabのデフォルト動作をキャンセル
                e.Handled = true;
            }

        }
    }
}
