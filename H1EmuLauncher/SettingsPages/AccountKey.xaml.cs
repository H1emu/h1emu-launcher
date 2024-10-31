using System;
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
        }

        private void GenerateAccountKeyButtonClick(object sender, RoutedEventArgs e)
        {
            string generatedKey = string.Empty;
            Random random = new();

            for (int i = 0; i < 64; i++)
                generatedKey += Info.ALLOWED_ACCOUNT_KEY_CHARS[random.Next(Info.ALLOWED_ACCOUNT_KEY_CHARS.Length)];

            if (accountKeyBoxText.Visibility == Visibility.Visible)
                accountKeyBoxText.Text = generatedKey;
            else
                accountKeyBoxPassword.Password = generatedKey;

            generateAccountKeyButton.Visibility = Visibility.Collapsed;
        }

        private void AccountKeyBoxTextTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(accountKeyBoxText.Text))
                accountKeyBoxHint.Visibility = Visibility.Hidden;

            if (accountKeyBoxPassword.Password != accountKeyBoxText.Text)
                accountKeyBoxPassword.Password = accountKeyBoxText.Text;

            if (accountKeyBoxText.Visibility == Visibility.Visible)
            {
                Properties.Settings.Default.sessionIdKey = accountKeyBoxText.Text.Trim();
                Properties.Settings.Default.Save();
            }
        }

        private void AccountKeyBoxPasswordPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(accountKeyBoxPassword.Password))
                accountKeyBoxHint.Visibility = Visibility.Hidden;

            if (accountKeyBoxText.Text != accountKeyBoxPassword.Password)
                accountKeyBoxText.Text = accountKeyBoxPassword.Password;

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

        private void DiscordInviteLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.DISCORD_LINK);
        }

        private void DiscordInviteLinkClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.DISCORD_LINK,
                UseShellExecute = true
            });
        }

        private void AccountKeyLoaded(object sender, RoutedEventArgs e)
        {
            accountKeyBoxPassword.Password = Properties.Settings.Default.sessionIdKey;

            if (string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                generateAccountKeyButton.Visibility = Visibility.Visible;
        }

        private void ShowKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Hidden;
            hideKeyButton.Visibility = Visibility.Visible;
            accountKeyBoxPassword.Visibility = Visibility.Hidden;
            accountKeyBoxText.Visibility = Visibility.Visible;
        }

        private void HideKey(object sender, RoutedEventArgs e)
        {
            showKeyButton.Visibility = Visibility.Visible;
            hideKeyButton.Visibility = Visibility.Hidden;
            accountKeyBoxPassword.Visibility = Visibility.Visible;
            accountKeyBoxText.Visibility = Visibility.Hidden;
        }
    }
}
