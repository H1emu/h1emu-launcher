using H1EMU_Launcher.Resources;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Media;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace H1EMU_Launcher
{
    public partial class MainWindow : Window
    {

#pragma warning disable SYSLIB0014 // Warning saying that WebClient is discontinued and not supported anymore.

        string downloadUrl;
        public static string downloadFileName;

        Splash sp = new Splash();

        public MainWindow()
        {
            InitializeComponent();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            sp.Show();
            CheckVersion();
        }

        public void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "d-fens HttpClient");
                string jsonLauncher = wc.DownloadString(new Uri(Classes.Info.LAUNCHER_JSON_API));

                // Get latest release number and date published for app.

                var jsonDesLauncher = JsonConvert.DeserializeObject<dynamic>(jsonLauncher);
                string online = $"{jsonDesLauncher.tag_name}".Substring(1);
                string local = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');
                downloadUrl = jsonDesLauncher.assets[0].browser_download_url;
                downloadFileName = jsonDesLauncher.assets[0].name;

                if (local == online)
                {
                    sp.Close();

                    this.Topmost = true;
                    this.Close();
                }
                else
                {
                    sp.Close();
                }
            }
            catch (Exception e) when (e.Message == "No such host is known. (api.github.com:443)")
            {
                sp.Close();

                CustomMessageBox.Show(FindResource("item66").ToString() + $" \"{e.Message}\"." + FindResource("item137").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));

                this.Topmost = true;
                this.Close();
            }
            catch (Exception ex)
            {
                sp.Close();

                CustomMessageBox.Show(FindResource("item66").ToString() + $" \"{ex.Message}\".");

                this.Topmost = true;
                this.Close();
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.Gray);
            notNow.IsEnabled = false;
            updateButton.IsEnabled = false;
            closeButton.IsEnabled = false;

            new Thread(() =>
            {
                try
                {
                    ManualResetEvent ma = new ManualResetEvent(false);

                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += (s, e) =>
                    {
                        Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            downloadSetupProgress.Value = e.ProgressPercentage;
                        });
                    };
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        ma.Set();
                    };

                    wc.DownloadFileAsync(new Uri(downloadUrl), $"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}");
                    ma.WaitOne();

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item142").ToString().Replace("{0}", ex.Message));
                    });
                }

                Environment.Exit(69);

            }).Start();
        }

        private void NotNowClick(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void CloseUpdater(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainUpdateWindowLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
        }

        private void MainUpdateWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.White);
            this.Hide();

            Launcher la = new Launcher();
            la.Show();
        }

        private void NotNowHyperLinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void MainUpdateWindowActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}