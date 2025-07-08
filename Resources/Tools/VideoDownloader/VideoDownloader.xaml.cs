using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.IO;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace volotools.Tools
{
    public partial class VideoDownloader : System.Windows.Controls.UserControl
    {

        public class DownloadVideoClass
        {
            // ReactivePropertyで進捗を管理
            public ReactiveProperty<string> Icon { get; } = new ReactiveProperty<string>("");
            public ReactiveProperty<string> VideoTitle { get; } = new ReactiveProperty<string>("");
            public ReactiveProperty<double> Progress1 { get; } = new ReactiveProperty<double>(0);
            public ReactiveProperty<double> Progress2 { get; } = new ReactiveProperty<double>(0);
            public ReactiveProperty<string> Speed { get; } = new ReactiveProperty<string>("");
        }
        public ObservableCollection<DownloadVideoClass> DownloadVideos { get; set; }
        public VideoDownloader()
        {
            InitializeComponent();
            DownloadVideos = [];
            DataContext = this;
            Dir.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // TextBoxの内容を取得
            string url = VideoURL.Text;
            string dir = Dir.Text;

            // テキストが空でない場合のみ処理
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(dir))
            {
                // 非同期でダウンロード処理を実行
                await DownloadVideo(url,dir);
            }
        }

        public async Task DownloadVideo(string url, string outputPath)
        {
            try
            {
                // フォルダ作成
                outputPath = Path.GetFullPath(outputPath.Replace('/', '\\'));
                Directory.CreateDirectory(outputPath);

                // TextBoxをクリア
                VideoURL.Clear();

                // Itemsコレクションにデータを追加
                DownloadVideoClass downloadVideo = new() { };
                DownloadVideos.Add(downloadVideo);
                downloadVideo.VideoTitle.Value = url;
                downloadVideo.Icon.Value = "ProgressDownload";

                // タイトルを設定
                string ytDlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\yt-dlp.exe");
                string arguments = $"--get-title {url}"; // yt-dlp に渡す引数（出力先を指定）
                Process process1 = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = ytDlpPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process1.OutputDataReceived += (sender, e) =>
                {
                    // 標準出力から動画タイトルを取得
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        downloadVideo.VideoTitle.Value = SanitizeFileName(e.Data);
                    }
                };
                process1.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // エラーメッセージの処理
                        Console.WriteLine("Error: " + e.Data);
                    }
                };

                process1.Start();
                process1.BeginOutputReadLine();
                process1.BeginErrorReadLine();

                // 結果を待機（非同期処理を同期的に待つ）
                await Task.Run(() =>
                {
                    process1.WaitForExit();
                });

                // ユーザーのダウンロードフォルダパスを取得
                string tempPath = Path.GetTempPath();
                var outputFilePath = $"{tempPath}{downloadVideo.VideoTitle.Value}.mp4";

                // 出力ファイル名のパターンを指定（一時フォルダ内）
                arguments = $"-f \"bestaudio[ext=m4a]+bestvideo[ext=mp4]\" --merge-output-format mp4 -o \"{outputFilePath}\" {url}"; // yt-dlp に渡す引数（出力先を指定）

                Process process = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = ytDlpPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {

                        // UIスレッドでProgressBarを更新
                        this.Dispatcher.Invoke(() =>
                        {
                            // //進捗を抽出
                            var progressPer = Regex.Matches(e.Data, @"(\d+(\.\d+)?)%");
                            downloadVideo.Speed.Value = Regex.Match(e.Data, @"\d+(\.\d+)?MiB/s").Groups[0].Value;
                            if (progressPer.Count > 0)
                            {

                                var progress = (int)Math.Floor(double.Parse(progressPer[0].Value.TrimEnd('%')));
                                if (downloadVideo.Progress1.Value != 100)
                                {
                                    downloadVideo.Progress1.Value = progress;
                                }
                                else if (progress != 100)
                                {
                                    downloadVideo.Progress2.Value = progress;
                                }
                                else if (downloadVideo.Progress2.Value != 0)
                                {
                                    downloadVideo.Progress2.Value = 100;
                                }
                            }
                        });
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // エラーメッセージの処理
                        Console.WriteLine("Error: " + e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());  // 非同期でプロセス終了を待機

                if (File.Exists(outputFilePath))
                {
                    File.SetLastWriteTime(outputFilePath, DateTime.Now);
                    
                    MoveFileWithRename(outputFilePath, outputPath);
                }
                else
                {
                    throw new InvalidOperationException("動画を保存できませんでした");
                }
                downloadVideo.Icon.Value = "Check";

            }
            catch (Exception ex)
            {
                // エラーハンドリング
                System.Windows.MessageBox.Show($"Error downloading video: {ex.Message}");
            }
        }
        public static void MoveFileWithRename(string sourcePath, string destinationDir)
        {
            // フォルダがなければ作成
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 元ファイル名と拡張子を取得
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string extension = Path.GetExtension(sourcePath);
            string destinationPath = Path.Combine(destinationDir, fileName + extension);

            int count = 1;

            // 重複がある場合は (1), (2), ... をファイル名に追加
            while (File.Exists(destinationPath))
            {
                string newFileName = $"{fileName} ({count}){extension}";
                destinationPath = Path.Combine(destinationDir, newFileName);
                count++;
            }

            // 移動実行
            File.Move(sourcePath, destinationPath);
        }
        public static string SanitizeFileName(string input)
        {
            // ファイル名に使用できない文字を置換
            string invalidChars = new(Path.GetInvalidFileNameChars());
            string sanitized = input;

            // 各無効文字を空文字に置き換える
            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c.ToString(), string.Empty);
            }

            // 先頭と末尾の空白やピリオドを削除
            sanitized = sanitized.TrimStart().TrimEnd('.');

            return sanitized;
        }

        private void DirSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Dir.Text = dialog.SelectedPath;
            }
        }
    }
}
