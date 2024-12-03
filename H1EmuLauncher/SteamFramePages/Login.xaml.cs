using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SteamFramePages
{
    public partial class Login : UserControl
    {
        private static Storyboard loadingAnimation;
        private static CancellationToken token;
        public static CancellationTokenSource tokenSource = new();
        public static string gameInfo = "-app 295110 -depot 295111 -manifest 8395659676467739522";

        public Login()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            loadingAnimation = FindResource("LoadingIconAnimation") as Storyboard;
        }

        public void UpdateLang()
        {
            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void PasswordBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password))
                {
                    CustomMessageBox.Show(FindResource("item36").ToString(), LauncherWindow.launcherInstance);
                    return;
                }

                new Thread(() =>
                {
                    TryLoginDownload();
                }).Start();
            }
        }

        private void UsernameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password))
                {
                    CustomMessageBox.Show(FindResource("item36").ToString(), LauncherWindow.launcherInstance);
                    return;
                }

                new Thread(() =>
                {
                    TryLoginDownload();
                }).Start();
            }
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password))
            {
                CustomMessageBox.Show(FindResource("item36").ToString(), LauncherWindow.launcherInstance);
                return;
            }

            new Thread(() =>
            {
                TryLoginDownload();
            }).Start();
        }

        public static string version = string.Empty;
        public async void TryLoginDownload()
        {
            string username = null;
            string password = null;

            string[] args = gameInfo.Split(' ');

            uint appId = GetParameter(args, "-app", ContentDownloader.INVALID_APP_ID);
            List<uint> depotIdList = GetParameterList<uint>(args, "-depot");
            List<ulong> manifestIdList = GetParameterList<ulong>(args, "-manifest");

            List<(uint, ulong)> depotManifestIds = new List<(uint, ulong)>();
            var zippedDepotManifest = depotIdList.Zip(manifestIdList, (depotId, manifestId) => (depotId, manifestId));
            depotManifestIds.AddRange(zippedDepotManifest);

            ContentDownloader.Config.MaxDownloads = 8;
            AccountSettingsStore.LoadFromFile("account.config");

            Dispatcher.Invoke(new Action(delegate
            {
                username = usernameBox.Text.Trim();
                password = passwordBox.Password.Trim();

                loginEnterButton.Visibility = Visibility.Hidden;
                loadingIcon.Visibility = Visibility.Visible;
                loadingAnimation.Begin();
            }));

            try
            {
                if (InitializeSteam(username, password))
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (_2FA.loadingAnimation != null)
                            _2FA.loadingAnimation.Stop();
                    }));

                SelectLocation:
                    bool result = true;

                    tokenSource.Dispose();
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;

                    System.Windows.Forms.FolderBrowserDialog selectDirectory = new();
                    selectDirectory.Description = FindResource("item51").ToString();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (selectDirectory.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            result = false;
                    }));

                    if (!result)
                        goto SelectLocation;

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (Directory.GetFileSystemEntries(selectDirectory.SelectedPath).Length != 0)
                        {
                            UpdateLang();

                            MessageBoxResult dr = CustomMessageBox.Show(FindResource("item89").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), LauncherWindow.launcherInstance, true, true, false, false);
                            if (dr != MessageBoxResult.Yes)
                                result = false;
                        }
                    }));

                    if (!result)
                        goto SelectLocation;

                    ContentDownloader.DEFAULT_DOWNLOAD_DIR = selectDirectory.SelectedPath;

                    foreach (ulong manifestId in manifestIdList)
                    {
                        if (manifestId == 8395659676467739522)
                            version = "2016";
                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\DownloadStatus.xaml", UriKind.Relative));
                    }));

                    await Task.Delay(5000);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        DownloadStatus.downloadStatusInstance.gameDownloadText.Text = $"{version}:";
                        DownloadStatus.downloadStatusInstance.downloadProgressText.Text = "Processing...";
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    }));

                    ContentDownloader.downloadSpeedTimer.Start();
                    await ContentDownloader.DownloadAppAsync(appId, depotManifestIds, "Public", null, null, null, false, false).ConfigureAwait(false);

                    Properties.Settings.Default.activeDirectory = ContentDownloader.DEFAULT_DOWNLOAD_DIR;
                    Properties.Settings.Default.Save();

                    if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\DepotDownloader"))
                        Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\DepotDownloader", true);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        UpdateLang();
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\DownloadComplete.xaml", UriKind.Relative));
                        loadingAnimation.Stop();
                        loadingIcon.Visibility = Visibility.Hidden;
                        loginEnterButton.Visibility = Visibility.Visible;
                        LauncherWindow.launcherInstance.directoryBox.Text = Properties.Settings.Default.activeDirectory;
                        LauncherWindow.launcherInstance.CheckGameVersionAndPath(LauncherWindow.launcherInstance, false, true);
                        CustomMessageBox.Show($"{FindResource("item37")} {version}.", LauncherWindow.launcherInstance);
                    }));
                }
                else
                {
                    Debug.WriteLine("Error: InitializeSteam failed");

                    Dispatcher.Invoke(new Action(delegate
                    {
                        BackToLogin();
                        if (_2FA.loadingAnimation != null)
                            _2FA.loadingAnimation.Stop();
                    }));
                }
            }
            catch (Exception e) when (e is TaskCanceledException)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    BackToLogin();
                    CustomMessageBox.Show($"{FindResource("item38")} {version}.", LauncherWindow.launcherInstance);
                }));
            }
            catch (Exception ex) when (ex is ContentDownloaderException || ex is OperationCanceledException)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    BackToLogin();
                    if (ex.Message.Contains("is not available from this account."))
                        CustomMessageBox.Show($"{FindResource("item39")} \"{ex.Message}\".\n\n{FindResource("item15")}", LauncherWindow.launcherInstance);
                    else
                        CustomMessageBox.Show($"{FindResource("item39")} \"{ex.Message}\".", LauncherWindow.launcherInstance);
                }));
            }
            catch (Exception exc)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    BackToLogin();
                    CustomMessageBox.Show($"{FindResource("item40")} \"{exc.Message}\".", LauncherWindow.launcherInstance);
                }));
            }
            finally
            {
                ContentDownloader.downloadSpeedTimer.Stop();
                ContentDownloader.downloadSpeed = 0;
                ContentDownloader.sizeDownloadedPublic = 0;
                ContentDownloader.ShutdownSteam3();
                username = null;
                password = null;
            }

            try
            {
                AccountSettingsStore.Instance.ContentServerPenalty.Clear();
                AccountSettingsStore.Instance = null;

                DepotConfigStore.Instance.InstalledManifestIDs.Clear();
                DepotConfigStore.Instance = null;
            }
            catch { }
        }

        static bool InitializeSteam(string username, string password)
        {
            if (!ContentDownloader.Config.UseQrCode)
            {
                if (username != null && password == null && (!ContentDownloader.Config.RememberPassword || !AccountSettingsStore.Instance.LoginTokens.ContainsKey(username)))
                {
                    do
                    {
                        Debug.WriteLine("Enter account password for \"{0}\": ", username);
                        if (Console.IsInputRedirected)
                        {
                            password = Console.ReadLine();
                        }
                        else
                        {
                            // Avoid console echoing of password
                            password = Util.ReadPassword();
                        }

                        Debug.WriteLine("");
                    } while (string.Empty == password);
                }
                else if (username == null)
                {
                    Debug.WriteLine("No username given. Using anonymous account with dedicated server subscription.");
                }
            }

            return ContentDownloader.InitializeSteam3(username, password);
        }

        static int IndexOfParam(string[] args, string param)
        {
            for (var x = 0; x < args.Length; ++x)
            {
                if (args[x].Equals(param, StringComparison.OrdinalIgnoreCase))
                    return x;
            }

            return -1;
        }

        public static T GetParameter<T>(string[] args, string param, T defaultValue = default)
        {
            var index = IndexOfParam(args, param);

            if (index == -1 || index == (args.Length - 1))
                return defaultValue;

            var strParam = args[index + 1];

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(strParam);
            }

            return default;
        }

        static List<T> GetParameterList<T>(string[] args, string param)
        {
            var list = new List<T>();
            var index = IndexOfParam(args, param);

            if (index == -1 || index == (args.Length - 1))
                return list;

            index++;

            while (index < args.Length)
            {
                var strParam = args[index];

                if (strParam[0] == '-') break;

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    list.Add((T)converter.ConvertFromString(strParam));
                }

                index++;
            }

            return list;
        }

        public void BackToLogin()
        {
            UpdateLang();
            LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\Login.xaml", UriKind.Relative));
            loadingAnimation.Stop();
            loadingIcon.Visibility = Visibility.Hidden;
            loginEnterButton.Visibility = Visibility.Visible;
        }

        private void UsernameBoxGotFocus(object sender, RoutedEventArgs e)
        {
            usernameHint.Visibility = Visibility.Hidden;
        }

        private void UsernameBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(usernameBox.Text))
                usernameHint.Visibility = Visibility.Visible;
        }

        private void PasswordBoxGotFocus(object sender, RoutedEventArgs e)
        {
            passwordHint.Visibility = Visibility.Hidden;
        }

        private void PasswordBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(passwordBox.Password))
                passwordHint.Visibility = Visibility.Visible;
        }
    }
}