using H1EMU_Launcher.Resources;
using System;
using System.Windows;
using System.Windows.Controls;

namespace H1EMU_Launcher
{
    public partial class DownloadStatus : Page
    {
        public static DownloadStatus downStatus;

        public DownloadStatus()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            downStatus = this;
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = CustomMessageBox.ShowResult($"{FindResource("item33")} {Login.version}?{FindResource("item138").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine)}");

            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                ContentDownloader.tokenSource.Cancel();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            gameDownloadText.Text = $"{Login.version}:";
        }
    }
}