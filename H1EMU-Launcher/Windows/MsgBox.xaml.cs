using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for MsgBox.xaml
    /// </summary>

    public partial class MsgBox : Window
    {
        public MsgBox()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void OkButton(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainMsgBoxActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void MainMsgBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (AboutPage.abtpage != null)
            {
                AboutPage.abtpage.aboutPageBlur.Radius = 15;
                AboutPage.abtpage.aboutPageFade.Visibility = Visibility.Visible;
            }

            if (AddServer.addsver != null)
            {
                AddServer.addsver.addServerBlur.Radius = 15;
                AddServer.addsver.addServerFade.Visibility = Visibility.Visible;
            }

            if (Launcher.lncher != null)
            {
                Launcher.lncher.launcherBlur.Radius = 15;
                Launcher.lncher.launcherFade.Visibility = Visibility.Visible;
            }

            if (Settings.sttngs != null)
            {
                Settings.sttngs.settingsBlur.Radius = 15;
                Settings.sttngs.settingsFade.Visibility = Visibility.Visible;
            }
        }
    }
}
