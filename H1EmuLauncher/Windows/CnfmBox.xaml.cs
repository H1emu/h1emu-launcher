using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class CnfmBox : Window
    {
        public CnfmBox()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
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
            if (AboutPage.aboutPageInstance != null)
            {
                AboutPage.aboutPageInstance.aboutPageBlur.Radius = 15;
                AboutPage.aboutPageInstance.aboutPageFade.Visibility = Visibility.Visible;
            }

            if (AddServer.addServerInstance != null)
            {
                AddServer.addServerInstance.addServerBlur.Radius = 15;
                AddServer.addServerInstance.addServerFade.Visibility = Visibility.Visible;
            }

            if (Launcher.launcherInstance != null)
            {
                Launcher.launcherInstance.launcherBlur.Radius = 15;
                Launcher.launcherInstance.launcherFade.Visibility = Visibility.Visible;
            }

            if (Settings.settingsInstance != null)
            {
                Settings.settingsInstance.settingsBlur.Radius = 15;
                Settings.settingsInstance.settingsFade.Visibility = Visibility.Visible;
            }
        }
    }
}