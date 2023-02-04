using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void OkButton(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void MoveMessageBox(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainMsgBoxActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private void MainMsgBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (AboutPageWindow.aboutPageInstance != null)
            {
                AboutPageWindow.aboutPageInstance.aboutPageBlur.Radius = 15;
                AboutPageWindow.aboutPageInstance.aboutPageFade.Visibility = Visibility.Visible;
            }

            if (AddServerWindow.addServerInstance != null)
            {
                AddServerWindow.addServerInstance.addServerBlur.Radius = 15;
                AddServerWindow.addServerInstance.addServerFade.Visibility = Visibility.Visible;
            }

            if (LauncherWindow.launcherInstance != null)
            {
                LauncherWindow.launcherInstance.launcherBlur.Radius = 15;
                LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Visible;
            }

            if (SettingsWindow.settingsInstance != null)
            {
                SettingsWindow.settingsInstance.settingsBlur.Radius = 15;
                SettingsWindow.settingsInstance.settingsFade.Visibility = Visibility.Visible;
            }
        }
    }
}