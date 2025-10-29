using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher.SettingsPages
{
    public partial class AccountKey : Page
    {
        public static AccountKey accountKeyInstance;

        public AccountKey()
        {
            InitializeComponent();
            accountKeyInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void GenerateAccountKeyButtonClick(object sender, RoutedEventArgs e)
        {
            accountKeyBoxPassword.Password = AccountKeyUtil.GenerateNewAccountKey();
        }

        private void AccountKeyBoxTextTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(accountKeyBoxText.Text))
            {
                accountKeyBoxHint.Visibility = Visibility.Hidden;
                ToggleGenerateButtonVisibility(Visibility.Collapsed);
            }
            else
                ToggleGenerateButtonVisibility(Visibility.Visible);

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
            {
                accountKeyBoxHint.Visibility = Visibility.Hidden;
                ToggleGenerateButtonVisibility(Visibility.Collapsed);
            }
            else
                ToggleGenerateButtonVisibility(Visibility.Visible);

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
                generateAccountKeyButtonRow.Visibility = Visibility.Visible;
            else
                generateAccountKeyButtonRow.Visibility = Visibility.Collapsed;
        }

        private void ToggleKeyVisibility(object sender, RoutedEventArgs e)
        {
            System.Windows.Shapes.Path toggleKeyVisibilityButtonPath = new()
            {
                Stretch = Stretch.Uniform
            };

            Binding bindingToggleKeyVisibilityButton = new("Foreground")
            {
                Source = toggleKeyVisibilityButton,
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(toggleKeyVisibilityButtonPath, System.Windows.Shapes.Path.FillProperty, bindingToggleKeyVisibilityButton);

            if (accountKeyBoxPassword.Visibility == Visibility.Visible)
            {
                accountKeyBoxPassword.Visibility = Visibility.Hidden;
                accountKeyBoxText.Visibility = Visibility.Visible;
                toggleKeyVisibilityButtonPath.Data = (PathGeometry)FindResource("EyeHideIcon");
            }
            else
            {
                accountKeyBoxPassword.Visibility = Visibility.Visible;
                accountKeyBoxText.Visibility = Visibility.Hidden;
                toggleKeyVisibilityButtonPath.Data = (PathGeometry)FindResource("EyeIcon");
            }

            toggleKeyVisibilityButton.Content = toggleKeyVisibilityButtonPath;
        }

        public void ToggleGenerateButtonVisibility(Visibility visibility)
        {
            if (visibility == Visibility.Visible)
            {
                generateAccountKeyButtonRow.Visibility = Visibility.Visible;
                generateAccountKeyButtonRowContent.Measure(new Size(generateAccountKeyButtonRow.MaxWidth, generateAccountKeyButtonRow.MaxHeight));
                DoubleAnimation show = new(0, generateAccountKeyButtonRowContent.DesiredSize.Height, new Duration(TimeSpan.FromMilliseconds(150)))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };
                generateAccountKeyButtonRow.BeginAnimation(HeightProperty, show);
            }
            else
            {
                generateAccountKeyButtonRowContent.Measure(new Size(generateAccountKeyButtonRow.MaxWidth, generateAccountKeyButtonRow.MaxHeight));
                DoubleAnimation hide = new(generateAccountKeyButtonRowContent.DesiredSize.Height, 0, new Duration(TimeSpan.FromMilliseconds(150)))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn }
                };
                hide.Completed += (s, o) => generateAccountKeyButtonRow.Visibility = Visibility.Collapsed;
                generateAccountKeyButtonRow.BeginAnimation(HeightProperty, hide);
            }
        }
    }
}
