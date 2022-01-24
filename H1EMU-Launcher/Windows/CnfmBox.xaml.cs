using H1EMU_Launcher.Resources;
using System;
using System.Windows;
using System.Windows.Input;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for CnfmBox.xaml
    /// </summary>

    public partial class CnfmBox : Window
    {
        public CnfmBox()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            this.Topmost = true;
            this.Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainCnfmBoxActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void MainCnfmBoxLoaded(object sender, RoutedEventArgs e)
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