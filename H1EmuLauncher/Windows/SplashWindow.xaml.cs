using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class SplashWindow : Window
    {
        public static SplashWindow splashInstance;
        public static HttpClient httpClient = new();
        private Version latestVersion;
        private Version localVersion;

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
            await CheckVersion();
        }

        private async Task CheckVersion()
        {
            try
            {
                // Download launcher information from GitHub endpoint
                HttpResponseMessage response = await httpClient.GetAsync(Info.LAUNCHER_JSON_API);

                // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                // Get latest release number and date published for app.
                string jsonLauncher = await response.Content.ReadAsStringAsync();
                JsonEndPoints.Launcher.Root jsonLauncherDes = JsonSerializer.Deserialize<JsonEndPoints.Launcher.Root>(jsonLauncher);

                latestVersion = new(jsonLauncherDes.tag_name.Substring(1));
                localVersion = new(Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.'));
                UpdateWindow.installerDownloadUrl = jsonLauncherDes.assets[0].browser_download_url;
                UpdateWindow.installerFileName = jsonLauncherDes.assets[0].name;

                Close();
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

                Hide();
                CustomMessageBox.Show($"{FindResource("item66")} {FindResource("item16")}{exceptionList}\n\n{FindResource("item49")}", this);
                Close();
            }
            catch (Exception ex)
            {
                Hide();
                CustomMessageBox.Show($"{FindResource("item66")} \"{ex.Message}\"\n\n{FindResource("item49")}", this);
                Close();
            }
        }

        private void MoveSplashScreenWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SplashScreenWindowContentRendered(object sender, System.EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
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
                if (Properties.Settings.Default.firstTimeUse || Properties.Settings.Default.agreedToTOSIteration < Info.AGREED_TO_TOS_ITERATION)
                {
                    DisclaimerWindow dc = new();

                    if (!Properties.Settings.Default.firstTimeUse && Properties.Settings.Default.agreedToTOSIteration < Info.AGREED_TO_TOS_ITERATION)
                        dc.welcomeMessage.Visibility = Visibility.Collapsed;

                    dc.ShowDialog();
                }

                LauncherWindow lw = new();
                lw.Show();
            }

            splashInstance = null;
        }
    }
}