using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json.Serialization;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows.Media.Animation;
using System.Linq;
using H1EmuLauncher.Classes;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Media;

namespace H1EmuLauncher
{
    public partial class LauncherWindow : Window
    {
        System.Windows.Forms.NotifyIcon launcherNotifyIcon = new();
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

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            CarouselNextAnimation = FindResource("CarouselNextImageAnimation") as Storyboard;
            CarouselNextAnimationFollow = FindResource("CarouselNextImageAnimationFollow") as Storyboard;
            CarouselPreviousAnimation = FindResource("CarouselPrevImageAnimation") as Storyboard;
            CarouselPreviousAnimationFollow = FindResource("CarouselPrevImageAnimationFollow") as Storyboard;
            UnfocusPropertiesAnimationShow = FindResource("UnfocusPropertiesShow") as Storyboard;
            UnfocusPropertiesAnimationHide = FindResource("UnfocusPropertiesHide") as Storyboard;

            ContextMenu notifyIconContextMenu = new();
            notifyIconContextMenu.Style = (Style)FindResource("ContextMenuStyle");
            notifyIconContextMenu.PlacementTarget = this;

            MenuItem notifyIconMenuItem = new();
            notifyIconMenuItem.Style = (Style)FindResource("DeleteMenuItem");
            notifyIconMenuItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item194");
            notifyIconMenuItem.PreviewMouseLeftButtonDown += (o, s) => { Environment.Exit(0); };
            notifyIconMenuItem.MouseLeave += (o, s) => { notifyIconContextMenu.IsOpen = false; };
            notifyIconContextMenu.Items.Add(notifyIconMenuItem);

            launcherNotifyIcon.Icon = Properties.Resources.Icon;
            launcherNotifyIcon.Text = "H1EmuLauncher";
            launcherNotifyIcon.MouseDown += (o, s) =>
            {
                if (s.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Show();
                    Activate();
                    launcherNotifyIcon.Visible = false;
                }
                else if (s.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    notifyIconContextMenu.IsOpen = true;
                }
            };
        }

        public class ServerList
        {
            [JsonPropertyName("Server Name")]
            public string CustomServerName { get; set; }
            [JsonPropertyName("Server Address")]
            public string CustomServerIp { get; set; }
        }

        public async void ArgsWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (!systemWatcherFire)
                return;

            systemWatcherFire = false;

            await Task.Delay(500);

            rawArgs = File.ReadAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt").Split(" ");

            Dispatcher.Invoke(new Action(delegate
            {
                if (WindowState != WindowState.Normal)
                    WindowState = WindowState.Normal;

                Show();
                Activate();
                launcherNotifyIcon.Visible = false;
                ExecuteArguments();
            }));

