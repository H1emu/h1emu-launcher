using System;
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
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class UpdateWindow : Window
    {
        public static string downloadFileName;
        public static UpdateWindow updateInstance;
        public static HttpClient httpClient = new();
        private SplashWindow sp = new();
        private Version online;
        private Version local;
        private string downloadUrl;

        public UpdateWindow()
        {
            InitializeComponent();
            updateInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            httpClient.DefaultRequestHeaders.Add("User-Agent", "d-fens HttpClient");
            httpClient.Timeout = TimeSpan.FromMinutes(5);

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
                    online = new Version(jsonLauncherDes.tag_name.Substring(1));
                    local = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.'));
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
                        Topmost = true;
                        Close();
                        CustomMessageBox.Show($"{FindResource("item66")} {FindResource("item16")}{exceptionList}\n\n{FindResource("item49")}", LauncherWindow.launcherInstance);
                    }));
                    return;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();
                        Topmost = true;
                        Close();
                        CustomMessageBox.Show($"{FindResource("item66")} \"{ex.Message}\"\n\n{FindResource("item49")}", LauncherWindow.launcherInstance);
                    }));
                    return;
                }

                if (local < online)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();
                        Show();
                        UpdateLauncher();
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        sp.Close();
                        Topmost = true;
                        Close();
                    }));
                }

            }).Start();
        }

        private void UpdateLauncher()
        {
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

                    HttpResponseMessage result = httpClient.GetAsync(new Uri(downloadUrl)).Result;

                    // Throw an exception if we didn't get the correct response, with the first letter capitalised in the message
                    if (result.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"{char.ToUpper(result.ReasonPhrase.First())}{result.ReasonPhrase.Substring(1)}");

                    Stream contentStream = result.Content.ReadAsStream();
                    FileStream fs = new($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, false);

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
                                    downloadSetupProgressText.Text = $"{FindResource("item54")} {(float)totalRead / contentStream.Length * 100:0.00}%";
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

                    contentStream.Close();
                    fs.Close();
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
                        CustomMessageBox.Show($"{FindResource("item80")} {FindResource("item16")}{exceptionList}", this);
                    }));

                    return;
                }
                catch (Exception exc)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item80")} \"{exc.Message}\".\n\n{FindResource("item64")} \"{exc.StackTrace.Trim()}\".", this);
                    }));

                    return;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{downloadFileName}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ph)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item186")} \"{ph.Message}\"\n\n{FindResource("item187")}", this);
                    }));

                    return;
                }

                Environment.Exit(0);

            }).Start();
        }

        private void MoveUpdateWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void UpdateWindowLoaded(object sender, RoutedEventArgs e)
        {
            SystemSounds.Beep.Play();
        }

        private void UpdateWindowContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseUpdateWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Environment.Exit(0);
        }

        private void UpdateWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();

            LauncherWindow lw = new();
            lw.Show();

            updateInstance = null;
        }
    }
}