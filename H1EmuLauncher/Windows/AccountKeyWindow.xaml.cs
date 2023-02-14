using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AccountKeyWindow : Window
    {
        public static AccountKeyWindow accountKeyInstance;

        public AccountKeyWindow()
        {
            InitializeComponent();
            accountKeyInstance = this;
            Owner = SettingsWindow.settingsInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            accountKeyHyperlink.Text = Info.ACCOUNT_KEY_LINK;
        }

        private void CloseAccountKeyWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void OkCloseAccountKeyWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void AccountKeyDragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AccountKeyGotFocus(object sender, RoutedEventArgs e)
        {
            accountKeyHint.Visibility = Visibility.Hidden;
        }

        private void AccountKeyLostFocus(object sender, RoutedEventArgs e)
        {
            if (accountKeyBox.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(accountKeyBox.Password))
                    accountKeyHint.Visibility = Visibility.Visible;
            }
            else
            {
                if (string.IsNullOrEmpty(accountKeyBoxShown.Text))
                    accountKeyHint.Visibility = Visibility.Visible;
            }
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
            SettingsWindow.settingsInstance.UnfocusPropertiesAnimationShow.Begin();

            if (!SettingsWindow.launchAccountKeyWindow)
                accountKeyBox.Password = Properties.Settings.Default.sessionIdKey;

            if (!string.IsNullOrEmpty(accountKeyBox.Password))
                accountKeyHint.Visibility = Visibility.Hidden;
        }

        public bool IsCompleted = false;

        private void AccountKeyWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                if (accountKeyBox.Visibility == Visibility.Hidden)
                    accountKeyBox.Password = accountKeyBoxShown.Text;

                Properties.Settings.Default.sessionIdKey = accountKeyBox.Password.Trim();
                Properties.Settings.Default.Save();

                SettingsWindow.settingsInstance.UnfocusPropertiesAnimationHide.Begin();

                e.Cancel = true;
                Storyboard sb = FindResource("CloseAccountKey") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, _) =>
                    {
                        IsCompleted = true;
                        Close();
                    };

                    sb.Begin();
                }
            }
        }

        private void ShowKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Hidden;
            hideKeyButton.Visibility = Visibility.Visible;
            accountKeyBoxShown.Text = accountKeyBox.Password;

            accountKeyBox.Visibility = Visibility.Hidden;
            accountKeyBoxShown.Visibility = Visibility.Visible;
        }

        private void HideKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Visible;
            hideKeyButton.Visibility = Visibility.Hidden;
            accountKeyBox.Password = accountKeyBoxShown.Text;
            accountKeyBoxShown.Text = null;

            accountKeyBox.Visibility = Visibility.Visible;
            accountKeyBoxShown.Visibility = Visibility.Hidden;
        }
    }
}