using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class DisclaimerWindow : Window
    {
        public static DisclaimerWindow disclaimerInstance;

        public DisclaimerWindow()
        {
            InitializeComponent();
            disclaimerInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void ContinueButton(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.firstTimeUse = false;
            Properties.Settings.Default.agreedToTOSIteration = Info.AGREED_TO_TOS_ITERATION;
            Properties.Settings.Default.Save();

            Topmost = true;
            Close();
        }

        private void MoveDisclaimerWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void DisclaimerWindowLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
        }

        private void DisclaimerWindowContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseDisclaimerWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void DisclaimerWindowClosed(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.firstTimeUse || Properties.Settings.Default.agreedToTOSIteration < Info.AGREED_TO_TOS_ITERATION)
                Environment.Exit(0);

            disclaimerInstance = null;
        }

        private void AgreedToRulesCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if ((bool)checkBox.IsChecked)
                continueButton.IsEnabled = true;
            else
                continueButton.IsEnabled = false;
        }
    }
}