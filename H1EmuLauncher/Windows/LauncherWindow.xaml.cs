using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json.Serialization;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Windows.Media.Animation;
using System.Linq;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class LauncherWindow : Window
    {
        FileSystemWatcher argsWatcher = new();
        ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        public static LauncherWindow launcherInstance;
        public static string serverJsonFile = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\servers.json";
        public static string[] rawArgs = null;
        public static bool systemWatcherFire = true;
        public static string newServerName = null;
        public static string newServerIp = null;

        public Storyboard CarouselNextAnimation;
        public Storyboard CarouselNextAnimationFollow;
        public Storyboard CarouselPreviousAnimation;
        public Storyboard CarouselPreviousAnimationFollow;
        public Storyboard UnfocusPropertiesAnimationShow;
        public Storyboard UnfocusPropertiesAnimationHide;

        public LauncherWindow()
        {
            InitializeComponent();
            launcherInstance = this;

            CarouselNextAnimation = FindResource("CarouselNextImageAnimation") as Storyboard;
            CarouselNextAnimationFollow = FindResource("CarouselNextImageAnimationFollow") as Storyboard;
            CarouselPreviousAnimation = FindResource("CarouselPrevImageAnimation") as Storyboard;
            CarouselPreviousAnimationFollow = FindResource("CarouselPrevImageAnimationFollow") as Storyboard;
            UnfocusPropertiesAnimationShow = FindResource("UnfocusPropertiesShow") as Storyboard;
            UnfocusPropertiesAnimationHide = FindResource("UnfocusPropertiesHide") as Storyboard;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        public class ServerList
        {
            [JsonPropertyName("Server Name")]
            public string SName { get; set; }
            [JsonPropertyName("Server Address")]
            public string SAddress { get; set; }
        }

        public async void ArgsWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (!systemWatcherFire)
                return;

            systemWatcherFire = false;

            await Task.Delay(500);

            rawArgs = File.ReadAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt").Split(" ");

            BringToFront(Process.GetCurrentProcess());

            Dispatcher.Invoke(new Action(delegate
            {
                ExecuteArguments();
            }));

            systemWatcherFire = true;
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void BringToFront(Process pTemp)
        {
            SetForegroundWindow(pTemp.MainWindowHandle);
        }

        private void ServerSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (serverSelector.SelectedIndex == serverSelector.Items.Count - 1)
                serverSelector.SelectedIndex = 0;

            Properties.Settings.Default.lastServer = serverSelector.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void AddNewServer(object sender, MouseButtonEventArgs e)
        {
            AddServerDetails();
        }

        private void LoadServers()
        {
            if (!File.Exists(serverJsonFile))
                File.WriteAllText(serverJsonFile, "[]");

            try
            {
                // Load all of the servers into the server selector
                List<ServerList> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));
                foreach (ServerList server in currentjson)
                {
                    ComboBoxItem newItem = new ComboBoxItem { Content = server.SName, Style = (Style)FindResource("ComboBoxItemStyle") };
                    serverSelector.Items.Insert(serverSelector.Items.Count - 2, newItem);
                }

                // Add an event for only user added servers in the list to delete on right click
                foreach (var item in serverSelector.Items)
                {
                    int index = serverSelector.Items.IndexOf(item);
                    if (item is ComboBoxItem serverItem)
                    {
                        if (index > 1 && index < serverSelector.Items.Count - 2)
                        {
                            serverItem.PreviewMouseRightButtonUp += ItemRightMouseButtonUp;
                            ContextMenu deleteMenu = new ContextMenu();
                            deleteMenu.Style = (Style)FindResource("ContextMenuStyle");
                            serverItem.ContextMenu = deleteMenu;
                            MenuItem deleteOption = new MenuItem();
                            deleteOption.Header = "Delete";
                            deleteOption.Style = (Style)FindResource("DeleteMenuItem");
                            deleteOption.Click += DeleteServerFromList;
                            deleteMenu.Items.Add(deleteOption);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item184")} \"{e.Message}\".", this);
            }

            try
            {
                serverSelector.SelectedIndex = Properties.Settings.Default.lastServer;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error loading last selected server: \"{e.Message}\". Setting selected server index to 0.");
                serverSelector.SelectedIndex = 0;
            }
        }

        public void AddServerDetails()
        {
            MessageBoxResult dr = CustomMessageBox.AddServer(this);
            if (dr != MessageBoxResult.OK)
                return;

            try
            {
                List<ServerList> currentjson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));

                currentjson.Add(new ServerList()
                {
                    SName = newServerName.Trim(),
                    SAddress = newServerIp.Trim().Replace(" ", "")
                });

                string newJson = JsonSerializer.Serialize(currentjson, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(serverJsonFile, newJson);

                ComboBoxItem newItem = new ComboBoxItem { Content = newServerName.Trim(), Style = (Style)FindResource("ComboBoxItemStyle") };
                serverSelector.Items.Insert(serverSelector.Items.Count - 2, newItem);
                serverSelector.SelectedIndex = serverSelector.Items.Count - 3;

                // Add an event for only user added servers in the list to delete on right click
                foreach (var item in serverSelector.Items)
                {
                    int index = serverSelector.Items.IndexOf(item);
                    if (item is ComboBoxItem serverItem)
                    {
                        if (index == serverSelector.Items.Count - 3)
                        {
                            serverItem.PreviewMouseRightButtonUp += ItemRightMouseButtonUp;
                            ContextMenu deleteMenu = new ContextMenu();
                            deleteMenu.Style = (Style)FindResource("ContextMenuStyle");
                            serverItem.ContextMenu = deleteMenu;
                            MenuItem deleteOption = new MenuItem();
                            deleteOption.Header = "Delete";
                            deleteOption.Style = (Style)FindResource("DeleteMenuItem");
                            deleteOption.Click += DeleteServerFromList;
                            deleteMenu.Items.Add(deleteOption);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", this);
            }

            newServerName = null;
            newServerIp = null;
        }

        private void ServerSelectorPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in serverSelector.Items)
            {
                if (item is ComboBoxItem serverItem)
                {
                    serverItem.Style = (Style)FindResource("ComboBoxItemStyle");
                }
            }
        }

        public ComboBoxItem itemRightClicked;

        private void ItemRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemRightClicked = (ComboBoxItem)sender;
            itemRightClicked.Style = (Style)FindResource("ComboBoxItemStyleSelected");
            System.Windows.Forms.Application.DoEvents();
        }

        private void DeleteServerFromList(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = CustomMessageBox.ShowResult(FindResource("item147").ToString(), this);
            if (dr != MessageBoxResult.Yes)
                return;

            List<ServerList> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));

            int index = -1;

            foreach (var item in currentjson)
            {
                index++;
                if (item.SName == (string)itemRightClicked.Content)
                    break;
            }

            currentjson.Remove(currentjson[index]);

            string finalJson = System.Text.Json.JsonSerializer.Serialize(currentjson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(serverJsonFile, finalJson);

            serverSelector.Items.Remove(itemRightClicked);
            serverSelector.SelectedIndex = index + 1;
        }

        public bool LaunchLocalServer(string gameVersionString)
        {
            try
            {
                if (!Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master\\node_modules"))
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(FindResource("item52").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                    }));

                    return false;
                }

                string serverVersion = string.Empty;

                if (gameVersionString == "15jan2015")
                {
                    serverVersion = "npm start";
                }
                else if (gameVersionString == "22dec2016")
                {
                    serverVersion = "npm run start-2016";
                }

                Process p = new()
                {
                    StartInfo = cmdShell
                };

                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master\\node-v{Info.NODEJS_VERSION}-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine(serverVersion);
                    }
                }

                p.WaitForExit(5000);

                if (p.HasExited)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(FindResource("item168").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                    }));

                    return false;
                }
            }
            catch (Exception er)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    CustomMessageBox.Show($"{FindResource("item53")} \"{er.Message}\"", this);
                }));

                return false;
            }

            return true;
        }

        private void LaunchClient(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new();

            if (!settings.CheckDirectory())
                return;

            int gameVersionInt = 0;
            string gameVersionString = string.Empty;
            string serverIp = string.Empty;
            string sessionId = string.Empty;

            new Thread(() =>
            {
                try
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (serverSelector.SelectedIndex == 0)
                            return;

                        if (serverSelector.SelectedIndex == 1)
                        {
                            serverIp = "localhost:1115";
                            return;
                        }
                    }));

                    List<ServerList> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));

                    foreach (var item in currentjson)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (item.SName == serverSelector.Text)
                            {
                                serverIp = item.SAddress;
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item142")} \"{ex.Message}\"", this);
                    }));
                }

                try
                {
                    settings.CheckGameVersion();
                    gameVersionString = SettingsWindow.gameVersionString;

                    switch (gameVersionString)
                    {
                        case "15jan2015":
                            gameVersionInt = 1;
                            break;
                        case "22dec2016":
                            gameVersionInt = 2;
                            break;
                    }

                    if (gameVersionString == "15jan2015" || gameVersionString == "22dec2016")
                    {
                        if (serverIp == "")
                            serverIp = Info.H1EMU_SERVER_IP;

                        if (serverIp == Info.H1EMU_SERVER_IP)
                        {
                            // sessionIdKey is the same as accountKey, not possible change the name without resetting users settings
                            if (string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    CustomMessageBox.Show(FindResource("item153").ToString(), this);
                                }));

                                return;
                            }
                            else
                            {
                                //if (!CheckAccountKey.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey))
                                    //return;

                                sessionId = $"{{\"sessionId\":\"{Properties.Settings.Default.sessionIdKey}\",\"gameVersion\":{gameVersionInt}}}";
                            }
                        }
                        else
                        {
                            sessionId = $"{{\"sessionId\":\"0\",\"gameVersion\":{gameVersionInt}}}";
                        }

                        if (serverIp == "localhost:1115")
                        {
                            if (!LaunchLocalServer(gameVersionString))
                                return;
                        }

                        ApplyPatchClass.CheckPatch();

                        Process p = new()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = $"{Properties.Settings.Default.activeDirectory}\\H1Z1.exe",
                                Arguments = $"sessionid={sessionId} gamecrashurl={Info.GAME_CRASH_URL} server={serverIp}",
                                WindowStyle = ProcessWindowStyle.Normal,
                                WorkingDirectory = Properties.Settings.Default.activeDirectory,
                                UseShellExecute = true
                            }
                        };

                        p.Start();
                    }
                    else if (gameVersionString == "processBeingUsed")
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                        }));

                        return;
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                        }));

                        return;
                    }
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item13")} \"{er.Message}\".", this);
                    }));
                }

            }).Start();
        }

        private void LauncherWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.firstTimeUse != 1) 
            {
                Hide();
                DisclaimerWindow dc = new();
                dc.ShowDialog();
            }

            if (File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{UpdateWindow.downloadFileName}"))
                File.Delete($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{UpdateWindow.downloadFileName}");

            if (!File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt"))
                File.Create($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt");

            if (Directory.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages"))
                Directory.Delete($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages", true);

            argsWatcher.Path = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher";
            argsWatcher.Filter = "args.txt";
            argsWatcher.EnableRaisingEvents = true;
            argsWatcher.Changed += new FileSystemEventHandler(ArgsWatcherChanged);

            VersionInformation();
            LoadServers();
            Carousel.BeginImageCarousel();
            LanguageBox.SelectedIndex = Properties.Settings.Default.language;

            if (LanguageBox.SelectedIndex == 1)
                chineseLink.Visibility = Visibility.Visible;
            else
                chineseLink.Visibility = Visibility.Collapsed;
        }

        public void VersionInformation()
        {
            try
            {
                // Update version, date published and patch notes code
                HttpResponseMessage result = UpdateWindow.httpClient.GetAsync(new Uri(Info.SERVER_JSON_API)).Result;

                // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                if (result.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"{char.ToUpper(result.ReasonPhrase.First())}{result.ReasonPhrase.Substring(1)}");

                // Get latest release number, date published, and patch notes for server
                string jsonServer = result.Content.ReadAsStringAsync().Result;
                JsonEndPoints.Server.Root jsonDesServer = JsonSerializer.Deserialize<JsonEndPoints.Server.Root>(jsonServer);
                string latestVersion = jsonDesServer.tag_name;
                DateTime publishDate = jsonDesServer.published_at;
                string latestPatchNotes = jsonDesServer.body;

                // Display the content on the launcher
                //DateTime date = DateTime.ParseExact(publishDate, "G", CultureInfo.InvariantCulture);
                datePublished.Text = $" ({publishDate:dd MMMM yyyy})";
                updateVersion.Text = $" {latestVersion}";
                patchNotesBox.Text = latestPatchNotes;

                // Cache the latest server version, date and patch notes in the case of no internet
                Properties.Settings.Default.latestServerVersion = latestVersion;
                Properties.Settings.Default.patchNotes = latestPatchNotes;
                Properties.Settings.Default.publishDate = publishDate;
                Properties.Settings.Default.Save();
            }
            catch
            {
                updateVersion.Text = $" {Properties.Settings.Default.latestServerVersion}";
                datePublished.Text = $" ({Properties.Settings.Default.publishDate:dd MMMM yyyy})";
                patchNotesBox.Text = Properties.Settings.Default.patchNotes;
            }
        }

        public void ExecuteArguments()
        {
            // If there are no args return
            if (!(rawArgs.Length > 0))
                return;

            // Close every other window apart from the launcher window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Name != "MainLauncherPage")
                    window.Close();
            }

            // If the arguments are launched from browser, remove some stuff
            if (rawArgs[0].Contains("%20"))
            {
                rawArgs[0] = rawArgs[0].Replace("h1emulauncher://", "").Replace("/\"", "").Replace("%20", " ");
                rawArgs = rawArgs[0].Split(" ");
            }

            // Set the server name and ip textboxes text
            if (!string.IsNullOrEmpty(SteamFrame.Login.GetParameter(rawArgs, "-name", "")) || !string.IsNullOrEmpty(SteamFrame.Login.GetParameter(rawArgs, "-ip", "")))
                AddServerDetails();

            // Launch settings and tell it to open Account Key window
            if (!string.IsNullOrEmpty(SteamFrame.Login.GetParameter(rawArgs, "-accountkey", "")))
            {
                SettingsWindow.launchAccountKeyWindow = true;
                SettingsWindow se = new();
                se.ShowDialog();
            }

            File.WriteAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", "");
            rawArgs = null;
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            int selectedLanguage = LanguageBox.SelectedIndex;

            if (selectedLanguage == 1)
                chineseLink.Visibility = Visibility.Visible;
            else
                chineseLink.Visibility = Visibility.Collapsed;

            try
            {
                switch (selectedLanguage)
                {
                    // Update and save settings
                    case 0:
                        SetLanguageFile.SaveLang(0);
                        break;
                    case 1:
                        SetLanguageFile.SaveLang(1);
                        break;
                    case 2:
                        SetLanguageFile.SaveLang(2);
                        break;
                    case 3:
                        SetLanguageFile.SaveLang(3);
                        break;
                    case 4:
                        SetLanguageFile.SaveLang(4);
                        break;
                    case 5:
                        SetLanguageFile.SaveLang(5);
                        break;
                    case 6:
                        SetLanguageFile.SaveLang(6);
                        break;
                    case 7:
                        SetLanguageFile.SaveLang(7);
                        break;
                    case 8:
                        SetLanguageFile.SaveLang(8);
                        break;
                    case 9:
                        SetLanguageFile.SaveLang(9);
                        break;
                    case 10:
                        SetLanguageFile.SaveLang(10);
                        break;
                    case 11:
                        SetLanguageFile.SaveLang(11);
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} \"{ex.Message}\"", this);
                return;
            }

            // Reload pages
            steamFramePanel.Refresh();
        }

        public bool doContinue = true;

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Carousel.playCarousel.Stop();
            Carousel.playCarousel.Begin();

            doContinue = true;
        }

        private void PrevImageClick(object sender, RoutedEventArgs e)
        {
            if (!doContinue)
                return;

            Carousel.PreviousImage();

            doContinue = false;
        }

        private void NextImageClick(object sender, RoutedEventArgs e)
        {
            if (!doContinue)
                return;

            Carousel.NextImage();

            doContinue = false;
        }

        private void CarouselMouseEnter(object sender, MouseEventArgs e)
        {
            Carousel.playCarousel.Pause();

            nextImage.Visibility = Visibility.Visible;
            prevImage.Visibility = Visibility.Visible;

            DoubleAnimation showImageControlsNext = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            showImageControlsNext.AccelerationRatio = 0.2;
            showImageControlsNext.DecelerationRatio = 0.2;
            showImageControlsNext.SetValue(Storyboard.TargetProperty, nextImage);
            showImageControlsNext.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsNext = new Storyboard();
            playShowImageControlsNext.Children.Add(showImageControlsNext);
            playShowImageControlsNext.Begin();

            DoubleAnimation showImageControlsPrev = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            showImageControlsPrev.AccelerationRatio = 0.2;
            showImageControlsPrev.DecelerationRatio = 0.2;
            showImageControlsPrev.SetValue(Storyboard.TargetProperty, prevImage);
            showImageControlsPrev.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsPrev = new Storyboard();
            playShowImageControlsPrev.Children.Add(showImageControlsPrev);
            playShowImageControlsPrev.Begin();
        }

        private void CarouselMouseLeave(object sender, MouseEventArgs e)
        {
            Carousel.playCarousel.Resume();

            DoubleAnimation showImageControlsNext = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
            showImageControlsNext.AccelerationRatio = 0.2;
            showImageControlsNext.DecelerationRatio = 0.2;
            showImageControlsNext.SetValue(Storyboard.TargetProperty, nextImage);
            showImageControlsNext.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsNext = new Storyboard();
            playShowImageControlsNext.Children.Add(showImageControlsNext);
            playShowImageControlsNext.Completed += (s, o) => nextImage.Visibility = Visibility.Hidden;
            playShowImageControlsNext.Begin();

            DoubleAnimation showImageControlsPrev = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));
            showImageControlsPrev.AccelerationRatio = 0.2;
            showImageControlsPrev.DecelerationRatio = 0.2;
            showImageControlsPrev.SetValue(Storyboard.TargetProperty, prevImage);
            showImageControlsPrev.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsPrev = new Storyboard();
            playShowImageControlsPrev.Children.Add(showImageControlsPrev);
            playShowImageControlsPrev.Completed += (s, o) => prevImage.Visibility = Visibility.Hidden;
            playShowImageControlsPrev.Begin();
        }

        private void ReportBuglink(object sender, RoutedEventArgs e)
        {
            ReportBugWindow reportBug = new();
            reportBug.ShowDialog();
        }

        private void AboutHyperlink(object sender, RoutedEventArgs e)
        {
            AboutPageWindow aboutPage = new();
            aboutPage.ShowDialog();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow se = new();
            se.ShowDialog();
        }

        private void H1Hyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri.ToString(),
                UseShellExecute = true
            });
        }

        private void FullUpdatesHyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri.ToString(),
                UseShellExecute = true
            });
        }

        private void H1EmuChineseLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri.ToString(),
                UseShellExecute = true
            });
        }

        private void OpenDiscord(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.DISCORD_LINK,
                UseShellExecute = true
            });
        }

        private void patchNotesCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.CHANGELOG);
        }

        private void websiteLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.WEBSITE);
        }

        private void discordLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.DISCORD_LINK);
        }

        private void chineseLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.CHANGELOG);
        }

        private void LauncherWindowContentRendered(object sender, EventArgs e)
        {
            ExecuteArguments();
        }

        private void MinimiseButtonClick(object sender, RoutedEventArgs e)
        {
            Launcher.RenderTransformOrigin = new Point(0.5, 1);

            Storyboard sb = FindResource("MinimiseLauncher") as Storyboard;

            if (sb != null)
            {
                sb.Completed += (s, _) =>
                {
                    WindowState = WindowState.Minimized;
                    sb.Stop();
                };

                sb.Begin();
            }
        }

        private void LauncherStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Storyboard sb = FindResource("RestoreLauncher") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, _) =>
                    {
                        Launcher.RenderTransformOrigin = new Point(0.5, 0.5);
                        sb.Stop();
                    };

                    sb.Begin();
                }
            }
        }

        public bool IsCompleted = false;

        private void LauncherClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                e.Cancel = true;
                Storyboard sb = FindResource("CloseLauncher") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, _) =>
                    {
                        IsCompleted = true;
                        Environment.Exit(0);
                    };

                    sb.Begin();
                }
            }
        }

        private void CloseLauncher(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}