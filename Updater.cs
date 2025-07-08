using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace volotools
{

    public static class Updater
    {

        private static readonly string versionURL = "https://raw.githubusercontent.com/aestasO/volotools/master/NewestVersion.json";
        private static readonly string ytdlpVersionURL = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";

        public static async Task CheckForUpdatesAsync()
        {
            try
            {
                // ソフトのバージョン情報を取得
                string json = File.ReadAllText("Version.json");
                JObject versionInfo = JObject.Parse(json);

                // 一時フォルダ削除
                string tempDir = Path.Combine(Path.GetTempPath(), "VoloTools");
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deleting temp folder failed: {ex.Message}");
                }

                // アップデートを確認
                string updaterArguments = $"\"{AppDomain.CurrentDomain.BaseDirectory}\"";
                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                string volotoolsURL = "", ytdlpURL = "";
                // volotools
                json = await client.GetStringAsync(versionURL);
                var newestVersionInfo = JObject.Parse(json);
                if (new Version((string)newestVersionInfo["Version"]) > new Version((string)versionInfo["Volotools"]))
                {
                    volotoolsURL = (string)newestVersionInfo["URL"]; 
                }
                // ytdlp
                json = await client.GetStringAsync(ytdlpVersionURL);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var tag_name = root.GetProperty("tag_name").GetString();
                if (DateTime.ParseExact(tag_name, "yyyy.M.d", null) > DateTime.ParseExact((string)versionInfo["Ytdlp"], "yyyy.M.d", null))
                {                
                    var fileName = "yt-dlp.exe";
                    var assets = root.GetProperty("assets").EnumerateArray();
                    var exeAsset = assets.FirstOrDefault(a =>
                        a.GetProperty("name").GetString().Equals(fileName, StringComparison.OrdinalIgnoreCase));
                    ytdlpURL = exeAsset.GetProperty("browser_download_url").GetString();
                }

                // Updeter.exeを起動
                if (updaterArguments.Length > 0)
                {
                    // Updater.exe を一時ディレクトリにコピー
                    Directory.CreateDirectory(tempDir);
                    string updaterFileName = "Updater.exe";
                    string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updaterFileName);
                    string tempUpdaterPath = Path.Combine(tempDir, updaterFileName);
                    File.Copy(sourcePath, tempUpdaterPath, true);
                    // 管理者権限でUpdater.exeを起動
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempUpdaterPath,
                        Arguments = $"\"{AppDomain.CurrentDomain.BaseDirectory}\" \"{volotoolsURL}\" \"{ytdlpURL}\"",
                        Verb = "runas"
                    });
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Update check failed:: {ex.Message}");
            }
        }
    }
}