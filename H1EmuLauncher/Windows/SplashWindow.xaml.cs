using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class SplashWindow : Window
    {
        public static SplashWindow splashInstance;

        public SplashWindow()
        {
            InitializeComponent();
            splashInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void MoveSplashScreenWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SplashScreenWindowContentRendered(object sender, System.EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void SplashScreenWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            splashInstance = null;
        }
    }
}