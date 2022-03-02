using System.Media;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    class CustomMessageBox
    {
        public static bool result = false;

        public static void Show(string text, Window owner = null)
        {
            MsgBox newBox = new MsgBox();
            newBox.text.Text = text;

            if (owner != null)
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
            CnfmBox cnfmBox = new CnfmBox();
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
            AddServer addServer = new AddServer();

            if (owner != null)
                addServer.Owner = owner;
            else
                addServer.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!string.IsNullOrEmpty(Launcher.newServerName) || !string.IsNullOrEmpty(Launcher.newServerIp))
            {
                H1EmuLauncher.AddServer.addServerInstance.serverNameBox.Text = Launcher.newServerName;
                H1EmuLauncher.AddServer.addServerInstance.serverIpBox.Text = Launcher.newServerIp;
                H1EmuLauncher.AddServer.addServerInstance.serverNameHint.Visibility = Visibility.Hidden;
                H1EmuLauncher.AddServer.addServerInstance.serverIpHint.Visibility = Visibility.Hidden;
            }

            addServer.ShowDialog();

            if (owner != null)
                owner.Activate();

            if (result)
            {
                Launcher.newServerName = addServer.serverNameBox.Text;
                Launcher.newServerIp = addServer.serverIpBox.Text;

                return MessageBoxResult.OK;
            }
            else
            {
                Launcher.newServerName = null;
                Launcher.newServerIp = null;

                return MessageBoxResult.No;
            }
        }
    }
}