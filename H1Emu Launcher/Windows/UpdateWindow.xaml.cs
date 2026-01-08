using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher
{
    public partial class UpdateWindow : Window
    {
        public static UpdateWindow updateInstance;
        public static string installerDownloadURL;
        public static string installerFileName;

        public UpdateWindow()
        {
            InitializeComponent();
            updateInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private async void UpdateWindowLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
            await UpdateLauncher();
        }

        private async Task UpdateLauncher()
        {
            try
            {
                downloadSetupProgress.IsIndeterminate = true;

                // Delete any old installation files if they exist in case of corruption
                if (File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{installerFileName}"))
                    File.Delete($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{installerFileName}");

                HttpResponseMessage response = await SplashWindow.httpClient.GetAsync(installerDownloadURL, HttpCompletionOption.ResponseHeadersRead);
                // Throw an exception if we didn't get the correct response, with the first letter capitalised in the message
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                downloadSetupProgress.IsIndeterminate = false;

                long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using Stream contentStream = await response.Content.ReadAsStreamAsync();
                using FileStream fileStream = new($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{installerFileName}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                byte[] buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                {
                    // Write the data to the file
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;

                    // Update the progress bar
                    if (totalBytes > 0)
                    {
                        float progressPercentage = (float)totalBytesRead * 100 / totalBytes;
                        downloadSetupProgress.Value = progressPercentage;
                        downloadSetupProgressText.Text = $"{FindResource("item54")} {progressPercentage:0.00}%";
                    }
                }
            }
            catch (AggregateException e)
            {
                string exceptionList = string.Empty;
                foreach (Exception exception in e.InnerExceptions)
                    exceptionList += $"\n\n{exception.GetType().Name}: {exception.Message}";

                if (e.InnerException is HttpRequestException ex)
                {
                    if (ex.StatusCode == null)
                        exceptionList += $"\n\n{FindResource("item137")}";
                }

                CustomMessageBox.Show($"{FindResource("item80").ToString().Replace(":", ".").Replace("：", ".")} {FindResource("item16")}{exceptionList}", this);
                return;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item80")} \"{ex.Message}\".", this);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{installerFileName}",
                    UseShellExecute = true
                });
            }
            catch (Exception ph)
            {
                CustomMessageBox.Show($"{FindResource("item186")} \"{ph.Message}\"\n\n{FindResource("item187")}", this);
                return;
            }

            Environment.Exit(0);
        }

        private void MoveUpdateWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseUpdateWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Environment.Exit(0);
        }

        private void UpdateWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateInstance = null;
        }
    }
}