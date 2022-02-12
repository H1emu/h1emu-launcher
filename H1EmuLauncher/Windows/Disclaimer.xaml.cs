using System;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class Disclaimer : Window
    {
        public Disclaimer()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.firstTimeUse = 1;
            Properties.Settings.Default.Save();

            this.Topmost = true;
            this.Close();
        }

        private void ContinueButton(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.firstTimeUse = 1;
            Properties.Settings.Default.Save();

            this.Topmost = true;
            this.Close();
        }

        private void MoveDisclaimer(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void DisclaimerActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        public int seconds = 10;
        public DispatcherTimer timer;

        private void timerTick(object sender, EventArgs e)
        {
            seconds--;

            if (seconds == -1) 
            { 
                timer.Stop(); 
                continueButton.Content = FindResource("item167").ToString();
                continueButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
            }
            else { continueButton.Content = seconds.ToString(); }
        }

        private void DisclaimerLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            continueButton.Content = seconds.ToString();

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