            systemWatcherFire = true;
        }

        public void ExecuteArguments()
        {
            // If there are no args return
            if (!(rawArgs.Length > 0))
                return;

            // If the arguments are launched from browser, remove some stuff
            if (rawArgs[0].Contains("%20"))
            {
                rawArgs[0] = rawArgs[0].Replace("h1emulauncher://", "").Replace("/\"", "").Replace("%20", " ");
                rawArgs = rawArgs[0].Split(" ");
            }

            // If the new server and ip arguments exist, open the Add Server window and tell it to fill in the name and ip fields with the specified argument values
            if (!string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-servername", "")) || !string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-serverip", "")))
            {
                // Close every other window apart from the Launcher and Add Server windows
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.Name != Name && window.Name != "AddServer")
                        window.Close();
                }

                string newServerName = SteamFramePages.Login.GetParameter(rawArgs, "-servername", "");
                string newServerIp = SteamFramePages.Login.GetParameter(rawArgs, "-serverip", "");

                if (AddServerWindow.addServerInstance == null)
                    CustomMessageBox.AddServer(this, newServerName, newServerIp);
                else
                    AddServerWindow.addServerInstance.FillInFields(newServerName, newServerIp);
            }
            // If the account key argument exists, open the Settings window and tell it to open the Account Key tab with accountkey argument value
            else if (!string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-accountkey", "")))
            {
                // Close every other window apart from the Launcher and Settings windows
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.Name != Name && window.Name != "Settings")
                        window.Close();
                }

                SettingsWindow.accountKeyArgument = SteamFramePages.Login.GetParameter(rawArgs, "-accountkey", "");

                if (SettingsWindow.settingsInstance == null)
                {
                    SettingsWindow.openAccountKeyPage = true;
                    SettingsWindow se = new();
                    se.Show();
                    launcherFade.IsHitTestVisible = true;
                }
                else
                {
                    SettingsWindow.openAccountKeyPage = true;
                    SettingsWindow.SwitchToAccountKeyTab();
                }
            }

            File.WriteAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", "");
            rawArgs = null;
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

        private void LoadServers()
        {
            if (!File.Exists(serverJsonFile))
                File.WriteAllText(serverJsonFile, "[]");

            try
            {
                // Load all of the servers into the server selector
                List<ServerList> currentjson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));
                foreach (ServerList server in currentjson)
                {
                    ComboBoxItem newItem = new ComboBoxItem { Content = server.CustomServerName, Style = (Style)FindResource("ComboBoxItemStyle") };
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
                            deleteOption.Style = (Style)FindResource("DeleteMenuItem");
                            deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
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

        private void AddNewServer(object sender, MouseButtonEventArgs e)
        {
            CustomMessageBox.AddServer(this);
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

        public void ItemRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemRightClicked = (ComboBoxItem)sender;
            itemRightClicked.Style = (Style)FindResource("ComboBoxItemStyleSelected");
            System.Windows.Forms.Application.DoEvents();
        }

        public void DeleteServerFromList(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = CustomMessageBox.Show(FindResource("item147").ToString(), this, true, true, false, false);
            if (dr != MessageBoxResult.Yes)
                return;

            List<ServerList> currentjson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));

            int index = -1;

            foreach (var item in currentjson)
            {
                index++;
                if (item.CustomServerName == (string)itemRightClicked.Content)
                    break;
            }

            currentjson.Remove(currentjson[index]);

            string finalJson = JsonSerializer.Serialize(currentjson, new JsonSerializerOptions { WriteIndented = true });
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
                else if (gameVersionString == "kotk")
                {
                    //serverVersion = "npm run start-2016";
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
            SettingsPages.GameFiles gameFiles = new();

            if (!gameFiles.CheckDirectory())
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

                    List<ServerList> currentjson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(serverJsonFile));

                    foreach (var item in currentjson)
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (item.CustomServerName == serverSelector.Text)
                            {
                                serverIp = item.CustomServerIp;
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
                    gameFiles.CheckGameVersion();
                    gameVersionString = SettingsPages.GameFiles.gameVersionString;

                    switch (gameVersionString)
                    {
                        case "15jan2015":
                            gameVersionInt = 1;
                            break;
                        case "22dec2016":
                            gameVersionInt = 2;
                            break;
                        case "kotk":
                            gameVersionInt = 3;
                            break;
                    }

                    if (gameVersionString == "15jan2015" || gameVersionString == "22dec2016" || gameVersionString == "kotk")
                    {
                        if (serverIp == "")
                        {
                            serverIp = Info.H1EMU_SERVER_IP;

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
                                // Check the users account key validity to deicde whether to let them connect to the H1Emu login server
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

                        if (Properties.Settings.Default.autoMinimise)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                launcherNotifyIcon.Visible = true;
                                Hide();
                            }));

                            new ToastContentBuilder().AddText(FindResource("item191").ToString()).Show();
                        }
                    }
                    else if (gameVersionString == "processBeingUsed")
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, false, false, true);
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
                        playButton.IsEnabled = true;
                        playButton.Content = FindResource("item8").ToString();
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

            if (Properties.Settings.Default.imageCarouselVisibility)
            {
                // Show image carousel
                imageCarousel.Visibility = Visibility.Visible;

                Properties.Settings.Default.imageCarouselVisibility = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                // Hide image carousel
                imageCarousel.Visibility = Visibility.Hidden;

                Properties.Settings.Default.imageCarouselVisibility = false;
                Properties.Settings.Default.Save();
            }
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

        double carouselButtonsAnimationDurationMilliseconds = 80;

        private void CarouselMouseEnter(object sender, MouseEventArgs e)
        {
            if (Carousel.playCarousel == null)
                return;

            Carousel.playCarousel.Pause();

            nextImage.Visibility = Visibility.Visible;
            prevImage.Visibility = Visibility.Visible;

            // Animation for fade in visibility next image button
            DoubleAnimation showImageControlsNext = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)));
            showImageControlsNext.AccelerationRatio = 0.2;
            showImageControlsNext.DecelerationRatio = 0.2;
            showImageControlsNext.SetValue(Storyboard.TargetProperty, nextImage);
            showImageControlsNext.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsNext = new Storyboard();
            playShowImageControlsNext.Children.Add(showImageControlsNext);
            playShowImageControlsNext.Begin();

            // Animation for fade in visibility previous image button
            DoubleAnimation showImageControlsPrev = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)));
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
            if (Carousel.playCarousel == null)
                return;

            Carousel.playCarousel.Resume();

            // Animation for fade out visibility next image button
            DoubleAnimation showImageControlsNext = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)));
            showImageControlsNext.AccelerationRatio = 0.2;
            showImageControlsNext.DecelerationRatio = 0.2;
            showImageControlsNext.SetValue(Storyboard.TargetProperty, nextImage);
            showImageControlsNext.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsNext = new Storyboard();
            playShowImageControlsNext.Children.Add(showImageControlsNext);
            playShowImageControlsNext.Completed += (s, o) => nextImage.Visibility = Visibility.Hidden;
            playShowImageControlsNext.Begin();

            // Animation for fade out visibility previous image button
            DoubleAnimation showImageControlsPrev = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)));
            showImageControlsPrev.AccelerationRatio = 0.2;
            showImageControlsPrev.DecelerationRatio = 0.2;
            showImageControlsPrev.SetValue(Storyboard.TargetProperty, prevImage);
            showImageControlsPrev.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(OpacityProperty));

            Storyboard playShowImageControlsPrev = new Storyboard();
            playShowImageControlsPrev.Children.Add(showImageControlsPrev);
            playShowImageControlsPrev.Completed += (s, o) => prevImage.Visibility = Visibility.Hidden;
            playShowImageControlsPrev.Begin();
        }

        private void HyperlinkClick(object sender, RoutedEventArgs e)
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

        private void LauncherFadeMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (launcherFade.IsHitTestVisible)
                SystemSounds.Beep.Play();
        }

        private void MinimiseToSystemTrayButtonClick(object sender, RoutedEventArgs e)
        {
            launcherNotifyIcon.Visible = true;
            Hide();

            new ToastContentBuilder().AddText(FindResource("item191").ToString()).Show();
        }

        private void MinimiseButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void LauncherClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseLauncher(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}