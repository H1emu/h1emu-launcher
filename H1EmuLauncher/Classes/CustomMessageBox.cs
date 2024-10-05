using System.Media;
using System.Threading;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    class CustomMessageBox
    {
        public static MessageBoxResult buttonPressed = MessageBoxResult.OK;

        public static MessageBoxResult Show(string text, Window owner = null, bool yesButtonVisibility = false, bool noButtonVisibility = false, bool terminateH1Z1ButtonVisibility = false, bool okButtonVisibility = true)
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

        public static MessageBoxResult ShowServerInstallOptions(string text, Window owner = null)
        {
            buttonPressed = MessageBoxResult.OK;
            ShowServerInstallOptions installServerFilesMessageBox = new();
            installServerFilesMessageBox.text.Text = text;

            if (owner != null && owner.IsVisible)
                installServerFilesMessageBox.Owner = owner;
            else
                installServerFilesMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            installServerFilesMessageBox.ShowDialog();

            if (owner != null)
                owner.Activate();

            return buttonPressed;
        }

        public static void AddServer(Window owner, string newServerName = null, string newServerIp = null)
        {
            buttonPressed = MessageBoxResult.OK;
            AddServerWindow addServer = new();

            if (owner != null)
                addServer.Owner = owner;
            else
                addServer.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(newServerName) || !string.IsNullOrEmpty(newServerIp))
            {
                addServer.serverNameBox.Text = newServerName;
                addServer.serverIpBox.Text = newServerIp;
                addServer.serverNameHint.Visibility = Visibility.Hidden;
                addServer.serverIpHint.Visibility = Visibility.Hidden;
            }

            addServer.Show();
            LauncherWindow.launcherInstance.launcherFade.IsHitTestVisible = true;
        }

        public static void EditServer(Window owner, int editIndex, string newServerName = null, string newServerIp = null)
        {
            buttonPressed = MessageBoxResult.OK;
            AddServerWindow editServer = new();

            if (owner != null)
                editServer.Owner = owner;
            else
                editServer.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(newServerName) || !string.IsNullOrEmpty(newServerIp))
            {
                editServer.serverNameBox.Text = newServerName;
                editServer.serverIpBox.Text = newServerIp;
                editServer.serverNameHint.Visibility = Visibility.Hidden;
                editServer.serverIpHint.Visibility = Visibility.Hidden;
            }

            editServer.saveServerButton.Content = LauncherWindow.launcherInstance.FindResource("item213").ToString();
            editServer.editIndex = editIndex;

            editServer.Show();
            LauncherWindow.launcherInstance.launcherFade.IsHitTestVisible = true;
        }
    }
}