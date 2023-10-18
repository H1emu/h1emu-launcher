using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AddServerWindow : Window
    {
        public static AddServerWindow addServerInstance;
        public Storyboard UnfocusPropertiesAnimationShow;
        public Storyboard UnfocusPropertiesAnimationHide;

        public AddServerWindow()
        {
            InitializeComponent();
            addServerInstance = this;
            Owner = LauncherWindow.launcherInstance;

            UnfocusPropertiesAnimationShow = FindResource("UnfocusPropertiesShow") as Storyboard;
            UnfocusPropertiesAnimationHide = FindResource("UnfocusPropertiesHide") as Storyboard;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void AddServerTopBar(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ServerIpBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CustomMessageBox.buttonPressed = MessageBoxResult.OK;
                Close();
            }
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverNameBox.Text) || string.IsNullOrEmpty(serverIpBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString(), this);
                return;
            }

            if (serverNameBox.Text.Trim() == FindResource("item139").ToString() || serverNameBox.Text.Trim() == FindResource("item140").ToString() || serverNameBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString(), this);
                return;
            }

            List<LauncherWindow.ServerList> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.serverJsonFile));

            foreach (var item in currentjson)
            {
                if (item.CustomServerName == serverNameBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item143").ToString(), this);
                    return;
                }
            }

            CustomMessageBox.buttonPressed = MessageBoxResult.OK;
            Topmost = true;
            Close();
        }

        private void CloseAddServer(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.Cancel;
            Topmost = true;
            Close();
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.Cancel;
            Topmost = true;
            Close();
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

        private void AddServerLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();
        }

        private void AddServerContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void AddServerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();
        }
    }
}