using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EMU_Redux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

#pragma warning disable SYSLIB0014 // Warning saying that WebClient is discontinued and not supported anymore.

        Splash sp = new Splash();

        public MainWindow()
        {
            InitializeComponent();

            sp.Show();
            CheckVersion();
        }

        public void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "d-fens HttpClient");
                string jsonApp = wc.DownloadString("https://api.github.com/repos/H1emu/h1emu-launcher/releases/latest");

                WebClient wc2 = new WebClient();
                wc2.Headers.Add("User-Agent", "d-fens HttpClient");
                string jsonServer = wc2.DownloadString("https://api.github.com/repos/QuentinGruber/h1z1-server/releases/latest");

                // Get latest release number and date published for app
                var jsonDesApp = JsonConvert.DeserializeObject<dynamic>(jsonApp);
                string raw = jsonDesApp.tag_name;
                string online = raw.Substring(1);
                string local = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');

                // Get latest release number and date published for server
                var jsonDesServer = JsonConvert.DeserializeObject<dynamic>(jsonServer);
                string onlineServer = jsonDesServer.tag_name;
                string dateExactServer = jsonDesServer.published_at;
                string patchNotesServer = jsonDesServer.body;

                // If there is an internet connection it gets the most recent notes
                Launcher.latestUpdateVersionServer = onlineServer;
                Launcher.recentDateServer = dateExactServer;
                Launcher.patchNotes = patchNotesServer;

                // Store the latest server version, date and patch notes in the case of no internet
                Properties.Settings.Default.latestServerVersion = onlineServer;
                Properties.Settings.Default.publishDate = dateExactServer;
                Properties.Settings.Default.patchNotes = patchNotesServer;
                Properties.Settings.Default.Save();

                if (local == online)
                {
                    sp.Close();

                    Launcher la = new Launcher();
                    la.Show();

                    this.Close();
                }
                else
                {
                    sp.Close();

                    DoubleAnimation fadeAnimation = new DoubleAnimation();
                    fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
                    fadeAnimation.From = 0.0d;
                    fadeAnimation.To = 1.0d;
                    MainUpdateWindow.BeginAnimation(OpacityProperty, fadeAnimation);
                }
            }
            catch (Exception e) when (e.Message == "No such host is known. (api.github.com:443)")
            {
                sp.Close();

                CustomMessageBox.Show($"Unable to retrieve GitHub information: \"{e.Message}\"\n\n(Are you connected to the internet?)");

                Launcher la = new Launcher();
                la.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                sp.Close();

                CustomMessageBox.Show($"Unable to retrieve GitHub information: \"{ex.Message}\"");

                Launcher la = new Launcher();
                la.Show();

                this.Close();
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/H1emu/h1emu-launcher/releases/latest/download/H1Emu.Launcher.exe",
                UseShellExecute = true
            });

            this.Close();
        }

        private void MainUpdateWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainUpdateWindow.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainUpdateWindow.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            e.Cancel = false;
        }

        private void NotNowClick(object sender, RoutedEventArgs e)
        {
            Launcher la = new Launcher();
            la.Show();
            this.Close();
        }

        private void CloseUpdater(object sender, RoutedEventArgs e)
        {
            Launcher la = new Launcher();
            la.Show();
            this.Close();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainUpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
        }
    }
}
