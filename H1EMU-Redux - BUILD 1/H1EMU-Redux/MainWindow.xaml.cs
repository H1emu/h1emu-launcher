using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 0.0d;
            fadeAnimation.To = 1.0d;
            MainUpdateWindow.BeginAnimation(OpacityProperty, fadeAnimation);

            CheckVersion();
        }

        public void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "d-fens HttpClient");
                string json = wc.DownloadString("https://api.github.com/repos/H1emu/H1emu-server-app/releases/latest");

                var jsonDes = JsonConvert.DeserializeObject<dynamic>(json);
                string raw = jsonDes.tag_name;
                string online = raw.Substring(1);
                string dateExact = jsonDes.published_at;
                Launcher.latestUpdateVersion = online;
                Launcher.recentDate = dateExact;
                Properties.Settings.Default.latestServerVersion = online;
                Properties.Settings.Default.publishDate = dateExact;
                Properties.Settings.Default.Save();

                string local = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');

                if (local == online)
                {
                    Launcher la = new Launcher();
                    la.Show();
                    this.Close();
                }
            }
            catch (Exception e) when (e.Message == "No such host is known. (api.github.com:443)")
            {
                CustomMessageBox.Show($"Unable to retrieve GitHub information: {e.Message}\n\nAre you connected to the internet?");

                Launcher la = new Launcher();
                la.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Unable to retrieve GitHub information: {ex.Message}");

                Launcher la = new Launcher();
                la.Show();
                this.Close();
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/H1emu/H1emu-server-app/releases/latest/download/H1emu.exe",
                UseShellExecute = true
            });
        }

        public void CloseForm()
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainUpdateWindow.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainUpdateWindow.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            Launcher la = new Launcher();
            la.Show();
            this.Close();
        }

        private void NotNowClick(object sender, RoutedEventArgs e)
        {
            CloseForm();
        }

        private void CloseUpdater(object sender, RoutedEventArgs e)
        {
            CloseForm();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
