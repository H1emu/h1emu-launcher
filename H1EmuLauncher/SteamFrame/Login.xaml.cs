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

namespace H1EmuLauncher.SteamFrame
{
    public partial class Login : UserControl
    {
        public static Storyboard loadingAnimation;
        public static CancellationTokenSource tokenSource = new();
        static CancellationToken token;
        public static string gameInfo { get; set; }

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
                AccountSettingsStore.LoadFromFile("account.config");
            }
            catch (Exception er)
            {
                Debug.WriteLine(er.Message);
            }

            var depotManifestIds = new List<(uint, ulong)>();

            if (InitializeSteam(username, password))
            {
                try
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\GameInfo.xaml", UriKind.Relative));
                        _2FA.loadingAnimation.Stop();
                    }));

                    bool result = true;

                SelectLocation:
                    tokenSource.Dispose();
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;

                    token.WaitHandle.WaitOne();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        System.Windows.Forms.FolderBrowserDialog folderBrowser = new();
                        folderBrowser.Description = LauncherWindow.launcherInstance.FindResource("item51").ToString();

                        if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (Directory.GetFileSystemEntries(folderBrowser.SelectedPath).Length != 0)
                            {
                                UpdateLang();

                                MessageBoxResult dr = CustomMessageBox.ShowResult(FindResource("item89").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), LauncherWindow.launcherInstance);
                                if (dr == MessageBoxResult.Yes)
                                {
                                    ContentDownloader.DEFAULT_DOWNLOAD_DIR = folderBrowser.SelectedPath;
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            else
                            {
                                ContentDownloader.DEFAULT_DOWNLOAD_DIR = folderBrowser.SelectedPath;
                                result = true;
                            }
                        }
                        else
                        {
                            result = false;
                        }
                    }));

                    if (!result)
                        goto SelectLocation;

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\DownloadStatus.xaml", UriKind.Relative));
                    }));

                    string[] args = gameInfo.Split(' ');

                    ContentDownloader.Config.MaxDownloads = 8;
                    var appId = GetParameter(args, "-app", ContentDownloader.INVALID_APP_ID);
                    var depotIdList = GetParameterList<uint>(args, "-depot");
                    var manifestIdList = GetParameterList<ulong>(args, "-manifest");

                    if (manifestIdList.Count > 0)
                    {
                        if (depotIdList.Count != manifestIdList.Count)
                        {
                            Debug.WriteLine("Error: -manifest requires one id for every -depot specified");

                            username = null;
                            password = null;

                            return;
                        }

                        var zippedDepotManifest = depotIdList.Zip(manifestIdList, (depotId, manifestId) => (depotId, manifestId));
                        depotManifestIds.AddRange(zippedDepotManifest);
                    }
                    else
                    {
                        depotManifestIds.AddRange(depotIdList.Select(depotId => (depotId, ContentDownloader.INVALID_MANIFEST_ID)));
                    }

                    foreach (ulong manifestVersion in manifestIdList)
                    {
                        if (manifestVersion == 1930886153446950288)
                        {
                            version = "2015";
                        }
                        else if (manifestVersion == 8395659676467739522)
                        {
                            version = "2016";
                        }
                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        DownloadStatus.downloadStatusInstance.gameDownloadText.Text = $"{version}:";
                        DownloadStatus.downloadStatusInstance.downloadProgressText.Text = "Processing...";
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    }));

                    await ContentDownloader.DownloadAppAsync(appId, depotManifestIds, "Public", null, null, null, false, false).ConfigureAwait(false);

                    Properties.Settings.Default.activeDirectory = ContentDownloader.DEFAULT_DOWNLOAD_DIR;
                    Properties.Settings.Default.Save();

                    if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\DepotDownloader"))
                        Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\DepotDownloader", true);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        UpdateLang();
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\DownloadComplete.xaml", UriKind.Relative));
                        loadingAnimation.Stop();
                        loadingIcon.Visibility = Visibility.Hidden;
                        loginEnterButton.Visibility = Visibility.Visible;
                        CustomMessageBox.Show($"{FindResource("item37")} {version}.", LauncherWindow.launcherInstance);
                    }));
                }
                catch (Exception ph) when (ph is TaskCanceledException)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        UpdateLang();
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\Login.xaml", UriKind.Relative));
                        loadingAnimation.Stop();
                        loadingIcon.Visibility = Visibility.Hidden;
                        loginEnterButton.Visibility = Visibility.Visible;
                        CustomMessageBox.Show($"{FindResource("item38")} {version}.", LauncherWindow.launcherInstance);
                    }));
                }
                catch (Exception ex) when (ex is ContentDownloaderException || ex is OperationCanceledException)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        UpdateLang();
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\Login.xaml", UriKind.Relative));
                        loadingAnimation.Stop();
                        loadingIcon.Visibility = Visibility.Hidden;
                        loginEnterButton.Visibility = Visibility.Visible;

                        if (ex.Message.Contains("is not available from this account."))
                            CustomMessageBox.Show($"{FindResource("item39")} \"{ex.Message}\".\n\n{FindResource("item15")}", LauncherWindow.launcherInstance);
                        else
                            CustomMessageBox.Show($"{FindResource("item39")} \"{ex.Message}\".", LauncherWindow.launcherInstance);
                    }));
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        UpdateLang();
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFrame\\Login.xaml", UriKind.Relative));
                        loadingAnimation.Stop();
                        loadingIcon.Visibility = Visibility.Hidden;
                        loginEnterButton.Visibility = Visibility.Visible;
                        CustomMessageBox.Show($"{FindResource("item40")} \"{er.Message}\".", LauncherWindow.launcherInstance);
                    }));
                }
                finally
                {
                    ContentDownloader.ShutdownSteam3();
                }
            }
            else
            {
                Debug.WriteLine("Error: InitializeSteam failed.");

                Dispatcher.Invoke(new Action(delegate
                {
                    loadingAnimation.Stop();
                    loadingIcon.Visibility = Visibility.Hidden;
                    loginEnterButton.Visibility = Visibility.Visible;
                }));
            }

            try
            {
                AccountSettingsStore.Instance.SentryData.Clear();
                AccountSettingsStore.Instance.ContentServerPenalty.Clear();
                AccountSettingsStore.Instance.LoginKeys.Clear();
                AccountSettingsStore.Instance = null;

                DepotConfigStore.Instance.InstalledManifestIDs.Clear();
                DepotConfigStore.Instance = null;
            }
            catch { }

            username = null;
            password = null;
        }

        static bool InitializeSteam(string username, string password)
        {
            if (username != null && password == null && (!ContentDownloader.Config.RememberPassword || !AccountSettingsStore.Instance.LoginKeys.ContainsKey(username)))
            {
                do
                {
                    Console.Write($"Enter account password for \"{username}\": ");
                    if (Console.IsInputRedirected)
                    {
                        password = Console.ReadLine();
                    }
                    else
                    {
                        // Avoid console echoing of password
                        password = Util.ReadPassword();
                    }
                    Console.WriteLine();
                } while (String.Empty == password);
            }
            else if (username == null)
            {
                Console.WriteLine("No username given. Using anonymous account with dedicated server subscription.");
            }

            // capture the supplied password in case we need to re-use it after checking the login key
            ContentDownloader.Config.SuppliedPassword = password;

            return ContentDownloader.InitializeSteam3(username, password);
        }

        static int IndexOfParam(string[] args, string param)
        {
            for (int x = 0; x < args.Length; ++x)
            {
                if (args[x].Equals(param, StringComparison.OrdinalIgnoreCase))
                    return x;
            }
            return -1;
        }

        static bool HasParameter(string[] args, string param)
        {
            return IndexOfParam(args, param) > -1;
        }

        public static T GetParameter<T>(string[] args, string param, T defaultValue = default)
        {
            int index = IndexOfParam(args, param);

            if (index == -1 || index == (args.Length - 1))
                return defaultValue;

            string strParam = args[index + 1];

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(strParam);
            }

            return default;
        }

        static List<T> GetParameterList<T>(string[] args, string param)
        {
            List<T> list = new();
            int index = IndexOfParam(args, param);

            if (index == -1 || index == (args.Length - 1))
                return list;

            index++;

            while (index < args.Length)
            {
                string strParam = args[index];

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