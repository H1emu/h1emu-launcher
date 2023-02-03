using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AddServerWindow : Window
    {
        public static AddServerWindow addServerInstance;

        public AddServerWindow()
        {
            InitializeComponent();
            addServerInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void AddServerTopBar(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ServerIpBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CustomMessageBox.result = true;
                this.Close();
            }
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverNameBox.Text) || string.IsNullOrEmpty(serverIpBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString());
                return;
            }

            if (serverNameBox.Text.Trim() == FindResource("item139").ToString() || serverNameBox.Text.Trim() == FindResource("item140").ToString() || serverNameBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString());
                return;
            }

            List<LauncherWindow.ServerList> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.serverJsonFile));

            foreach (var item in currentjson)
            {
                if (item.SName == serverNameBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item143").ToString());
                    return;
                }
            }

            CustomMessageBox.result = true;
            this.Topmost = true;
            this.Close();
        }

        private void CloseAddServerMenu(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void ServerNameGotFocus(object sender, RoutedEventArgs e)
        {
            serverNameHint.Visibility = Visibility.Hidden;
        }

        private void ServerNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverNameBox.Text))
                serverNameHint.Visibility = Visibility.Visible;
        }

        private void ServerIPGotFocus(object sender, RoutedEventArgs e)
        {
            serverIpHint.Visibility = Visibility.Hidden;
        }

        private void ServerIPLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverIpBox.Text))
                serverIpHint.Visibility = Visibility.Visible;
        }

        private void AddServerMenuLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 15;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Visible;
        }

        private void AddServerMenuClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 0;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void AddServerMenuActivated(object sender, EventArgs e)
        {
            addServerBlur.Radius = 0;
            addServerFade.Visibility = Visibility.Hidden;
        }
    }
}