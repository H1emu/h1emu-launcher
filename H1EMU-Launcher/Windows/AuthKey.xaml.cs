using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for AuthKey.xaml
    /// </summary>
    public partial class AuthKey : Window
    {
        public AuthKey()
        {
            InitializeComponent();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageFile.SetLanguageCode();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseAuthKeyWindow(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void OkCloseAuthKeyWindow(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void AuthKeyDragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AuthKeyGotFocus(object sender, RoutedEventArgs e)
        {
            authKeyHint.Visibility = Visibility.Hidden;
        }

        private void AuthKeyLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(authKeyBox.Password)) { authKeyHint.Visibility = Visibility.Visible; }
        }

        private void UserProfileLink(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.h1emu.com/us/cockpit/my/security-launcher/",
                UseShellExecute = true
            });
        }

        private void AuthKeyLoaded(object sender, RoutedEventArgs e)
        {
            Settings.sttngs.settingsBlur.Radius = 15;
            Settings.sttngs.settingsFade.Visibility = Visibility.Visible;

            authKeyBox.Password = Properties.Settings.Default.sessionIdKey;

            if (!string.IsNullOrEmpty(authKeyBox.Password)) { authKeyHint.Visibility = Visibility.Hidden; }
        }

        private void AuthKeyWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.sttngs.settingsBlur.Radius = 0;
            Settings.sttngs.settingsFade.Visibility = Visibility.Hidden;

            Properties.Settings.Default.sessionIdKey = authKeyBox.Password;
            Properties.Settings.Default.Save();
        }

        private void ShowHidePasswordClick(object sender, RoutedEventArgs e)
        {
            authKeyBoxShown.Text = authKeyBox.Password;

            if (authKeyBox.Visibility == Visibility.Visible)
            {
                authKeyBox.Visibility = Visibility.Hidden;
                authKeyBoxShown.Visibility = Visibility.Visible;
            }
            else
            {
                authKeyBox.Visibility = Visibility.Visible;
                authKeyBoxShown.Visibility = Visibility.Hidden;
            }
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            authKeyBlur.Radius = 0;
            authKeyFade.Visibility = Visibility.Hidden;
        }
    }
}
