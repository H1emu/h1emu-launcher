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
using System.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Toolkit.Uwp.Notifications;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class LauncherWindow : Window
    {
        private System.Windows.Forms.NotifyIcon launcherNotifyIcon = new();
        private FileSystemWatcher argsWatcher = new();
        private ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        public static LauncherWindow launcherInstance;
        public static ContextMenu notifyIconContextMenu = new();
        public static string customServersJsonFile = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\servers.json";
        public static string recentServersJsonFile = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\recentServers.json";
        public static string[] rawArgs = null;
        public static string gameVersionString { get; set; }
        public static bool systemWatcherFire = true;
        public static bool executeArguments;

        public Storyboard CarouselNextAnimation;
        public Storyboard CarouselNextAnimationFollow;
        public Storyboard CarouselPreviousAnimation;
        public Storyboard CarouselPreviousAnimationFollow;
        public Storyboard UnfocusPropertiesAnimationShow;
        public Storyboard UnfocusPropertiesAnimationHide;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SetForegroundWindow(IntPtr hwnd);
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
                    // Get context menu handle and bring it to the foreground
                    if (PresentationSource.FromVisual(notifyIconContextMenu) is HwndSource hwndSource)
                        SetForegroundWindow(hwndSource.Handle);
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
                ExecuteArguments();
                launcherNotifyIcon.Visible = false;
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
            if (!serverSelector.IsLoaded)
                return;

            if (serverSelector.SelectedIndex == serverSelector.Items.Count - 1 ||
                serverSelector.SelectedIndex == serverSelector.Items.Count - 2 && serverSelector.Items[serverSelector.Items.Count - 2] is Separator ||
                serverSelector.SelectedIndex == 2 && serverSelector.Items[2] is Separator) 
            {
                serverSelector.SelectedIndex = 0;
            }

            Properties.Settings.Default.lastServer = serverSelector.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void LoadServers()
        {
            if (!File.Exists(customServersJsonFile) || string.IsNullOrEmpty(File.ReadAllText(customServersJsonFile)))
                File.WriteAllText(customServersJsonFile, "[]");

            if (!File.Exists(recentServersJsonFile) || string.IsNullOrEmpty(File.ReadAllText(recentServersJsonFile)))
                File.WriteAllText(recentServersJsonFile, "[]");

            try
            {
                // Load options for context menu on the applications' NotifyIcon
                notifyIconContextMenu.Style = (Style)FindResource("ContextMenuStyle");
                notifyIconContextMenu.PlacementTarget = this;

                MenuItem notifyIconMenuItemH1EmuServers = new();
                notifyIconMenuItemH1EmuServers.Style = (Style)FindResource("CustomMenuItem");
                notifyIconMenuItemH1EmuServers.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) };
                notifyIconMenuItemH1EmuServers.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item139");
                notifyIconMenuItemH1EmuServers.Margin = new Thickness(0, 5, 0, 0);
                notifyIconMenuItemH1EmuServers.PreviewMouseLeftButtonDown += (o, s) =>
                {
                    serverSelector.SelectedIndex = 0;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                MenuItem notifyIconMenuItemSingleplayer = new();
                notifyIconMenuItemSingleplayer.Style = (Style)FindResource("CustomMenuItem");
                notifyIconMenuItemSingleplayer.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) };
                notifyIconMenuItemSingleplayer.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item140");
                notifyIconMenuItemSingleplayer.Margin = new Thickness(0, 5, 0, 0);
                notifyIconMenuItemSingleplayer.PreviewMouseLeftButtonDown += (o, s) =>
                {
                    serverSelector.SelectedIndex = 1;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                Separator notifyIconItemSeparator = new();
                notifyIconItemSeparator.Style = (Style)FindResource("SeparatorMenuItem");
                notifyIconItemSeparator.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                notifyIconItemSeparator.Margin = new Thickness(10, 7, 10, 2);

                MenuItem notifyIconMenuItemExit = new();
                notifyIconMenuItemExit.Style = (Style)FindResource("CustomMenuItem");
                notifyIconMenuItemExit.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Delete.png", UriKind.Absolute)) };
                notifyIconMenuItemExit.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item194");
                notifyIconMenuItemExit.Margin = new Thickness(0, 5, 0, 5);
                notifyIconMenuItemExit.PreviewMouseLeftButtonDown += (o, s) => { Environment.Exit(0); };

                notifyIconContextMenu.Items.Add(notifyIconMenuItemH1EmuServers);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemSingleplayer);
                notifyIconContextMenu.Items.Add(notifyIconItemSeparator);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemExit);

                List<ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(recentServersJsonFile));
                foreach (ServerList server in currentJsonRecent)
                {
                    MenuItem newItem = new();
                    newItem.Style = (Style)FindResource("CustomMenuItem");
                    newItem.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) };
                    newItem.Header = server.CustomServerName;
                    newItem.Margin = new Thickness(0, 5, 0, 0);
                    newItem.PreviewMouseLeftButtonDown += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, newItem);
                }

                if (notifyIconContextMenu.Items.Count > 4)
                {
                    Separator separator = new();
                    separator.Style = (Style)FindResource("SeparatorMenuItem");
                    separator.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                    separator.Margin = new Thickness(10, 7, 10, 2);
                    notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, separator);
                }

                // Load all of the servers into the server selector
                List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
                foreach (ServerList server in currentJson)
                {
                    ComboBoxItem newItem = new();
                    newItem.Content = server.CustomServerName;
                    newItem.Style = (Style)FindResource("ComboBoxItemStyle");
                    serverSelector.Items.Insert(serverSelector.Items.Count - 1, newItem);
                }

                if (serverSelector.Items.Count > 4)
                {
                    Separator separator = new();
                    separator.Style = (Style)FindResource("SeparatorMenuItem");
                    separator.Background = new SolidColorBrush(Color.FromRgb(66, 66, 66));
                    serverSelector.Items.Insert(serverSelector.Items.Count - 1, separator);
                }

                // Add an event for only user added servers in the list to delete on right click
                for (int i = 0; i <= serverSelector.Items.Count - 1; i++)
                {
                    if (serverSelector.Items[i] is ComboBoxItem serverItem)
                    {
                        if (i > 1 && i < serverSelector.Items.Count - 2)
                        {
                            serverItem.PreviewMouseRightButtonUp += ItemRightMouseButtonUp;
                            ContextMenu deleteMenu = new();
                            deleteMenu.Style = (Style)FindResource("ContextMenuStyle");
                            serverItem.ContextMenu = deleteMenu;

                            MenuItem editOption = new();
                            editOption.Style = (Style)FindResource("CustomMenuItem");
                            editOption.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Edit.png", UriKind.Absolute)) };
                            editOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                            editOption.Click += EditServerInfo;

                            MenuItem deleteOption = new();
                            deleteOption.Style = (Style)FindResource("CustomMenuItem");
                            deleteOption.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Delete.png", UriKind.Absolute)) };
                            deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                            deleteOption.Margin = new Thickness(0, 5, 0, 0);
                            deleteOption.Click += DeleteServerFromList;

                            deleteMenu.Items.Add(editOption);
                            deleteMenu.Items.Add(deleteOption);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item184")} \"{e.Message}\".", this);
            }

            serverSelector.SelectedIndex = Properties.Settings.Default.lastServer;
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
                    serverItem.Style = (Style)FindResource("ComboBoxItemStyle");
            }
        }

        public ComboBoxItem itemRightClicked;
        public void ItemRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemRightClicked = (ComboBoxItem)sender;
            itemRightClicked.Style = (Style)FindResource("ComboBoxItemStyleSelected");
            System.Windows.Forms.Application.DoEvents();
        }

        public void EditServerInfo(object sender, RoutedEventArgs e)
        {
            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
            for (int i = currentJson.Count - 1; i >= 0; i--)
            {
                if (currentJson[i].CustomServerName == (string)itemRightClicked.Content)
                {
                    CustomMessageBox.EditServer(this, i, currentJson[i].CustomServerName, currentJson[i].CustomServerIp);
                    serverSelector.IsDropDownOpen = false;
                    break;
                }
            }
        }

        public void DeleteServerFromList(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = CustomMessageBox.Show(FindResource("item147").ToString(), this, true, true, false, false);
            if (dr != MessageBoxResult.Yes)
                return;

            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
            for (int i = currentJson.Count - 1; i >= 0; i--)
            {
                if (currentJson[i].CustomServerName == (string)itemRightClicked.Content) 
                {
                    currentJson.Remove(currentJson[i]);
                    serverSelector.SelectedIndex = i + 1;
                    break;
                }
            }

            List<ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(recentServersJsonFile));
            for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
            {
                if (currentJsonRecent[i].CustomServerName == (string)itemRightClicked.Content)
                {
                    currentJsonRecent.Remove(currentJsonRecent[i]);
                    for (int j = notifyIconContextMenu.Items.Count - 1; j >= 0; j--)
                    {
                        if (notifyIconContextMenu.Items[j] is not MenuItem)
                            continue;

                        MenuItem item = (MenuItem)notifyIconContextMenu.Items[j];
                        if ((string)item.Header == (string)itemRightClicked.Content)
                            notifyIconContextMenu.Items.Remove(item);
                    }
                }
            }

            string finalJsonServers = JsonSerializer.Serialize(currentJson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(customServersJsonFile, finalJsonServers);

            string finalJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(recentServersJsonFile, finalJsonRecentServers);

            serverSelector.Items.Remove(itemRightClicked);

            if (notifyIconContextMenu.Items.Count == 5)
                notifyIconContextMenu.Items.RemoveAt(notifyIconContextMenu.Items.Count - 2);

            if (serverSelector.Items.Count == 5)
                serverSelector.Items.RemoveAt(serverSelector.Items.Count - 2);
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

                if (gameVersionString == "22dec2016")
                    serverVersion = "npm run start-2016";
                else if (gameVersionString == "kotk")
                {
                    //serverVersion = "npm run start-2016";
                }

                Process startSingleplayerServerProcess = new Process { StartInfo = cmdShell };
                startSingleplayerServerProcess.Start();
                using (StreamWriter sw = startSingleplayerServerProcess.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master\\node-v{Info.NODEJS_VERSION}-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine(serverVersion);
                    }
                }
                startSingleplayerServerProcess.WaitForExit(5000);

                if (startSingleplayerServerProcess.HasExited)
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
            if (!CheckDirectory())
                return;

            int gameVersionInt = 0;
            string serverIp = string.Empty;
            string sessionId = string.Empty;

            new Thread(() =>
            {
                try
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (serverSelector.SelectedIndex == 1)
                            serverIp = "localhost:1115";
                        else if (serverSelector.SelectedIndex != 0 && serverSelector.SelectedIndex != 1)
                        {
                            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
                            foreach (ServerList item in currentJson)
                            {
                                if (item.CustomServerName == serverSelector.Text)
                                    serverIp = item.CustomServerIp;
                            }
                        }
                    }));
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
                    CheckGameVersion();
                    switch (gameVersionString)
                    {
                        case "22dec2016":
                            gameVersionInt = 2;
                            break;
                        case "kotk":
                            gameVersionInt = 3;
                            break;
                    }

                    if (gameVersionString == "22dec2016" || gameVersionString == "kotk")
                    {
                        if (string.IsNullOrEmpty(serverIp))
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
                                // Check the users account key validity to decide whether to let the user connect to the H1Emu login server
                                //if (!CheckAccountKey.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey))
                                //return;

                                sessionId = $"{{\"sessionId\":\"{Properties.Settings.Default.sessionIdKey}\",\"gameVersion\":{gameVersionInt}}}";
                            }
                        }
                        else
                            sessionId = $"{{\"sessionId\":\"0\",\"gameVersion\":{gameVersionInt}}}";

                        if (serverIp == "localhost:1115")
                        {
                            if (!LaunchLocalServer(gameVersionString))
                                return;
                        }

                        ApplyPatchClass.CheckPatch();

                        //Start voice chat client
                        /*Process voiceChatClient = new();
                        if (gameVersionString == "22dec2016")
                        {
                            voiceChatClient.StartInfo = new ProcessStartInfo
                            {
                                FileName = $"{Properties.Settings.Default.activeDirectory}\\H1EmuVoice\\H1EmuVoice.exe",
                                WindowStyle = ProcessWindowStyle.Normal,
                                WorkingDirectory = $"{Properties.Settings.Default.activeDirectory}\\H1EmuVoice",
                                UseShellExecute = true
                            };
                            voiceChatClient.Start();
                        }*/

                        // Start game
                        Process h1Process = new();
                        h1Process.StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{Properties.Settings.Default.activeDirectory}\\H1Z1.exe",
                            Arguments = $"sessionid={sessionId} gamecrashurl={Info.GAME_CRASH_URL} server={serverIp}",
                            WindowStyle = ProcessWindowStyle.Normal,
                            WorkingDirectory = Properties.Settings.Default.activeDirectory,
                            UseShellExecute = true
                        };
                        h1Process.EnableRaisingEvents = true;
                        h1Process.Exited += (o, s) =>
                        {
                            if (Visibility == Visibility.Hidden)
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    Show();
                                    Activate();
                                    launcherNotifyIcon.Visible = false;

                                    Process[] processes = Process.GetProcesses();
                                    foreach (Process process in processes) 
                                    {
                                        if (process.ProcessName == "H1EmuVoice")
                                            process.Kill();
                                    }
                                }));
                            }
                        };
                        h1Process.Start();

                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (serverSelector.SelectedIndex != 0 && serverSelector.SelectedIndex != 1 && serverSelector.SelectedIndex != serverSelector.Items.Count - 1 && serverSelector.SelectedItem is ComboBoxItem)
                                AddServerToRecentList(serverSelector.Text, serverIp);
                        }));

                        if (Properties.Settings.Default.autoMinimise && Visibility == Visibility.Visible)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Hide();
                                launcherNotifyIcon.Visible = true;
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
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                        }));
                    }
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show($"{FindResource("item13")}\n\n{er.GetType().Name}: \"{er.Message}\".", this);
                    }));
                }
                finally
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        playButton.IsEnabled = true;
                        playButton.Content = FindResource("item8").ToString();
                    }));
                }

            }).Start();
        }

        private void AddServerToRecentList(string name, string ip)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                try
                {
                    List<ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(recentServersJsonFile));
                    for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                    {
                        if (currentJsonRecent[i].CustomServerName == serverSelector.Text)
                        {
                            currentJsonRecent.Remove(currentJsonRecent[i]);
                            for (int j = notifyIconContextMenu.Items.Count - 1; j >= 0; j--)
                            {
                                if (notifyIconContextMenu.Items[j] is MenuItem item)
                                {
                                    if ((string)item.Header == name)
                                        notifyIconContextMenu.Items.Remove(item);
                                }
                            }
                        }
                    }

                    currentJsonRecent.Add(new ServerList()
                    {
                        CustomServerName = name.Trim(),
                        CustomServerIp = ip.Trim().Replace(" ", "")
                    });

                    string newJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(recentServersJsonFile, newJsonRecentServers);

                    MenuItem newItem = new MenuItem();
                    newItem.Style = (Style)FindResource("CustomMenuItem");
                    newItem.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) };
                    newItem.Header = name;
                    newItem.Margin = new Thickness(0, 5, 0, 0);
                    newItem.PreviewMouseLeftButtonDown += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(3, newItem);

                    if (notifyIconContextMenu.Items.Count == 5)
                    {
                        Separator separator = new Separator();
                        separator.Style = (Style)FindResource("SeparatorMenuItem");
                        separator.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                        separator.Margin = new Thickness(0, 7, 10, 2);
                        notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, separator);
                    }

                    if (notifyIconContextMenu.Items.Count > 10)
                    {
                        MenuItem item = (MenuItem)notifyIconContextMenu.Items[notifyIconContextMenu.Items.Count - 3];
                        for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                        {
                            if (currentJsonRecent[i].CustomServerName == (string)item.Header)
                                currentJsonRecent.Remove(currentJsonRecent[i]);
                        }

                        notifyIconContextMenu.Items.RemoveAt(notifyIconContextMenu.Items.Count - 3);
                        newJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(recentServersJsonFile, newJsonRecentServers);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", this);
                }
            }));
        }

        private void LaunchToCustomServerFromNotifyIcon(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MenuItem clickedMenuItem = (MenuItem)sender;
                for (int i = 0; i <= serverSelector.Items.Count - 1; i++)
                {
                    if (serverSelector.Items[i] is not ComboBoxItem)
                        continue;

                    ComboBoxItem serverSelectorItem = (ComboBoxItem)serverSelector.Items[i];
                    if ((string)serverSelectorItem.Content == (string)clickedMenuItem.Header)
                    {
                        serverSelector.SelectedIndex = i;
                        playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", this);
            }
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

            DisplayVersionInformation();
            LoadServers();
            Carousel.BeginImageCarousel();

            new Thread(() =>
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) && Directory.Exists(Properties.Settings.Default.activeDirectory) && File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    }));
                }
                else if (!string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) && Directory.Exists(Properties.Settings.Default.activeDirectory) && !File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                        currentGame.Text = FindResource("item69").ToString();
                    }));
                    return;
                }
                else
                {
                    Properties.Settings.Default.activeDirectory = "Directory";
                    Properties.Settings.Default.Save();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        directoryBox.Text = FindResource("item75").ToString();
                        currentGame.Text = FindResource("item69").ToString();
                    }));
                    return;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    currentGame.Text = FindResource("item70").ToString();
                }));

                CheckGameVersion();

                if (gameVersionString == "processBeingUsed")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, false, true);
                    }));
                }
                else if (gameVersionString != "22dec2016" && gameVersionString != "kotk")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = FindResource("item72").ToString();
                    }));
                }

            }).Start();
        }

        private void LauncherWindowContentRendered(object sender, EventArgs e)
        {
            if (executeArguments)
                ExecuteArguments();
        }

        public void DisplayVersionInformation()
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

        public void SelectDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog selectDirectory = new();

            if (selectDirectory.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            Properties.Settings.Default.activeDirectory = selectDirectory.SelectedPath;
            Properties.Settings.Default.Save();

            if (!string.IsNullOrEmpty(selectDirectory.SelectedPath))
                directoryBox.Text = Properties.Settings.Default.activeDirectory;
            else
                directoryBox.Text = FindResource("item75").ToString();

            if (!CheckDirectory())
                return;

            new Thread(() =>
            {
                CheckGameVersion();

                switch (gameVersionString)
                {
                    case "22dec2016":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item74").ToString(), SettingsWindow.settingsInstance);
                        }));

                        break;
                    case "kotk":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item190").ToString(), SettingsWindow.settingsInstance);
                        }));

                        break;
                    case "processBeingUsed":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, false, true);
                        }));

                        break;
                    default:

                        Dispatcher.Invoke(new Action(delegate
                        {
                            currentGame.Text = FindResource("item72").ToString();
                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance);
                        }));

                        break;
                }

            }).Start();
        }

        public bool CheckDirectory()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    currentGame.Text = FindResource("item69").ToString();

                    if (this != null && Visibility == Visibility.Visible)
                        CustomMessageBox.Show(FindResource("item14").ToString(), SettingsWindow.settingsInstance);
                    else
                        CustomMessageBox.Show($"{FindResource("item14")}\n\n{FindResource("item9")}", LauncherWindow.launcherInstance);
                }));
                return false;
            }
            return true;
        }

        public void OpenDirectory(object sender, RoutedEventArgs e)
        {
            if (!CheckDirectory())
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = Properties.Settings.Default.activeDirectory,
                UseShellExecute = true
            });
        }

        public void CheckGameVersion()
        {
            gameVersionString = string.Empty;

            Dispatcher.Invoke(new Action(delegate
            {
                taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                directoryButton.IsEnabled = false;
            }));

            Dispatcher.Invoke(new Action(delegate
            {
                currentGame.Text = FindResource("item70").ToString();
                taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            }));

            Crc32 crc32 = new();
            string hash = string.Empty;

            try
            {
                using FileStream fs = File.Open($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe", FileMode.Open);
                foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();
            }
            catch (IOException)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    currentGame.Text = FindResource("item120").ToString();
                    directoryButton.IsEnabled = true;
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                }));

                gameVersionString = "processBeingUsed";
                return;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    directoryButton.IsEnabled = true;
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                }));
                return;
            }

            switch (hash)
            {
                case "bc5b3ab6": // Just Survive: 22nd December 2016
                    gameVersionString = "22dec2016";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} 2016";
                    }));

                    break;
                case "ec7ffa43": // King of the Kill: 23rd February 2017
                    gameVersionString = "kotk";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} KotK";
                    }));

                    break;
                default:
                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = FindResource("item69").ToString();
                    }));

                    break;
            }

            Dispatcher.Invoke(new Action(delegate
            {
                taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                directoryButton.IsEnabled = true;
            }));
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
            Hide();
            launcherNotifyIcon.Visible = true;
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