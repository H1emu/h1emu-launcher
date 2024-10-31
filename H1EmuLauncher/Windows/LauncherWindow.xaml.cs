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
        public static bool systemWatcherFire = true;
        public static bool executeArguments;

        public Storyboard CarouselNextAnimation;
        public Storyboard CarouselNextAnimationFollow;
        public Storyboard CarouselPreviousAnimation;
        public Storyboard CarouselPreviousAnimationFollow;

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

        public async void ExecuteArguments()
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
                // Close every other window apart from the Launcher and Add Server window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.Name != Name && window.Name != "AddServer")
                        window.Close();
                }

                string newServerName = SteamFramePages.Login.GetParameter(rawArgs, "-servername", "");
                string newServerIp = SteamFramePages.Login.GetParameter(rawArgs, "-serverip", "");

                if (AddServerWindow.addServerInstance == null)
                {
                    AddServerWindow addServer = new();
                    addServer.serverNameBox.Text = newServerName;
                    addServer.serverIpBox.Text = newServerIp;
                    addServer.serverNameHint.Visibility = Visibility.Hidden;
                    addServer.serverIpHint.Visibility = Visibility.Hidden;
                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            addServer.ShowDialog();
                        }));
                    });
                }
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
                    SettingsWindow sw = new();
                    sw.settingsTabControl.SelectedIndex = 1;
                    await Task.Run(() => 
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            sw.ShowDialog();
                        }));
                    });
                }
                else
                    SettingsWindow.SwitchToAccountKeyTab();
            }

            File.WriteAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", "");
            rawArgs = null;
        }

        private void ServerSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!serverSelector.IsLoaded)
                return;

            if (serverSelector.SelectedIndex == serverSelector.Items.Count - 1 ||
                serverSelector.SelectedIndex == serverSelector.Items.Count - 2 ||
                serverSelector.SelectedIndex == 2 || serverSelector.SelectedIndex == -1)
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

                MenuItem notifyIconMenuItemH1EmuServers = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) },
                    Margin = new Thickness(0, 5, 0, 0)
                };
                notifyIconMenuItemH1EmuServers.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item139");
                notifyIconMenuItemH1EmuServers.PreviewMouseLeftButtonDown += (o, s) =>
                {
                    serverSelector.SelectedIndex = 0;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                MenuItem notifyIconMenuItemSingleplayer = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) },
                    Margin = new Thickness(0, 5, 0, 0)
                };
                notifyIconMenuItemSingleplayer.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item140");
                notifyIconMenuItemSingleplayer.PreviewMouseLeftButtonDown += (o, s) =>
                {
                    serverSelector.SelectedIndex = 1;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                Separator notifyIconItemSeparator = new()
                {
                    Style = (Style)FindResource("SeparatorMenuItem"),
                    Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                    Margin = new Thickness(10, 7, 10, 2)
                };

                MenuItem notifyIconMenuItemExit = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Delete.png", UriKind.Absolute)) },
                    Margin = new Thickness(0, 5, 0, 5)
                };
                notifyIconMenuItemExit.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item194");
                notifyIconMenuItemExit.PreviewMouseLeftButtonDown += (o, s) => { Close(); };

                notifyIconContextMenu.Items.Add(notifyIconMenuItemH1EmuServers);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemSingleplayer);
                notifyIconContextMenu.Items.Add(notifyIconItemSeparator);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemExit);

                List<ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(recentServersJsonFile));
                foreach (ServerList server in currentJsonRecent)
                {
                    MenuItem newItem = new()
                    {
                        Style = (Style)FindResource("CustomMenuItem"),
                        Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) },
                        Header = server.CustomServerName,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    newItem.PreviewMouseLeftButtonDown += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, newItem);
                }

                if (notifyIconContextMenu.Items.Count > 4)
                {
                    Separator separator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                        Margin = new Thickness(10, 7, 10, 2)
                    };
                    notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, separator);
                }

                // Load all of the servers into the server selector
                List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
                foreach (ServerList server in currentJson)
                {
                    ComboBoxItem newItem = new()
                    {
                        Content = server.CustomServerName,
                        Style = (Style)FindResource("ComboBoxItemStyle")
                    };
                    serverSelector.Items.Insert(serverSelector.Items.Count - 1, newItem);
                }

                if (serverSelector.Items.Count > 4)
                {
                    Separator separator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66))
                    };
                    serverSelector.Items.Insert(serverSelector.Items.Count - 1, separator);
                }

                // Add an event for only user added servers in the list to delete on right click
                for (int i = 0; i <= serverSelector.Items.Count - 1; i++)
                {
                    if (serverSelector.Items[i] is ComboBoxItem serverItem && i > 1 && i < serverSelector.Items.Count - 2)
                    {
                        ContextMenu itemContextMenu = new()
                        {
                            Style = (Style)FindResource("ContextMenuStyle")
                        };
                        serverItem.ContextMenu = itemContextMenu;

                        MenuItem editOption = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                            Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Edit.png", UriKind.Absolute)) }
                        };
                        editOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                        editOption.Click += (s, e) => { EditServerInfo(serverItem); };

                        Separator separator = new()
                        {
                            Style = (Style)FindResource("SeparatorMenuItem"),
                            Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                            Margin = new Thickness(10, 7, 10, 7)
                        };

                        MenuItem deleteOption = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                            Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Delete.png", UriKind.Absolute)) },
                        };
                        deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                        deleteOption.Click += (s, e) => { DeleteServer(serverItem); };

                        itemContextMenu.Items.Add(editOption);
                        itemContextMenu.Items.Add(separator);
                        itemContextMenu.Items.Add(deleteOption);
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
            AddServerWindow addServer = new();
            addServer.ShowDialog();
        }

        public async void EditServerInfo(ComboBoxItem serverItem)
        {
            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
            for (int i = currentJson.Count - 1; i >= 0; i--)
            {
                if (currentJson[i].CustomServerName == (string)serverItem.Content)
                {
                    AddServerWindow editServer = new();
                    editServer.serverNameBox.Text = currentJson[i].CustomServerName;
                    editServer.serverIpBox.Text = currentJson[i].CustomServerIp;
                    editServer.serverNameHint.Visibility = Visibility.Hidden;
                    editServer.serverIpHint.Visibility = Visibility.Hidden;
                    editServer.saveServerButton.SetResourceReference(ContentProperty, "item213");
                    editServer.editIndex = i;
                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            editServer.ShowDialog();
                        }));
                    });

                    serverSelector.IsDropDownOpen = false;
                    break;
                }
            }
        }

        public void DeleteServer(ComboBoxItem serverItem)
        {
            MessageBoxResult dr = CustomMessageBox.Show(FindResource("item147").ToString(), this, true, true, false, false);
            if (dr != MessageBoxResult.Yes)
                return;

            // Delete the server from the custom servers file list
            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
            for (int i = currentJson.Count - 1; i >= 0; i--)
            {
                if (currentJson[i].CustomServerName == (string)serverItem.Content) 
                {
                    currentJson.Remove(currentJson[i]);
                    if (serverSelector.SelectedItem == serverItem)
                    {
                        if (i + 4 < serverSelector.Items.Count - 2)
                            serverSelector.SelectedIndex = i + 4;
                        else
                            serverSelector.SelectedIndex = i + 2;
                    }
                    break;
                }
            }

            string finalJsonServers = JsonSerializer.Serialize(currentJson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(customServersJsonFile, finalJsonServers);

            // Delete the server from the recent servers file list, used for the system tray icon context menu
            List<ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(recentServersJsonFile));
            for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
            {
                if (currentJsonRecent[i].CustomServerName == (string)serverItem.Content)
                {
                    currentJsonRecent.Remove(currentJsonRecent[i]);
                    for (int j = notifyIconContextMenu.Items.Count - 1; j >= 0; j--)
                    {
                        if (notifyIconContextMenu.Items[j] is not MenuItem)
                            continue;

                        MenuItem item = (MenuItem)notifyIconContextMenu.Items[j];
                        if ((string)item.Header == (string)serverItem.Content)
                            notifyIconContextMenu.Items.Remove(item);
                    }
                }
            }

            string finalJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(recentServersJsonFile, finalJsonRecentServers);

            serverSelector.Items.Remove(serverItem);

            if (notifyIconContextMenu.Items.Count == 5)
                notifyIconContextMenu.Items.RemoveAt(notifyIconContextMenu.Items.Count - 2);

            if (serverSelector.Items.Count == 5)
                serverSelector.Items.RemoveAt(serverSelector.Items.Count - 2);
        }

        Process startSingleplayerServerProcess;
        public bool LaunchLocalServer()
        {
            try
            {
                if (!Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master\\node_modules"))
                {
                    MessageBoxResult mbr = MessageBoxResult.None;
                    Dispatcher.Invoke(new Action(delegate
                    {
                        mbr = CustomMessageBox.InstallServerInline(FindResource("item52").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                    }));

                    if (mbr != MessageBoxResult.Yes)
                        return false;
                }

                startSingleplayerServerProcess = new Process { StartInfo = cmdShell };
                startSingleplayerServerProcess.Start();
                using (StreamWriter sw = startSingleplayerServerProcess.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master\\node-v{Info.NODEJS_VERSION}-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine("npm run start-2016");
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
            catch (Exception e)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    CustomMessageBox.Show($"{FindResource("item53")} \"{e.Message}\"", this);
                }));
                return false;
            }

            return true;
        }

        private void LaunchClient(object sender, RoutedEventArgs e)
        {
            playButton.IsEnabled = false;
            if (!CheckGameVersionAndPath(this))
            {
                playButton.IsEnabled = true;
                return;
            }

            string serverIp = string.Empty;
            string sessionId = string.Empty;
            int serverIndex = serverSelector.SelectedIndex;

            new Thread(async () =>
            {
                try
                {
                    switch (serverIndex)
                    {
                        case 0:
                            // sessionIdKey is the same as accountKey, not possible change the name without resetting users settings
                            // If connecting to H1Emu servers, check is an Account Key is set
                            if (string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item153").ToString(), this, true, true, false, false);

                                    if (mbr != MessageBoxResult.Yes)
                                        throw new Exception("emptyAccountKey");
                                    else
                                        throw new Exception("createAccountKey");
                                }));
                            }

                            // If connecting to H1Emu servers, check Account Key validity
                            await CheckAccountKey.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey);

                            sessionId = $"{{\"sessionId\":\"{Properties.Settings.Default.sessionIdKey}\",\"gameVersion\":2}}";
                            serverIp = Info.H1EMU_SERVER_IP;
                            break;

                        case 1:
                            if (!LaunchLocalServer())
                                throw new Exception("launchLocalServerFailed");

                            sessionId = $"{{\"sessionId\":\"0\",\"gameVersion\":2}}";
                            serverIp = "localhost:1115";
                            break;

                        default:
                            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
                            foreach (ServerList item in currentJson)
                            {
                                Dispatcher.Invoke(new Action(delegate
                                {
                                    if (item.CustomServerName == serverSelector.Text)
                                    {
                                        sessionId = $"{{\"sessionId\":\"0\",\"gameVersion\":2}}";
                                        serverIp = item.CustomServerIp;
                                    }
                                }));
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    switch (e.Message)
                    {
                        case "emptyAccountKey":
                        case "launchLocalServerFailed":
                            break;

                        case "createAccountKey":
                            Dispatcher.Invoke(new Action(delegate
                            {
                                SettingsWindow sw = new();
                                sw.settingsTabControl.SelectedIndex = 1;
                                sw.ShowDialog();
                            }));
                            break;

                        default:
                            Dispatcher.Invoke(new Action(delegate
                            {
                                CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\"", this);
                            }));
                            break;
                    }

                    Dispatcher.Invoke(new Action(delegate
                    {
                        playButton.IsEnabled = true;
                    }));
                    return;
                }

                try
                {
                    // Check that the patch is the latest version
                    ApplyPatchClass.CheckPatch();

                    // If connecting to H1Emu servers, validate assets
                    //if (serverIp == Info.H1EMU_SERVER_IP)
                        //CheckAssetsBeforeLaunch.CheckAssets();

                    // Launch game
                    Process h1Process = new()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{Properties.Settings.Default.activeDirectory}\\H1Z1.exe",
                            Arguments = $"sessionid={sessionId} gamecrashurl={Info.GAME_CRASH_URL} server={serverIp}",
                            WindowStyle = ProcessWindowStyle.Normal,
                            WorkingDirectory = Properties.Settings.Default.activeDirectory,
                            UseShellExecute = true,
                            Verb = "runas"
                        },
                        EnableRaisingEvents = true
                    };
                    h1Process.Exited += (o, s) =>
                    {
                        if (Visibility == Visibility.Hidden)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                Show();
                                Activate();
                                launcherNotifyIcon.Visible = false;
                            }));
                        }

                        Dispatcher.Invoke(new Action(delegate
                        {
                            playButton.IsEnabled = true;
                            playButton.SetResourceReference(ContentProperty, "item8");
                        }));

                        if (startSingleplayerServerProcess != null)
                            startSingleplayerServerProcess.Kill(true);
                    };
                    h1Process.Start();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        playButton.SetResourceReference(ContentProperty, "item217");
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
                catch (Exception e)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        playButton.IsEnabled = true;
                        playButton.SetResourceReference(ContentProperty, "item8");
                        CustomMessageBox.Show($"{FindResource("item13")}\n\n{e.GetType().Name}: \"{e.Message}\".", this);
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

                    MenuItem newItem = new()
                    {
                        Style = (Style)FindResource("CustomMenuItem"),
                        Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Play.png", UriKind.Absolute)) },
                        Header = name,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    newItem.PreviewMouseLeftButtonDown += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(3, newItem);

                    if (notifyIconContextMenu.Items.Count == 5)
                    {
                        Separator separator = new()
                        {
                            Style = (Style)FindResource("SeparatorMenuItem"),
                            Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                            Margin = new Thickness(0, 7, 10, 2)
                        };
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
                catch (Exception e)
                {
                    CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
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
            // Delete old setup file
            if (File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{UpdateWindow.downloadFileName}"))
                File.Delete($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{UpdateWindow.downloadFileName}");

            // If arguments file doesn't exist then create one
            if (!File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt"))
                File.Create($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt");

            // Delete old carousel images folder, no longer needed on newer versions of the launcher
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

            if (Properties.Settings.Default.language == 1)
                chineseLink.Visibility = Visibility.Visible;

            DisplayVersionInformation();
            LoadServers();
            Carousel.BeginImageCarousel();
            CheckGameVersionAndPath(this, false, true);
        }

        private async void LauncherWindowContentRendered(object sender, EventArgs e)
        {
            if (executeArguments)
                ExecuteArguments();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                await CheckAccountKey.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey);
        }

        public void DisplayVersionInformation()
        {
            try
            {
                // Update version, date published and patch notes code
                HttpResponseMessage result = UpdateWindow.httpClient.GetAsync(Info.SERVER_JSON_API).Result;

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

            // Animation for fade in visibility next image button
            DoubleAnimation showImageControls = new(0, 1, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)))
            {
                AccelerationRatio = 0.2,
                DecelerationRatio = 0.2
            };
            nextImage.BeginAnimation(Window.OpacityProperty, showImageControls);
            prevImage.BeginAnimation(Window.OpacityProperty, showImageControls);
        }

        private void CarouselMouseLeave(object sender, MouseEventArgs e)
        {
            if (Carousel.playCarousel == null)
                return;

            Carousel.playCarousel.Resume();

            // Animation for fade out visibility next image button
            DoubleAnimation hideImageControls = new(1, 0, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)))
            {
                AccelerationRatio = 0.2,
                DecelerationRatio = 0.2
            };
            nextImage.BeginAnimation(Window.OpacityProperty, hideImageControls);
            prevImage.BeginAnimation(Window.OpacityProperty, hideImageControls);
        }

        public void SelectDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog selectDirectory = new();
            if (selectDirectory.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            Properties.Settings.Default.activeDirectory = selectDirectory.SelectedPath;
            Properties.Settings.Default.Save();

            new Thread(() =>
            {
                CheckGameVersionAndPath(this, true);

            }).Start();
        }

        public void OpenDirectory(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Properties.Settings.Default.activeDirectory))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = Properties.Settings.Default.activeDirectory,
                UseShellExecute = true
            });
        }

        public bool CheckGameVersionAndPath(Window callingWindow, bool alerts = false, bool silent = false)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                currentGame.SetResourceReference(TextBlock.TextProperty, "item70");
                taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                directoryButton.IsEnabled = false;
            }));

            Properties.Settings.Default.gameVersionString = string.Empty;
            Properties.Settings.Default.Save();

            if (!Directory.Exists(Properties.Settings.Default.activeDirectory))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    Properties.Settings.Default.activeDirectory = "Directory";
                    Properties.Settings.Default.Save();

                    directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    currentGame.SetResourceReference(TextBlock.TextProperty, "item69");
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    directoryButton.IsEnabled = true;
                }));
                
                return false;
            }
            else if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    currentGame.SetResourceReference(TextBlock.TextProperty, "item69");
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    directoryButton.IsEnabled = true;

                    if (!silent)
                    {
                        if (SettingsWindow.settingsInstance != null && SettingsWindow.settingsInstance.Visibility == Visibility.Visible)
                            CustomMessageBox.Show(FindResource("item14").ToString(), callingWindow);
                        else
                            CustomMessageBox.Show($"{FindResource("item14")}\n\n{FindResource("item9")}", callingWindow);
                    }
                }));

                return false;
            }

            Crc32 crc32 = new();
            string hash = string.Empty;

            try
            {
                using FileStream fs = File.Open($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe", FileMode.Open);
                foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();
            }
            catch (IOException)
            {
                Properties.Settings.Default.gameVersionString = "processBeingUsed";
                Properties.Settings.Default.Save();

                Dispatcher.Invoke(new Action(delegate
                {
                    directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    currentGame.SetResourceReference(TextBlock.TextProperty, "item120");
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    directoryButton.IsEnabled = true;
                    
                    if (!silent)
                        CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), callingWindow, false, false, true);
                }));

                return false;
            }
            catch (Exception e)
            {
                Properties.Settings.Default.gameVersionString = string.Empty;
                Properties.Settings.Default.Save();

                Dispatcher.Invoke(new Action(delegate
                {
                    directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    currentGame.SetResourceReference(TextBlock.TextProperty, "item148");
                    taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    directoryButton.IsEnabled = true;

                    if (!silent)
                        CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\".", callingWindow);
                }));

                return false;
            }

            switch (hash)
            {
                case "bc5b3ab6": // Just Survive: 22nd December 2016
                    Properties.Settings.Default.gameVersionString = "22dec2016";
                    Properties.Settings.Default.Save();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                        currentGame.SetResourceReference(TextBlock.TextProperty, "item122");
                        taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        directoryButton.IsEnabled = true;

                        if (alerts)
                            CustomMessageBox.Show(FindResource("item74").ToString(), callingWindow);
                    }));

                    return true;
                default:
                    Properties.Settings.Default.gameVersionString = hash;
                    Properties.Settings.Default.Save();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                        currentGame.SetResourceReference(TextBlock.TextProperty, "item72");
                        taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        directoryButton.IsEnabled = true;

                        if (!silent)
                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), callingWindow);
                    }));

                    return false;
            }
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new();
            sw.ShowDialog();
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

        private void discordLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.DISCORD_LINK);
        }

        private void chineseLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.CHANGELOG);
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
            Close();
        }
    }
}