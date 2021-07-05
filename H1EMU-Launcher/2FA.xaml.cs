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
        }
        private void _2FAKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Steam3Session.twoauth = authBox.Text;
                Steam3Session.tokenSource.Cancel();
            }
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            Steam3Session.twoauth = authBox.Text;
            Steam3Session.tokenSource.Cancel();
        }

        private void authBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(authBox.Text))
            {
                authHint.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(authBox.Text))
            {
                authHint.Visibility = Visibility.Collapsed;
            }
        }

        private void authBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(authBox.Text))
            {
                authHint.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(authBox.Text))
            {
                authHint.Visibility = Visibility.Collapsed;
            }
        }

        private void authHint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            authHint.Visibility = Visibility.Collapsed;
            authBox.Focus();
        }

        private void authBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            authHint.Visibility = Visibility.Collapsed;
        }
    }
}
