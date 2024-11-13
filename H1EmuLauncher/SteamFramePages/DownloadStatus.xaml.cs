using System;
using System.Windows;
using System.Windows.Controls;
using H1emuLauncher;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFramePages
{
    public partial class DownloadStatus : UserControl
    {
        public static DownloadStatus downloadStatusInstance;

        public DownloadStatus()
        {
            InitializeComponent();
            downloadStatusInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = CustomMessageBox.Show($"{FindResource("item33")} {Login.version}?{FindResource("item138").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine)}", LauncherWindow.launcherInstance, true, true, false, false);
            if (dr == MessageBoxResult.Yes)
                ContentDownloader.tokenSource.Cancel();
        }

        private void StatusLoaded(object sender, RoutedEventArgs e)
        {
            gameDownloadText.Text = $"{Login.version}:";
        }
    }
}