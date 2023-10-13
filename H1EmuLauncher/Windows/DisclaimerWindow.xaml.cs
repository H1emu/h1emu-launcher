using System;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseDisclaimer(object sender, RoutedEventArgs e)
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
        }

        private void DisclaimerClosed(object sender, EventArgs e)
        {
            if (timer.IsEnabled)
                Environment.Exit(0);

            LauncherWindow.launcherInstance.Show();
        }

        public bool IsCompleted = false;

        private void DisclaimerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                e.Cancel = true;
                Storyboard sb = FindResource("CloseDisclaimer") as Storyboard;

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