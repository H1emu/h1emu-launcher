using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for _2FA.xaml
    /// </summary>

    public partial class _2FA : Page
    {
        public _2FA()
        {
            InitializeComponent();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageFile.SetLanguageCode();

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
