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
    /// Interaction logic for DownloadComplete.xaml
    /// </summary>

    public partial class DownloadComplete : Page
    {
        public DownloadComplete()
        {
            InitializeComponent();
            downloadCompletelabel.Text = LanCtrler.GetWords("Download complete!");
            downTip.Text = LanCtrler.GetWords("Need to download a different version? Press the button below to login again!");
            loginBtn.Content = LanCtrler.GetWords("LOGIN");
        }

        private void BackToLoginPage(object sender, RoutedEventArgs e)
        {
            Launcher.lncher.SteamFrame.Navigate(new Uri("Login.xaml", UriKind.Relative));
        }
    }
}
