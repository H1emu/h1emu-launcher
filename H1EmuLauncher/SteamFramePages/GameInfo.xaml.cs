using System.Threading;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFramePages
{
    public partial class GameInfo : UserControl
    {
        public GameInfo()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void Button2015(object sender, RoutedEventArgs e)
        {
            Login.gameInfo = "-app 295110 -depot 295111 -manifest 1930886153446950288";
            Login.tokenSource.Cancel();
        }

        private void Button2016(object sender, RoutedEventArgs e)
        {
            Login.gameInfo = "-app 295110 -depot 295111 -manifest 8395659676467739522";
            Login.tokenSource.Cancel();
        }
    }
}