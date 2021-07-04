using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H1EMU_Redux
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
    }
}