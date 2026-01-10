using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher
{
    public partial class SplashWindow : Window
    {
        public static SplashWindow splashInstance;
        public static HttpClient httpClient = new();
        private static Version latestVersion;
        private static Version localVersion;
        public static bool checkForUpdates = true;

        public SplashWindow()
        {
            InitializeComponent();
            splashInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            httpClient.DefaultRequestHeaders.Add("User-Agent", "d-fens HttpClient");
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        private async void SplashScreenWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (checkForUpdates)
                await CheckVersion(this);
            else
                Close();
        }

        public static async Task<bool> CheckVersion(Window owner)
        {
            try
            {
                if (owner is LauncherWindow)
                    LauncherWindow.launcherInstance.playButton.SetResourceReference(ContentProperty, "item214");

                // Download launcher information from GitHub endpoint
                HttpResponseMessage response = await httpClient.GetAsync(Info.LAUNCHER_JSON_API, HttpCompletionOption.ResponseHeadersRead);

                // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                // Get latest release number and date published for app
                string jsonLauncher = await response.Content.ReadAsStringAsync();
                JsonEndPoints.H1EmuLauncherJson.Root jsonLauncherDes = JsonSerializer.Deserialize<JsonEndPoints.H1EmuLauncherJson.Root>(jsonLauncher);

                latestVersion = new(jsonLauncherDes.tag_name.Substring(1));
                localVersion = new(Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.'));
                UpdateWindow.installerDownloadURL = jsonLauncherDes.assets[0].browser_download_url;
                UpdateWindow.installerFileName = jsonLauncherDes.assets[0].name;

                if (localVersion < latestVersion)
                {
                    owner.Hide();
                    UpdateWindow uw = new();
                    uw.ShowDialog();
                }

                if (owner is SplashWindow)
                    owner.Close();
                else if (owner is LauncherWindow)
                    LauncherWindow.launcherInstance.playButton.SetResourceReference(ContentProperty, "item217");
            }
            catch (AggregateException e)
            {
                // Add each of the exceptions to a list to display
                string exceptionList = string.Empty;
                foreach (Exception exception in e.InnerExceptions)
                    exceptionList += $"\n\n{exception.GetType().Name}: {exception.Message}";

                if (e.InnerException is HttpRequestException ex)
                {
                    if (ex.StatusCode == null)
                        exceptionList += $"\n\n{owner.FindResource("item137")}";
                }

                if (owner is SplashWindow)
                    owner.Hide();
                else if (owner is LauncherWindow)
                {
                    if (LauncherWindow.launcherInstance.serverSelector.SelectedIndex != 1)
                    {
                        LauncherWindow.launcherInstance.playButton.IsEnabled = true;
                        LauncherWindow.launcherInstance.playButton.SetResourceReference(ContentProperty, "item8");
                    }
                }

                CustomMessageBox.Show($"{owner.FindResource("item66")} {owner.FindResource("item16")}{exceptionList}\n\n{owner.FindResource("item49")}", owner);

                if (owner is SplashWindow)
                    owner.Close();

                return false;
            }
            catch (Exception ex)
            {
                if (owner is SplashWindow)
                    owner.Hide();
                else if (owner is LauncherWindow)
                {
                    if (LauncherWindow.launcherInstance.serverSelector.SelectedIndex != 1)
                    {
                        LauncherWindow.launcherInstance.playButton.IsEnabled = true;
                        LauncherWindow.launcherInstance.playButton.SetResourceReference(ContentProperty, "item8");
                    }
                }

                CustomMessageBox.Show($"{owner.FindResource("item66")} \"{ex.Message}\"\n\n{owner.FindResource("item49")}", owner);

                if (owner is SplashWindow)
                    owner.Close();

                return false;
            }

            return true;
        }

        private void MoveSplashScreenWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SplashScreenWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            if (localVersion < latestVersion)
            {
                UpdateWindow uw = new();
                uw.Show();
            }
            else
            {
                if (Properties.Settings.Default.firstTimeUse || Properties.Settings.Default.agreedToTOSIteration < Info.TOS_ITERATION)
                {
                    DisclaimerWindow dc = new();

                    if (!Properties.Settings.Default.firstTimeUse && Properties.Settings.Default.agreedToTOSIteration < Info.TOS_ITERATION)
                    {
                        dc.welcomeMessage.Visibility = Visibility.Collapsed;
                        dc.TOSHeader.Text = FindResource("item5").ToString();
                    }

                    dc.ShowDialog();
                }

                LauncherWindow lw = new();
                lw.Show();
            }

            splashInstance = null;
        }
    }
}