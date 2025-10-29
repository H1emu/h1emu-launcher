using System;
using System.Windows;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher.SteamFramePages
{
    public partial class DownloadComplete
    {
        public DownloadComplete()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void BackToLoginPage(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\Login.xaml", UriKind.Relative));
        }
    }
}