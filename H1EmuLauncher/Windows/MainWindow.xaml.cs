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
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class MainWindow : Window
    {
        Splash sp = new();
        string downloadUrl;
        public static string downloadFileName;

        public MainWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            sp.Show();
            CheckVersion();
        }

        public void CheckVersion()
        {
            new Thread(() =>
            {
                try
                {
                    // Download launcher information from GitHub endpoint
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    WebClient wc = new();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                    wc.Headers.Add("User-Agent", "d-fens HttpClient");
                    string jsonLauncher = wc.DownloadString(new Uri(Info.LAUNCHER_JSON_API));

                    // Get latest release number and date published for app.
                    var jsonDesLauncher = JsonConvert.DeserializeObject<dynamic>(jsonLauncher);
                    string online = $"{jsonDesLauncher.tag_name}".Substring(1);
                    string local = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');
                    downloadUrl = jsonDesLauncher.assets[0].browser_download_url;
                    downloadFileName = jsonDesLauncher.assets[0].name;

                    if (local == online)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            sp.Close();

                            Topmost = true;
                            Close();
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            sp.Close();

                            Show();
                        }));
                    }
                }
                catch (Exception e) when (e.Message == "No such host is known. (api.github.com:443)")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();

                        CustomMessageBox.Show(FindResource("item66").ToString() + $" \"{e.Message}\"." + FindResource("item137").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), this);

                        Topmost = true;
                        Close();
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();

                        CustomMessageBox.Show(FindResource("item66").ToString().Replace("{0}", $"\"{ex.Message}\".").Replace("\\n\\n", Environment.NewLine + Environment.NewLine), this);

                        Topmost = true;
                        Close();
                    }));
                }

            }).Start();
        }

        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.Gray);
            notNow.IsEnabled = false;
            updateButton.IsEnabled = false;
            closeButton.IsEnabled = false;

            new Thread(() =>
            {
                try
                {
                    ManualResetEvent ma = new(false);

#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    WebClient wc = new();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                    wc.DownloadProgressChanged += (s, e) =>
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            downloadSetupProgress.Value = e.ProgressPercentage;
                        }));
                    };
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        ma.Set();
                    };

                    wc.DownloadFileAsync(new Uri(downloadUrl), $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}");
                    ma.WaitOne();

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", this);
                    }));
                }

                Environment.Exit(69);

            }).Start();
        }

        private void NotNowClick(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void CloseUpdater(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainUpdateWindowLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
        }

        private void MainUpdateWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.White);
            Hide();

            Launcher la = new();
            la.Show();
        }

        private void NotNowHyperLinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            notNow.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void MainUpdateWindowActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }
    }
}