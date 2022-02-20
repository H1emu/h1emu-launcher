using System.Media;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    class CustomMessageBox
    {
        public static bool result = false;

        public static void Show(string text)
        {
            MsgBox newBox = new MsgBox();
            newBox.text.Text = text;
            SystemSounds.Beep.Play();
            newBox.ShowDialog();
        }

        public static MessageBoxResult ShowResult(string text)
        {
            CnfmBox cnfmBox = new CnfmBox();
            cnfmBox.text.Text = text;
            SystemSounds.Beep.Play();
            cnfmBox.ShowDialog();

            if (result)
            {
                return MessageBoxResult.Yes;
            }
            else
            {
                return MessageBoxResult.No;
            }
        }

        public static MessageBoxResult AddServer()
        {
            AddServer addServer = new AddServer();

            if (!string.IsNullOrEmpty(Launcher.newServerName) || !string.IsNullOrEmpty(Launcher.newServerIp))
            {
                H1EmuLauncher.AddServer.addServerInstance.serverNameBox.Text = Launcher.newServerName;
                H1EmuLauncher.AddServer.addServerInstance.serverIpBox.Text = Launcher.newServerIp;
                H1EmuLauncher.AddServer.addServerInstance.serverNameHint.Visibility = Visibility.Hidden;
                H1EmuLauncher.AddServer.addServerInstance.serverIpHint.Visibility = Visibility.Hidden;
            }

            addServer.ShowDialog();

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