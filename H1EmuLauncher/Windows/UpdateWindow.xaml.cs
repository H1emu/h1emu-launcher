using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2.CDN;

namespace H1EmuLauncher
{
    public partial class UpdateWindow : Window
    {
        SplashWindow sp = new();
        string downloadUrl;
        public static string downloadFileName;
        public static HttpClient httpClient = new();

        public UpdateWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            httpClient.DefaultRequestHeaders.Add("User-Agent", "d-fens HttpClient");

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
                    HttpResponseMessage result = httpClient.GetAsync(new Uri(Info.LAUNCHER_JSON_API)).Result;
                    string jsonLauncher = result.Content.ReadAsStringAsync().Result;

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
                catch (AggregateException e) when (e.InnerException is HttpRequestException ex)
                {
                    if (ex.StatusCode == null)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            sp.Close();

                            CustomMessageBox.Show($"{FindResource("item66").ToString().Replace("{0}", $"\"{ex.Message}\".").Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}")}{FindResource("item137").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}")}", this);

                            Topmost = true;
                            Close();
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();

                        CustomMessageBox.Show(FindResource("item66").ToString().Replace("{0}", $"\"{ex.Message}\".").Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);

                        Topmost = true;
                        Close();
                    }));
                }

            }).Start();
        }

        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            downloadSetupProgressText.Text = $"{FindResource("item54")} 0%";
            progressBarGrid.Visibility = Visibility.Visible;
            notNowText.Visibility = Visibility.Collapsed;
            notNowHyperlink.Foreground = new SolidColorBrush(Colors.Gray);
            notNowHyperlink.IsEnabled = false;
            updateButton.IsEnabled = false;
            closeButton.IsEnabled = false;

            new Thread(() =>
            {
                try
                {
                    if (File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}"))
                        File.Delete($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}");

                    using (HttpResponseMessage response = httpClient.GetAsync(new Uri(downloadUrl)).Result)
                    {
                        using (Stream contentStream = response.Content.ReadAsStream(), fs = new FileStream($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, false))
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                downloadSetupProgress.Maximum = contentStream.Length;
                            }));

                            long totalRead = 0L;
                            long totalReads = 0L;
                            byte[] buffer = new byte[8192];
                            bool isMoreToRead = true;

                            do
                            {
                                int read = contentStream.Read(buffer, 0, buffer.Length);

                                if (read == 0)
                                    isMoreToRead = false;
                                else
                                {
                                    fs.Write(buffer, 0, read);

                                    totalRead += read;
                                    totalReads += 1;

                                    if (totalRead % 100 == 0)
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            downloadSetupProgress.Value = totalRead;
                                            downloadSetupProgressText.Text = $"{FindResource("item54")} {(float)totalRead / (float)contentStream.Length * 100:0.00}%";
                                        }));
                                    }
                                }
                            }
                            while (isMoreToRead);
                        }
                    }

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

                Dispatcher.Invoke(new Action(delegate
                {
                    progressBarGrid.Visibility = Visibility.Collapsed;
                    notNowText.Visibility = Visibility.Visible;
                    notNowHyperlink.Foreground = new SolidColorBrush(Colors.White);
                    notNowHyperlink.IsEnabled = true;
                    updateButton.IsEnabled = true;
                    closeButton.IsEnabled = true;
                }));

                Environment.Exit(0);
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
            Environment.Exit(0);
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
            Hide();

            LauncherWindow la = new();
            la.Show();
        }

        private void MainUpdateWindowActivated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void NotNowHyperLinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            notNowHyperlink.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }
}