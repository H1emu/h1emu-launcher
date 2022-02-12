using System;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void SplashScreenMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SplashScreenActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}