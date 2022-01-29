using H1EMU_Launcher.Resources;
using System;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace H1EMU_Launcher
{
    public partial class Disclaimer : Window
    {
        public Disclaimer()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
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
        public System.Windows.Forms.Timer timer;

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

            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = 1000;
            timer.Start();

            continueButton.Content = seconds.ToString();

            if (AboutPage.abtpage != null)
            {
                AboutPage.abtpage.aboutPageBlur.Radius = 15;
                AboutPage.abtpage.aboutPageFade.Visibility = Visibility.Visible;
            }

            if (AddServer.addsver != null)
            {
                AddServer.addsver.addServerBlur.Radius = 15;
                AddServer.addsver.addServerFade.Visibility = Visibility.Visible;
            }

            if (Launcher.lncher != null)
            {
                Launcher.lncher.launcherBlur.Radius = 15;
                Launcher.lncher.launcherFade.Visibility = Visibility.Visible;
            }

            if (Settings.sttngs != null)
            {
                Settings.sttngs.settingsBlur.Radius = 15;
                Settings.sttngs.settingsFade.Visibility = Visibility.Visible;
            }
        }
    }
}