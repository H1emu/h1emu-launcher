using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for gameinfo.xaml
    /// </summary>

    public partial class DownloadStatus : Page
    {
        public static DownloadStatus downStatus;

        public DownloadStatus()
        {
            InitializeComponent();
            downStatus = this;
            currentDowloadLabel.Text = LanCtrler.GetWords("Currently downloading:");
            versionLabel.Text = LanCtrler.GetWords("version");
            cancelDownloadButton.Content = LanCtrler.GetWords("CANCEL");
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = CustomMessageBox.ShowResult(string.Format(LanCtrler.GetWords("Are you sure you would like to cancel the download of H1Z1: Just Survive version {0}?\n\n(Files will not be deleted)"), Login.version));
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                ContentDownloader.tokenSource.Cancel();
            }
        }
    }
}