using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SettingsPages
{
    public partial class AccountKey : Page
    {
        public static AccountKey accountKeyInstance;

        public AccountKey()
        {
            InitializeComponent();
            accountKeyInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            accountKeyHyperlink.Text = Info.ACCOUNT_KEY_LINK;
        }

        private void AccountKeyBoxTextTextChanged(object sender, TextChangedEventArgs e)
        {
            if (accountKeyBoxText.Visibility == Visibility.Visible)
            {
                Properties.Settings.Default.sessionIdKey = accountKeyBoxText.Text.Trim();
                Properties.Settings.Default.Save();
            }
        }

        private void AccountKeyBoxPasswordPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (accountKeyBoxPassword.Visibility == Visibility.Visible)
            {
                Properties.Settings.Default.sessionIdKey = accountKeyBoxPassword.Password.Trim();
                Properties.Settings.Default.Save();
            }
        }

        private void AccountKeyBoxGotFocus(object sender, RoutedEventArgs e)
        {
            accountKeyBoxHint.Visibility = Visibility.Hidden;
        }

        private void AccountKeyBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(accountKeyBoxPassword.Password) && string.IsNullOrEmpty(accountKeyBoxText.Text))
                accountKeyBoxHint.Visibility = Visibility.Visible;
        }

        private void AccountKeyLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.ACCOUNT_KEY_LINK);
        }

        private void UserProfileLinkClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.ACCOUNT_KEY_LINK,
                UseShellExecute = true
            });
        }

        private void AccountKeyLoaded(object sender, RoutedEventArgs e)
        {
            if (!SettingsWindow.openAccountKeyPage)
                accountKeyBoxPassword.Password = Properties.Settings.Default.sessionIdKey;

            if (!string.IsNullOrEmpty(accountKeyBoxPassword.Password))
                accountKeyBoxHint.Visibility = Visibility.Hidden;
        }

        private void ShowKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Hidden;
            hideKeyButton.Visibility = Visibility.Visible;
            accountKeyBoxText.Text = accountKeyBoxPassword.Password;

            accountKeyBoxPassword.Visibility = Visibility.Hidden;
            accountKeyBoxText.Visibility = Visibility.Visible;
        }

        private void HideKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Visible;
            hideKeyButton.Visibility = Visibility.Hidden;
            accountKeyBoxPassword.Password = accountKeyBoxText.Text;

            accountKeyBoxPassword.Visibility = Visibility.Visible;
            accountKeyBoxText.Visibility = Visibility.Hidden;
        }
    }
}
