using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void SplashScreenMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SplashScreenActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }
    }
}