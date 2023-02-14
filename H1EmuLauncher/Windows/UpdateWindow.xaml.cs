using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class UpdateWindow : Window
    {
        SplashWindow sp = new();
        string online;
        string local;
        string downloadUrl;
        public static string downloadFileName;
        public static HttpClient httpClient = new();

        public UpdateWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
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

                    // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                    if (result.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"{char.ToUpper(result.ReasonPhrase.First())}{result.ReasonPhrase.Substring(1)}");

                    // Get latest release number and date published for app.
                    string jsonLauncher = result.Content.ReadAsStringAsync().Result;
                    JsonEndPoints.Launcher.Root jsonLauncherDes = JsonSerializer.Deserialize<JsonEndPoints.Launcher.Root>(jsonLauncher);
                    online = jsonLauncherDes.tag_name.Substring(1);
                    local = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');
                    downloadUrl = jsonLauncherDes.assets[0].browser_download_url;
                    downloadFileName = jsonLauncherDes.assets[0].name;
                }
                catch (AggregateException e)
                {
                    string exceptionList = string.Empty;
                    foreach (Exception exception in e.InnerExceptions)
                        exceptionList += $"\n\n{exception.GetType().Name}: {exception.Message}";

                    if (e.InnerException is HttpRequestException ex)
                    {
                        if (ex.StatusCode == null)
                            exceptionList += $"\n\n{FindResource("item137")}";

                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();

                        CustomMessageBox.Show($"{FindResource("item66")} {FindResource("item16")}{exceptionList}\n\n{FindResource("item49")}", this);

                        Topmost = true;
                        Close();
                    }));

                    return;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();

                        CustomMessageBox.Show($"{FindResource("item66")} \"{ex.Message}\"\n\n{FindResource("item49")}", this);

                        Topmost = true;
                        Close();
                    }));

                    return;
                }

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

                    Dispatcher.Invoke(new Action(delegate
                    {
                        downloadSetupProgress.IsIndeterminate = true;
                    }));

                    using (HttpResponseMessage result = httpClient.GetAsync(new Uri(downloadUrl)).Result)
                    {
                        // Throw an exception if we didn't get the correct response, with the first letter capitalised in the message
                        if (result.StatusCode != HttpStatusCode.OK)
                            throw new Exception($"{char.ToUpper(result.ReasonPhrase.First())}{result.ReasonPhrase.Substring(1)}");

                        using (Stream contentStream = result.Content.ReadAsStream(), fs = new FileStream($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, false))
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                downloadSetupProgress.IsIndeterminate = false;
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
                                    else if (totalRead == contentStream.Length)
                                    {
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            downloadSetupProgress.Value = downloadSetupProgress.Maximum;
                                            downloadSetupProgressText.Text = $"{FindResource("item54")} 100%";
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
                catch (AggregateException ex)
                {
                    string exceptionList = string.Empty;
                    foreach (Exception exception in ex.InnerExceptions)
                        exceptionList += $"\n\n{exception}";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item16")}{exceptionList}\n\n{FindResource("item49")}", this);
                    }));
                }
                catch (Exception es)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item142")} {es.Message}", this);
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
                    Environment.Exit(0);
                }));

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

        public bool IsCompleted = false;

        private void MainUpdateWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                e.Cancel = true;
                Storyboard sb = FindResource("CloseUpdate") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, _) =>
                    {
                        IsCompleted = true;
                        Close();
                    };

                    sb.Begin();
                }
            }
            else
            {
                Hide();

                LauncherWindow la = new();
                la.Show();
            }
        }
    }
}