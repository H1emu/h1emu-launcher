﻿using System.Threading;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFrame
{
    public partial class GameInfo : Page
    {
        public GameInfo()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void Button2015(object sender, RoutedEventArgs e)
        {
            Resources.MergedDictionaries.Clear();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            new Thread(() =>
            {
                Login.gameInfo = "-app 295110 -depot 295111 -manifest 1930886153446950288";
                Login.tokenSource.Cancel();

            }).Start();
        }

        private void Button2016(object sender, RoutedEventArgs e)
        {
            Resources.MergedDictionaries.Clear();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            new Thread(() => 
            {
                Login.gameInfo = "-app 295110 -depot 295111 -manifest 8395659676467739522";
                Login.tokenSource.Cancel();

            }).Start();
        }
    }
}