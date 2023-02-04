using System;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class DisclaimerWindow : Window
    {
        public int seconds = 10;
        public DispatcherTimer timer;

        public DisclaimerWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.firstTimeUse = 1;
            Properties.Settings.Default.Save();

            Topmost = true;
            Close();
        }

        private void ContinueButton(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.firstTimeUse = 1;
            Properties.Settings.Default.Save();

            Topmost = true;
            Close();
        }

        private void MoveDisclaimer(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void DisclaimerActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private void timerTick(object sender, EventArgs e)
        {
            seconds--;

            if (seconds == 0) 
            { 
                timer.Stop(); 
                continueButton.Content = FindResource("item167").ToString();
                continueButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
            }
            else
                continueButton.Content = seconds.ToString();
        }

        private void DisclaimerLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            continueButton.Content = seconds.ToString();

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

        private void DisclaimerClosed(object sender, EventArgs e)
        {
            if (timer.IsEnabled)
                Environment.Exit(0);
        }
    }
}