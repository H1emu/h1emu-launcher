using H1EMU_Launcher.Resources;
using System;
using System.Windows;
using System.Windows.Input;

namespace H1EMU_Launcher
{
    public partial class MsgBox : Window
    {
        public MsgBox()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
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

        private void MoveMessageBox(object sender, MouseButtonEventArgs e)
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