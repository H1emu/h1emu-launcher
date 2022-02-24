﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AccountKey : Window
    {
        public AccountKey()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            accountKeyHyperlink.Text = Info.ACCOUNT_KEY_LINK;
        }

        private void CloseAccountKeyWindow(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void OkCloseAccountKeyWindow(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void AccountKeyDragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AccountKeyGotFocus(object sender, RoutedEventArgs e)
        {
            accountKeyHint.Visibility = Visibility.Hidden;
        }

        private void AccountKeyLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(accountKeyBox.Password))
                accountKeyHint.Visibility = Visibility.Visible;
        }

        private void AccountKeyLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.ACCOUNT_KEY_LINK);
        }

        private void UserProfileLink(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.ACCOUNT_KEY_LINK,
                UseShellExecute = true
            });
        }

        private void AccountKeyLoaded(object sender, RoutedEventArgs e)
        {
            Settings.settingsInstance.settingsBlur.Radius = 15;
            Settings.settingsInstance.settingsFade.Visibility = Visibility.Visible;

            if (!Settings.launchAccountKeyWindow)
            {
                accountKeyBox.Password = Properties.Settings.Default.sessionIdKey;
            }

            if (!string.IsNullOrEmpty(accountKeyBox.Password))
                accountKeyHint.Visibility = Visibility.Hidden;
        }

        private void AccountKeyWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.settingsInstance.settingsBlur.Radius = 0;
            Settings.settingsInstance.settingsFade.Visibility = Visibility.Hidden;

            Properties.Settings.Default.sessionIdKey = accountKeyBox.Password;
            Properties.Settings.Default.Save();
        }

        private void ShowHidePasswordClick(object sender, RoutedEventArgs e)
        {
            accountKeyBoxShown.Text = accountKeyBox.Password;

            if (accountKeyBox.Visibility == Visibility.Visible)
            {
                accountKeyBox.Visibility = Visibility.Hidden;
                accountKeyBoxShown.Visibility = Visibility.Visible;
            }
            else
            {
                accountKeyBox.Visibility = Visibility.Visible;
                accountKeyBoxShown.Visibility = Visibility.Hidden;
            }
        }
    }
}