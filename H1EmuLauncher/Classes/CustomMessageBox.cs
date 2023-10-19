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

        public static void AddServer(Window owner, string newServerName = null, string newServerIp = null)
        {
            AddServerWindow addServer = new();

            if (owner != null)
                addServer.Owner = owner;
            else
                addServer.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(newServerName) || !string.IsNullOrEmpty(newServerIp))
            {
                AddServerWindow.addServerInstance.serverNameBox.Text = newServerName;
                AddServerWindow.addServerInstance.serverIpBox.Text = newServerIp;
                AddServerWindow.addServerInstance.serverNameHint.Visibility = Visibility.Hidden;
                AddServerWindow.addServerInstance.serverIpHint.Visibility = Visibility.Hidden;
            }

            addServer.Show();
            LauncherWindow.launcherInstance.launcherFade.IsHitTestVisible = true;
        }
    }
}