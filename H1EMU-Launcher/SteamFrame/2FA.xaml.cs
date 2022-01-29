using H1EMU_Launcher.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace H1EMU_Launcher
{
    public partial class _2FA : Page
    {
        public _2FA()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void _2FAKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loadingGif.Visibility = Visibility.Visible;
                twoFAButton.Visibility = Visibility.Hidden;

                Steam3Session.twoauth = authBox.Text;
                Steam3Session.tokenSource.Cancel();
            }
        }

        private void ContinueButton(object sender, RoutedEventArgs e)
        {
            loadingGif.Visibility = Visibility.Visible;
            twoFAButton.Visibility = Visibility.Hidden;

            Steam3Session.twoauth = authBox.Text;
            Steam3Session.tokenSource.Cancel();
        }

        private void AuthBoxGotFocus(object sender, RoutedEventArgs e)
        {
            authHint.Visibility = Visibility.Hidden;
        }

        private void AuthBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(authBox.Text)) { authHint.Visibility = Visibility.Visible; }
        }
    }
}