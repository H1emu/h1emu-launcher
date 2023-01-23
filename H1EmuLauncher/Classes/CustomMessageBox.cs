using System.Media;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    class CustomMessageBox
    {
        public static bool result = false;

        public static void Show(string text, Window owner = null)
        {
            MessageBoxWindow newBox = new();
            newBox.text.Text = text;

            if (owner != null && owner.IsVisible)
                newBox.Owner = owner;
            else
                newBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            newBox.ShowDialog();

            if (owner != null)
                owner.Activate();
        }

        public static MessageBoxResult ShowResult(string text, Window owner = null)
        {
            ConfirmBoxWindow cnfmBox = new();
            cnfmBox.text.Text = text;

            if (owner != null)
                cnfmBox.Owner = owner;
            else
                cnfmBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SystemSounds.Beep.Play();
            cnfmBox.ShowDialog();

            if (owner != null)
                owner.Activate();

            if (result)
            {
                return MessageBoxResult.Yes;
            }
            else
            {
                return MessageBoxResult.No;
            }
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

            if (result)
            {
                LauncherWindow.newServerName = addServer.serverNameBox.Text;
                LauncherWindow.newServerIp = addServer.serverIpBox.Text;

                return MessageBoxResult.OK;
            }
            else
            {
                LauncherWindow.newServerName = null;
                LauncherWindow.newServerIp = null;

                return MessageBoxResult.No;
            }
        }
    }
}