using System.Windows;
using System.Globalization;
using volotools;
using System.Media;
using System.Security.Principal;
using System.Diagnostics;

namespace volotools
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // アップデート確認
            try
            {
                await Updater.CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update check failed: " + ex.Message);
            }

            // MainWindow 起動
            var mainWindow = new MainWindow();
            mainWindow.Show();

        }
    }
}