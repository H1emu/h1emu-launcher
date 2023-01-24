using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFrame
{
    public partial class _2FA : Page
    {
        public static string twoFacInstruction;

        public _2FA()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            twoFacInstructionText.Text = twoFacInstruction;
        }

        public void PassAuthCode()
        {
            loadingGif.Visibility = Visibility.Visible;
            twoFAButton.Visibility = Visibility.Hidden;

            Steam3Session.twoauth = authBox.Text.Trim();
            Steam3Session.tokenSource.Cancel();
        }

        private void _2FAKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PassAuthCode();
            }
        }

        private void ContinueButton(object sender, RoutedEventArgs e)
        {
            PassAuthCode();
        }

        private void AuthBoxGotFocus(object sender, RoutedEventArgs e)
        {
            authHint.Visibility = Visibility.Hidden;
        }

        private void AuthBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(authBox.Text))
                authHint.Visibility = Visibility.Visible;
        }
    }
}