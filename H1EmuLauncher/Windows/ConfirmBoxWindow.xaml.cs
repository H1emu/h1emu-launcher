using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class ConfirmBoxWindow : Window
    {
        public ConfirmBoxWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            Topmost = true;
            Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            Topmost = true;
            Close();
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            Topmost = true;
            Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainCnfmBoxActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private void MainCnfmBoxLoaded(object sender, RoutedEventArgs e)
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