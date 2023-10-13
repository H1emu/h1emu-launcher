using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseAboutPage(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void MainAboutLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();
        }

        public bool IsCompleted = false;

        private void MainAboutClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();

                e.Cancel = true;
                Storyboard sb = FindResource("CloseAbout") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, o) =>
                    {
                        IsCompleted = true;
                        Close();
                    };

                    sb.Begin();
                }
            }
        }
    }
}