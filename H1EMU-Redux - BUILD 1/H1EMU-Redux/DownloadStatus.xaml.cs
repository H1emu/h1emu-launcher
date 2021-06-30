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

namespace H1EMU_Redux
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
        }

        private void CancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = CustomMessageBox.ShowResult($"Are you sure you would like to cancel the download of H1Z1: Just Survive version {Login.version}?\n\n(Files will not be deleted)");
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                ContentDownloader.tokenSource.Cancel();
            }
        }
    }
}