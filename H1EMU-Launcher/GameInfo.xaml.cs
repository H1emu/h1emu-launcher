using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    public partial class GameInfo : Page
    {
        public GameInfo()
        {
            InitializeComponent();
            whichVersionLabel.Text = LanCtrler.GetWords("Which version of");
            wouldULikeLabel.Text = LanCtrler.GetWords("would you like to install?");
            jan2015Btn.Content = LanCtrler.GetWords("JANUARY 2015");
            dec2016Btn.Content = LanCtrler.GetWords("DECEMBER 2016");
        }

        private void Button2015(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Login.gameInfo = "-app 295110 -depot 295111 -manifest 1930886153446950288";
                Login.tokenSource.Cancel();

            }).Start();

        }

        private void Button2016(object sender, RoutedEventArgs e)
        {
            new Thread(() => 
            {
                Login.gameInfo = "-app 295110 -depot 295111 -manifest 8395659676467739522";
                Login.tokenSource.Cancel();

            }).Start();
        }
    }
}