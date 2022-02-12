using System;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFrame
{
    public partial class DownloadComplete : Page
    {
        public DownloadComplete()
        {
            InitializeComponent();

            Resources.MergedDictionaries.Clear();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void BackToLoginPage(object sender, RoutedEventArgs e)
        {
            Launcher.launcherInstance.SteamFramePanel.Navigate(new Uri("..\\SteamFrame\\Login.xaml", UriKind.Relative));
        }
    }
}