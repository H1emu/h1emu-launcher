using System;
using System.Media;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    class CustomMessageBox
    {
        public static MessageBoxResult buttonPressed = MessageBoxResult.OK;

        public static MessageBoxResult Show(string text, Window owner = null, bool okButtonVisibility = true, bool yesButtonVisibility = false, bool noButtonVisibility = false, bool terminateH1Z1ButtonVisibility = false, bool discordLinkVisibility = false)
        {
            buttonPressed = MessageBoxResult.OK;
            MessageBoxWindow messageBox = new();
            messageBox.text.Text = text;

            if (yesButtonVisibility)
                messageBox.confirmYesButton.Visibility = Visibility.Visible;

            if (noButtonVisibility)
                messageBox.confirmNoButton.Visibility = Visibility.Visible;

            if (terminateH1Z1ButtonVisibility)
                messageBox.killH1Z1Button.Visibility = Visibility.Visible;

            if (!okButtonVisibility)
                messageBox.okButton.Visibility = Visibility.Collapsed;

            if (discordLinkVisibility)
                messageBox.discordInviteLink.Visibility = Visibility.Visible;

            if (owner != null && owner.IsVisible)
                messageBox.Owner = owner;
            else
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            messageBox.ShowDialog();

            if (owner != null)
                owner.Activate();

            return buttonPressed;
        }

        public static MessageBoxResult InstallServerInline(string text, Window owner = null)
        {
            buttonPressed = MessageBoxResult.OK;
            InstallServerInline installServerFilesMessageBox = new();
            installServerFilesMessageBox.text.Text = text;

            if (owner != null && owner.IsVisible)
            {
                installServerFilesMessageBox.Owner = owner;
            }
            else
                installServerFilesMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            installServerFilesMessageBox.ShowDialog();

            if (owner != null)
                owner.Activate();

            return buttonPressed;
        }
    }
}