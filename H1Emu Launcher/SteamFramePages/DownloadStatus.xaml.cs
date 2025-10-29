using System;
using System.Windows;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher.SteamFramePages
{
    public partial class DownloadStatus
    {
        public static DownloadStatus downloadStatusInstance;

        public DownloadStatus()
        {
            InitializeComponent();
            downloadStatusInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = CustomMessageBox.Show($"{FindResource("item33")} {Login.version}?{FindResource("item138").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine)}", LauncherWindow.launcherInstance, false, true, true);
            if (mbr == MessageBoxResult.Yes)
                ContentDownloader.tokenSource.Cancel();
        }
    }
}