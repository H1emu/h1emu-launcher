using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
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