using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AddServer : Window
    {
        public static AddServer addServerInstance;

        public AddServer()
        {
            InitializeComponent();
            addServerInstance = this;
            Owner = Launcher.launcherInstance;

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
            Launcher.launcherInstance.launcherBlur.Radius = 15;
            Launcher.launcherInstance.launcherFade.Visibility = Visibility.Visible;
        }

        private void AddServerMenuClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Launcher.launcherInstance.launcherBlur.Radius = 0;
            Launcher.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void AddServerMenuActivated(object sender, EventArgs e)
        {
            addServerBlur.Radius = 0;
            addServerFade.Visibility = Visibility.Hidden;
        }
    }
}