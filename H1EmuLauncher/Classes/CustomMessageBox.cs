using System.Media;
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

        public static MessageBoxResult AddServer(Window owner)
        {
            AddServerWindow addServer = new();

            if (owner != null)
                addServer.Owner = owner;
            else
                addServer.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(LauncherWindow.newServerName) || !string.IsNullOrEmpty(LauncherWindow.newServerIp))
            {
                AddServerWindow.addServerInstance.serverNameBox.Text = LauncherWindow.newServerName;
                AddServerWindow.addServerInstance.serverIpBox.Text = LauncherWindow.newServerIp;
                AddServerWindow.addServerInstance.serverNameHint.Visibility = Visibility.Hidden;
                AddServerWindow.addServerInstance.serverIpHint.Visibility = Visibility.Hidden;
            }

            addServer.ShowDialog();

            if (owner != null)
                owner.Activate();

            LauncherWindow.newServerName = addServer.serverNameBox.Text;
            LauncherWindow.newServerIp = addServer.serverIpBox.Text;

            return buttonPressed;
        }
    }
}