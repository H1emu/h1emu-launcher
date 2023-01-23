using System;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFrame
{
    public partial class DownloadStatus : Page
    {
        public static DownloadStatus downloadStatusInstance;

        public DownloadStatus()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            downloadStatusInstance = this;
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = CustomMessageBox.ShowResult($"{FindResource("item33")} {Login.version}?{FindResource("item138").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine)}", LauncherWindow.launcherInstance);

            if (dr == MessageBoxResult.Yes)
            {
                ContentDownloader.tokenSource.Cancel();
            }
        }

        private void StatusLoaded(object sender, RoutedEventArgs e)
        {
            gameDownloadText.Text = $"{Login.version}:";
        }
    }
}