using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>

    public partial class Login : Page
    {
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();
        static CancellationToken token;

        public static string gameInfo { get; set; } = "NULL";

        public Login()
        {
            InitializeComponent();
            needDowloadTip.Text = LanCtrler.GetWords("Need to download Just Survive?");
            loginSteamTip.Text = LanCtrler.GetWords("Enter your Steam credentials on the left to get started!");
            usernameHint.Text = LanCtrler.GetWords("Steam ID");
            passHint.Text = LanCtrler.GetWords("Password");
            
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!NullLoginCheck())
                {
                    return;
                }
                else
                {
                    new Thread(() =>
                    {
                        TryLoginDownload();

                    }).Start();
                }
            }
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            if (!NullLoginCheck())
            {
                return;
            }
            else
            {
                new Thread(() =>
                {
                    TryLoginDownload();
                }).Start();
            }
        }

        public bool NullLoginCheck()
        {
            if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password))
            {
                CustomMessageBox.Show(LanCtrler.GetWords("Please make sure to fill in the Username and Password boxes."));
                return false;
            }

            return true;
        }

        public static string version = "";

        public async void TryLoginDownload()
        {
            string username = "";
            string password = "";

            string[] args = gameInfo.Split(' ');

            Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                username = usernameBox.Text;
                password = passwordBox.Password;
            });

            try
            {
                AccountSettingsStore.LoadFromFile("account.config");
            }
            catch (Exception er)
            {
                Debug.WriteLine(er.Message);
            }

            ContentDownloader.Config.RememberPassword = HasParameter(args, "-remember-password");
            ContentDownloader.Config.DownloadManifestOnly = HasParameter(args, "-manifest-only");

            int cellId = GetParameter<int>(args, "-cellid", -1);
            if (cellId == -1) { cellId = 0; }

            ContentDownloader.Config.CellID = cellId;
            string fileList = GetParameter<string>(args, "-filelist");
            string[] files = null;
            if (fileList != null)
            {
                try
                {
                    string fileListData = File.ReadAllText(fileList);
                    files = fileListData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    ContentDownloader.Config.UsingFileList = true;
                    ContentDownloader.Config.FilesToDownload = new List<string>();
                    ContentDownloader.Config.FilesToDownloadRegex = new List<Regex>();

                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    foreach (var fileEnry in files)
                    {
                        try
                        {
                            string fileEntryProcessed;

                            if (isWindows) { fileEntryProcessed = fileEnry.Replace("/", "[\\\\|/]"); }
                            else { fileEntryProcessed = fileEnry; }

                            Regex rgx = new Regex(fileEntryProcessed, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            ContentDownloader.Config.FilesToDownloadRegex.Add(rgx);
                        }
                        catch
                        {
                            if (isWindows)
                            {
                                ContentDownloader.Config.FilesToDownload.Add(fileEnry.Replace("/", "\\"));
                            }
                            ContentDownloader.Config.FilesToDownload.Add(fileEnry);
                        }
                    }
                    Debug.WriteLine("Using filelist: " + fileList);
                }
                catch (Exception ex) { Debug.WriteLine("Warning: Unable to load filelist: " + ex.ToString()); }
            }

            ContentDownloader.Config.InstallDirectory = GetParameter<string>(args, "-dir");

            ContentDownloader.Config.VerifyAll = HasParameter(args, "-verify-all") || HasParameter(args, "-verify_all") || HasParameter(args, "-validate");
            ContentDownloader.Config.MaxServers = GetParameter<int>(args, "-max-servers", 20);
            ContentDownloader.Config.MaxDownloads = GetParameter<int>(args, "-max-downloads", 8);
            ContentDownloader.Config.MaxServers = Math.Max(ContentDownloader.Config.MaxServers, ContentDownloader.Config.MaxDownloads);
            ContentDownloader.Config.LoginID = HasParameter(args, "-loginid") ? (uint?)GetParameter<uint>(args, "-loginid") : null;

            ulong pubFile = GetParameter<ulong>(args, "-pubfile", ContentDownloader.INVALID_MANIFEST_ID);
            ulong ugcId = GetParameter<ulong>(args, "-ugc", ContentDownloader.INVALID_MANIFEST_ID);

            #region App downloading

            string branch = GetParameter<string>(args, "-branch") ?? GetParameter<string>(args, "-beta") ?? ContentDownloader.DEFAULT_BRANCH;
            ContentDownloader.Config.BetaPassword = GetParameter<string>(args, "-betapassword");

            ContentDownloader.Config.DownloadAllPlatforms = HasParameter(args, "-all-platforms");
            string os = GetParameter<string>(args, "-os", null);

            if (ContentDownloader.Config.DownloadAllPlatforms && !String.IsNullOrEmpty(os))
            {
                Debug.WriteLine("Error: Cannot specify -os when -all-platforms is specified.");
            }

            string arch = GetParameter<string>(args, "-osarch", null);

            ContentDownloader.Config.DownloadAllLanguages = HasParameter(args, "-all-languages");
            string language = GetParameter<string>(args, "-language", null);

            if (ContentDownloader.Config.DownloadAllLanguages && !String.IsNullOrEmpty(language))
            {
                Debug.WriteLine("Error: Cannot specify -language when -all-languages is specified.");
            }

            bool lv = HasParameter(args, "-lowviolence");

            List<(uint, ulong)> depotManifestIds = new List<(uint, ulong)>();
            bool isUGC = false;

            if (InitializeSteam(username, password))
            {
                try
                {
                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.SteamFrame.Navigate(new Uri("GameInfo.xaml", UriKind.Relative));
                    });

                    bool result = true;

                SelectLocation:
                    tokenSource.Dispose();
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;

                    token.WaitHandle.WaitOne();

                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

                        if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (Directory.GetFileSystemEntries(folderBrowser.SelectedPath).Length != 0)
                            {
                                System.Windows.Forms.DialogResult dr = CustomMessageBox.ShowResult("The selected directory is not empty.\n\nWould you like to continue anyway?");
                                if (dr == System.Windows.Forms.DialogResult.Yes)
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
                    });

                    if (!result) { goto SelectLocation; }

                    args = gameInfo.Split(' ');

                    List<uint> depotIdList = GetParameterList<uint>(args, "-depot");
                    List<ulong> manifestIdList = GetParameterList<ulong>(args, "-manifest");
                    if (manifestIdList.Count > 0)
                    {
                        if (depotIdList.Count != manifestIdList.Count)
                        {
                            Debug.WriteLine("Error: -manifest requires one id for every -depot specified");
                        }

                        var zippedDepotManifest = depotIdList.Zip(manifestIdList, (depotId, manifestId) => (depotId, manifestId));
                        depotManifestIds.AddRange(zippedDepotManifest);
                    }
                    else
                    {
                        depotManifestIds.AddRange(depotIdList.Select(depotId => (depotId, ContentDownloader.INVALID_MANIFEST_ID)));
                    }

                    uint appId = GetParameter<uint>(args, "-app", ContentDownloader.INVALID_APP_ID);
                    if (appId == ContentDownloader.INVALID_APP_ID)
                    {
                        Debug.WriteLine("Error: -app not specified!");
                    }

                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.SteamFrame.Navigate(new Uri("DownloadStatus.xaml", UriKind.Relative));
                    });

                    foreach (ulong manifestVersion in manifestIdList)
                    {
                        if (manifestVersion == 1930886153446950288)
                        {
                            version = "2015";

                            Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                DownloadStatus.downStatus.gameDownloadText.Text = $"{version}:";
                                DownloadStatus.downStatus.downloadProgressText.Text = "Processing...";
                            });
                        }
                        else
                        {
                            version = "2016";

                            Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                DownloadStatus.downStatus.gameDownloadText.Text = $"{version}:";
                                DownloadStatus.downStatus.downloadProgressText.Text = "Processing...";
                            });
                        }
                    }

                    await ContentDownloader.DownloadAppAsync(appId, depotManifestIds, branch, os, arch, language, lv, isUGC).ConfigureAwait(false);

                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(string.Format(LanCtrler.GetWords("Successfully downloaded H1Z1: Just Survive version{0}."), version));
                    });
                }
                catch (Exception ph) when (ph is OperationCanceledException)
                {
                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.SteamFrame.Navigate(new Uri("Login.xaml", UriKind.Relative));
                        CustomMessageBox.Show(string.Format(LanCtrler.GetWords("Successfully cancelled the download of H1Z1: Just Survive version {0}"),version));
                    });

                    return;
                }
                catch (Exception ex) when (ex is ContentDownloaderException)
                {
                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.SteamFrame.Navigate(new Uri("Login.xaml", UriKind.Relative));
                        CustomMessageBox.Show(string.Format(LanCtrler.GetWords("ContentDownloaderException:{0}"),ex.Message));
                    });

                    return;
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.SteamFrame.Navigate(new Uri("Login.xaml", UriKind.Relative));
                        CustomMessageBox.Show(string.Format(LanCtrler.GetWords("Download failed to due to an unhandled exception:{0}"),er.Message));
                    });

                    return;
                }
                finally
                {
                    ContentDownloader.ShutdownSteam3();
                }
            }
            else
            {
                Debug.WriteLine("Error: InitializeSteam failed.");
            }

            username = "";
            password = "";

            #endregion
        }

        static bool InitializeSteam(string username, string password)
        {
            if (username != null && password == null && (!ContentDownloader.Config.RememberPassword || !AccountSettingsStore.Instance.LoginKeys.ContainsKey(username)))
            {
                do
                {
                    Console.Write("Enter account password for \"{0}\": ", username);
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

        static T GetParameter<T>(string[] args, string param, T defaultValue = default(T))
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

            return default(T);
        }

        static List<T> GetParameterList<T>(string[] args, string param)
        {
            List<T> list = new List<T>();
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

        private void passBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(passwordBox.Password))
            {
                passHint.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                passHint.Visibility = Visibility.Collapsed;
            }
        }

        private void usernameBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(usernameBox.Text))
            {
                usernameHint.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(usernameBox.Text))
            {
                usernameHint.Visibility = Visibility.Collapsed;
            }
        }

        private void usernameHint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            usernameHint.Visibility = Visibility.Collapsed;
            usernameBox.Focus();
        }

        private void passHint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            passHint.Visibility = Visibility.Collapsed;
            passwordBox.Focus();
        }

        private void usernameBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            usernameHint.Visibility = Visibility.Collapsed;
        }

        private void passwordBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            passHint.Visibility = Visibility.Collapsed;
        }
    }
}
