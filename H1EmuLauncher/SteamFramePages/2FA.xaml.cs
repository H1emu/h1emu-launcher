using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFramePages
{
    public partial class _2FA : UserControl
    {
        public static Storyboard loadingAnimation;
        public static int twoFacInstruction;
        public static string code;

        public _2FA()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Add(Classes.SetLanguageFile.LoadFile());

            loadingAnimation = FindResource("LoadingIconAnimation") as Storyboard;

            switch (twoFacInstruction)
            {
                case 1:
                    _2FACodeScreen.Visibility = Visibility.Visible;
                    _2FAAppScreen.Visibility = Visibility.Collapsed;
                    twoFacInstructionText.Text = FindResource("item78").ToString();
                    break;
                case 2:
                    _2FACodeScreen.Visibility = Visibility.Visible;
                    _2FAAppScreen.Visibility = Visibility.Collapsed;
                    twoFacInstructionText.Text = FindResource("item79").ToString();
                    break;
                case 3:
                    _2FACodeScreen.Visibility = Visibility.Collapsed;
                    _2FAAppScreen.Visibility = Visibility.Visible;
                    twoFacInstructionText.Text = FindResource("item81").ToString();
                    break;
            }
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
                CustomMessageBox.Show(FindResource("item19").ToString(), LauncherWindow.launcherInstance);
                return;
            }

            twoFAButton.Visibility = Visibility.Hidden;
            loadingIcon.Visibility = Visibility.Visible;
            loadingAnimation.Begin();

            code = authBox.Text.Trim();
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