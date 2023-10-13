using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace H1EmuLauncher.SteamFrame
{
    public partial class _2FA : UserControl
    {
        public static Storyboard loadingAnimation;
        public static int twoFacInstruction;

        public _2FA()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(Classes.SetLanguageFile.LoadFile());

            loadingAnimation = FindResource("LoadingIconAnimation") as Storyboard;

            if (twoFacInstruction == 1)
                twoFacInstructionText.Text = FindResource("item78").ToString();
            else
                twoFacInstructionText.Text = FindResource("item79").ToString();
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

        public void PassAuthCode()
        {
            if (string.IsNullOrEmpty(authBox.Text.Trim()))
            {
                Classes.CustomMessageBox.Show(FindResource("item19").ToString(), LauncherWindow.launcherInstance);
                return;
            }

            twoFAButton.Visibility = Visibility.Hidden;
            loadingIcon.Visibility = Visibility.Visible;
            loadingAnimation.Begin();

            Steam3Session.twoauth = authBox.Text.Trim();
            Steam3Session.tokenSource.Cancel();
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