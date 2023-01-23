using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AboutPageWindow : Window
    {
        public static AboutPageWindow aboutPageInstance;

        public AboutPageWindow()
        {
            InitializeComponent();
            aboutPageInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void MainAboutLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 15;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Visible;
        }

        private void MainAboutClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 0;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void MainAboutActivated(object sender, EventArgs e)
        {
            aboutPageBlur.Radius = 0;
            aboutPageFade.Visibility = Visibility.Hidden;
        }
    }
}