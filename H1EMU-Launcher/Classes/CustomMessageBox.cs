using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H1EMU_Launcher
{
    class CustomMessageBox
    {
        public static bool result = false;
        public static string newServerName;
        public static string newServerIp;

        public static void Show(string text)
        {
            MsgBox newBox = new MsgBox();
            newBox.text.Text = text;
            SystemSounds.Beep.Play();
            newBox.ShowDialog();
        }

        public static DialogResult ShowResult(string text)
        {
            CnfmBox cnfmBox = new CnfmBox();
            cnfmBox.text.Text = text;
            SystemSounds.Beep.Play();
            cnfmBox.ShowDialog();

            if (result)
            {
                return DialogResult.Yes;
            }
            else
            {
                return DialogResult.No;
            }
        }

        public static DialogResult AddServer()
        {
            AddServer addServer = new AddServer();
            addServer.ShowDialog();

            if (result)
            {
                if (!string.IsNullOrEmpty(addServer.serverNameBox.Text) && !string.IsNullOrEmpty(addServer.serverIpBox.Text))
                {
                    newServerName = addServer.serverNameBox.Text;
                    newServerIp = addServer.serverIpBox.Text;
                }
                else
                {
                    newServerName = "";
                    newServerIp = "";
                }
                
                return DialogResult.Yes;
            }
            else
            {
                return DialogResult.No;
            }
        }
    }
}