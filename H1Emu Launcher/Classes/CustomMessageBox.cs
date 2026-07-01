using System.Media;
using System.Windows;

namespace H1Emu_Launcher.Classes
{
    class CustomMessageBox
    {
        public static MessageBoxResult buttonPressed = MessageBoxResult.OK;

        public static MessageBoxResult Show(string text, Window owner = null, bool okButtonVisibility = true, bool yesButtonVisibility = false, bool noButtonVisibility = false, bool terminateH1Z1ButtonVisibility = false, bool discordLinkVisibility = false)
        {
            buttonPressed = MessageBoxResult.OK;
            MessageBoxWindow customMessageBox = new();
            customMessageBox.text.Text = text;

            if (yesButtonVisibility)
                customMessageBox.confirmYesButton.Visibility = Visibility.Visible;

            if (noButtonVisibility)
                customMessageBox.confirmNoButton.Visibility = Visibility.Visible;

            if (terminateH1Z1ButtonVisibility)
                customMessageBox.killH1Z1Button.Visibility = Visibility.Visible;

            if (!okButtonVisibility)
                customMessageBox.okButton.Visibility = Visibility.Collapsed;

            if (discordLinkVisibility)
                customMessageBox.discordInviteLink.Visibility = Visibility.Visible;

            if (owner != null && owner.IsVisible)
                customMessageBox.Owner = owner;
            else
                customMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            customMessageBox.ShowDialog();

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