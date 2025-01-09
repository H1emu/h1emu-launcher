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
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Data;
using Microsoft.Win32;
using Microsoft.Toolkit.Uwp.Notifications;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class LauncherWindow : Window
    {
        private readonly System.Windows.Forms.NotifyIcon launcherNotifyIcon = new();
        private readonly ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        public static JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
        public static LauncherWindow launcherInstance;
        public static ContextMenu notifyIconContextMenu = new();
        public static string customServersJsonFile = $"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\servers.json";
        public static string recentServersJsonFile = $"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\recentServers.json";

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

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            CarouselNextAnimation = FindResource("CarouselNextImageAnimation") as Storyboard;
            CarouselNextAnimationFollow = FindResource("CarouselNextImageAnimationFollow") as Storyboard;
            CarouselPreviousAnimation = FindResource("CarouselPrevImageAnimation") as Storyboard;
            CarouselPreviousAnimationFollow = FindResource("CarouselPrevImageAnimationFollow") as Storyboard;

            launcherNotifyIcon.Icon = Properties.Resources.Icon;
            launcherNotifyIcon.Text = "H1Emu Launcher";
            launcherNotifyIcon.MouseDown += (o, s) =>
            {
                if (s.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Show();
                    Activate();
                }
                else if (s.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    notifyIconContextMenu.IsOpen = true;
                    // Get context menu handle and bring it to the foreground
                    if (PresentationSource.FromVisual(notifyIconContextMenu) is HwndSource hwndSource)
                        SetForegroundWindow(hwndSource.Handle);
                }
            };

            ulong bytesReadLastUpdated = 0;
            ContentDownloader.downloadSpeedTimer.Elapsed += (s, e) =>
            {
                ulong difference = ContentDownloader.sizeDownloadedPublic - bytesReadLastUpdated;
                bytesReadLastUpdated = ContentDownloader.sizeDownloadedPublic;
                ContentDownloader.downloadSpeed = (float)difference / (1024 * 1024);
            };
        }

        public class ServerList
        {
            [JsonPropertyName("Server Name")]
            public string CustomServerName { get; set; }

            [JsonPropertyName("Server Address")]
            public string CustomServerIp { get; set; }
        }

        public class ServerListRecent
        {
            [JsonPropertyName("Recent Server Name")]
            public string CustomServerNameRecent { get; set; }
        }

        public async Task ExecuteArguments(string[] rawArgs)
        {
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;

            Show();
            Activate();

            // If there are no args then return after the launcher was brought into focus
            if (rawArgs.Length == 0)
                return;

            // If the new server and ip arguments exist, open the Add Server window and tell it to fill in the name and ip fields with the specified argument values
            if (!string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-servername", "")) || !string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-serverip", "")))
            {
                // Close every other window apart from the Launcher and Add Server window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is not LauncherWindow && window is not AddOrEditServerWindow)
                        window.Close();
                }

                string newServerName = SteamFramePages.Login.GetParameter(rawArgs, "-servername", "");
                string newServerIp = SteamFramePages.Login.GetParameter(rawArgs, "-serverip", "");

                if (AddOrEditServerWindow.addServerInstance == null)
                {
                    AddOrEditServerWindow addServer = new();
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
                    AddOrEditServerWindow.addServerInstance.FillInFields(newServerName, newServerIp);

                newServerName = null;
                newServerIp = null;
                return;
            }

            // If the account key argument exists, open the Settings window and tell it to open the Account Key tab with accountkey argument value
            if (!string.IsNullOrEmpty(SteamFramePages.Login.GetParameter(rawArgs, "-accountkey", "")))
            {
                // Close every other window apart from the Launcher and Settings windows
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is not LauncherWindow && window is not SettingsWindow)
                        window.Close();
                }

                string newAccountKey = SteamFramePages.Login.GetParameter(rawArgs, "-accountkey", "");

                if (SettingsWindow.settingsInstance == null)
                {
                    SettingsWindow.newAccountKey = newAccountKey;
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
                    SettingsWindow.SwitchToAccountKeyTab(newAccountKey);

                newAccountKey = null;
                SettingsWindow.newAccountKey = null;
                return;
            }

            // If the launch game argument exists, then launch the game straight away
            if (string.Join(' ', rawArgs).Contains("-launchgame"))
            {
                playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                return;
            }
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

                MenuItem notifyIconMenuItemH1EmuServersPlay = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Margin = new Thickness(0, 0, 0, 6)
                };
                System.Windows.Shapes.Path pathH1EmuServersPlay = new()
                {
                    Data = (PathGeometry)FindResource("PlayIcon"),
                    Stretch = Stretch.Uniform,
                    Width = 14,
                    Height = 14
                };
                Binding bindingH1EmuServersPlay = new("Foreground")
                {
                    Source = notifyIconMenuItemH1EmuServersPlay,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathH1EmuServersPlay, System.Windows.Shapes.Path.StrokeProperty, bindingH1EmuServersPlay);

                notifyIconMenuItemH1EmuServersPlay.Icon = pathH1EmuServersPlay;
                notifyIconMenuItemH1EmuServersPlay.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item139");
                notifyIconMenuItemH1EmuServersPlay.Click += (o, s) =>
                {
                    serverSelector.SelectedIndex = 0;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                MenuItem notifyIconMenuItemSingleplayerPlay = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Margin = new Thickness(0, 0, 0, 6)
                };
                System.Windows.Shapes.Path pathSinglePlayerPlay = new()
                {
                    Data = (PathGeometry)FindResource("PlayIcon"),
                    Stretch = Stretch.Uniform,
                    Width = 14,
                    Height = 14
                };
                Binding bindingSinglePlayerPlay = new("Foreground")
                {
                    Source = notifyIconMenuItemSingleplayerPlay,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathSinglePlayerPlay, System.Windows.Shapes.Path.StrokeProperty, bindingSinglePlayerPlay);

                notifyIconMenuItemSingleplayerPlay.Icon = pathSinglePlayerPlay;
                notifyIconMenuItemSingleplayerPlay.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item140");
                notifyIconMenuItemSingleplayerPlay.Click += (o, s) =>
                {
                    serverSelector.SelectedIndex = 1;
                    playButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };

                Separator notifyIconItemSeparator = new()
                {
                    Style = (Style)FindResource("SeparatorMenuItem"),
                    Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                    Margin = new Thickness(10, 2, 10, 10)
                };

                MenuItem notifyIconMenuItemExit = new()
                {
                    Style = (Style)FindResource("CustomMenuItem")
                };
                System.Windows.Shapes.Path pathExitNotifyIcon = new()
                {
                    Data = (PathGeometry)FindResource("ExitIcon"),
                    Stretch = Stretch.Uniform,
                    Width = 14,
                    Height = 14,
                    Margin = new Thickness(1, 0, 0, 0)
                };
                Binding bindingExitNotifyIcon = new("Foreground")
                {
                    Source = notifyIconMenuItemExit,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathExitNotifyIcon, System.Windows.Shapes.Path.FillProperty, bindingExitNotifyIcon);

                notifyIconMenuItemExit.Icon = pathExitNotifyIcon;
                notifyIconMenuItemExit.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item194");
                notifyIconMenuItemExit.Click += (o, s) => { Close(); };

                notifyIconContextMenu.Items.Add(notifyIconMenuItemH1EmuServersPlay);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemSingleplayerPlay);
                notifyIconContextMenu.Items.Add(notifyIconItemSeparator);
                notifyIconContextMenu.Items.Add(notifyIconMenuItemExit);

                List<ServerListRecent> currentJsonRecent = JsonSerializer.Deserialize<List<ServerListRecent>>(File.ReadAllText(recentServersJsonFile));
                foreach (ServerListRecent server in currentJsonRecent)
                {
                    MenuItem playCustomServer = new()
                    {
                        Style = (Style)FindResource("CustomMenuItem"),
                        Margin = new Thickness(0, 0, 0, 6),
                        Header = server.CustomServerNameRecent
                    };
                    System.Windows.Shapes.Path pathCustomServerDelete = new()
                    {
                        Data = (PathGeometry)FindResource("PlayIcon"),
                        Stretch = Stretch.Uniform,
                        Width = 14,
                        Height = 14
                    };
                    Binding bindingCustomServerDelete = new("Foreground")
                    {
                        Source = playCustomServer,
                        Mode = BindingMode.OneWay
                    };
                    BindingOperations.SetBinding(pathCustomServerDelete, System.Windows.Shapes.Path.StrokeProperty, bindingCustomServerDelete);

                    playCustomServer.Icon = pathCustomServerDelete;
                    playCustomServer.Click += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, playCustomServer);
                }

                if (notifyIconContextMenu.Items.Count > 4)
                {
                    Separator separator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                        Margin = new Thickness(10, 2, 10, 10)
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

                        MenuItem editOptionCustom = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                            Margin = new Thickness(0, 0, 0, 6)
                        };
                        System.Windows.Shapes.Path pathCustom = new()
                        {
                            Data = (PathGeometry)FindResource("EditIcon"),
                            Stretch = Stretch.Uniform
                        };
                        Binding bindingCustom = new("Foreground")
                        {
                            Source = editOptionCustom,
                            Mode = BindingMode.OneWay
                        };
                        BindingOperations.SetBinding(pathCustom, System.Windows.Shapes.Path.FillProperty, bindingCustom);

                        editOptionCustom.Icon = pathCustom;
                        editOptionCustom.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                        editOptionCustom.Click += (s, e) => { EditServerInfo(serverItem); };

                        Separator separator = new()
                        {
                            Style = (Style)FindResource("SeparatorMenuItem"),
                            Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                            Margin = new Thickness(10, 2, 10, 10)
                        };

                        MenuItem deleteOptionCustom = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                        };
                        System.Windows.Shapes.Path pathDeleteCustom = new()
                        {
                            Data = (PathGeometry)FindResource("BinIcon"),
                            Stretch = Stretch.Uniform
                        };
                        Binding bindingDeleteCustom = new("Foreground")
                        {
                            Source = deleteOptionCustom,
                            Mode = BindingMode.OneWay
                        };
                        BindingOperations.SetBinding(pathDeleteCustom, System.Windows.Shapes.Path.FillProperty, bindingDeleteCustom);

                        deleteOptionCustom.Icon = pathDeleteCustom;
                        deleteOptionCustom.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                        deleteOptionCustom.Click += (s, e) => { DeleteServer(serverItem); };

                        itemContextMenu.Items.Add(editOptionCustom);
                        itemContextMenu.Items.Add(separator);
                        itemContextMenu.Items.Add(deleteOptionCustom);
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
            AddOrEditServerWindow addServer = new();
            addServer.ShowDialog();
        }

        public async void EditServerInfo(ComboBoxItem serverItem)
        {
            List<ServerList> currentJson = JsonSerializer.Deserialize<List<ServerList>>(File.ReadAllText(customServersJsonFile));
            for (int i = currentJson.Count - 1; i >= 0; i--)
            {
                if (currentJson[i].CustomServerName == (string)serverItem.Content)
                {
                    AddOrEditServerWindow editServer = new();
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
            MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item147").ToString(), this, false, true, true);
            if (mbr != MessageBoxResult.Yes)
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

            string newJson = JsonSerializer.Serialize(currentJson, jsonSerializerOptions);
            File.WriteAllText(customServersJsonFile, newJson);

            // Delete the server from the recent servers file list, used for the system tray icon context menu
            List<ServerListRecent> currentJsonRecent = JsonSerializer.Deserialize<List<ServerListRecent>>(File.ReadAllText(recentServersJsonFile));
            for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
            {
                if (currentJsonRecent[i].CustomServerNameRecent == (string)serverItem.Content)
                {
                    currentJsonRecent.Remove(currentJsonRecent[i]);
                    for (int j = notifyIconContextMenu.Items.Count - 1; j >= 0; j--)
                    {
                        if (notifyIconContextMenu.Items[j] is not MenuItem)
                            continue;

                        MenuItem item = (MenuItem)notifyIconContextMenu.Items[j];
                        if ((string)item.Header == (string)serverItem.Content)
                        {
                            notifyIconContextMenu.Items.Remove(item);
                            break;
                        }
                    }
                    break;
                }
            }

            string newJsonRecent = JsonSerializer.Serialize(currentJsonRecent, jsonSerializerOptions);
            File.WriteAllText(recentServersJsonFile, newJsonRecent);

            serverSelector.Items.Remove(serverItem);

            if (notifyIconContextMenu.Items.Count == 5)
                notifyIconContextMenu.Items.RemoveAt(notifyIconContextMenu.Items.Count - 2);

            if (serverSelector.Items.Count == 5)
                serverSelector.Items.RemoveAt(serverSelector.Items.Count - 2);
        }

        private void AddServerToRecentList(string name)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                try
                {
                    // Remove the item and add it back again so that the most recently played server is at the top of the list
                    List<ServerListRecent> currentJsonRecent = JsonSerializer.Deserialize<List<ServerListRecent>>(File.ReadAllText(recentServersJsonFile));
                    for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                    {
                        if (currentJsonRecent[i].CustomServerNameRecent == serverSelector.Text)
                        {
                            currentJsonRecent.Remove(currentJsonRecent[i]);
                            for (int j = notifyIconContextMenu.Items.Count - 1; j >= 0; j--)
                            {
                                if (notifyIconContextMenu.Items[j] is not MenuItem)
                                    continue;

                                MenuItem item = (MenuItem)notifyIconContextMenu.Items[j];
                                if ((string)item.Header == name)
                                {
                                    notifyIconContextMenu.Items.Remove(item);
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    currentJsonRecent.Add(new ServerListRecent()
                    {
                        CustomServerNameRecent = name
                    });

                    string newJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, jsonSerializerOptions);
                    File.WriteAllText(recentServersJsonFile, newJsonRecentServers);

                    MenuItem playCustomServer = new()
                    {
                        Style = (Style)FindResource("CustomMenuItem"),
                        Margin = new Thickness(0, 0, 0, 6),
                        Header = name
                    };
                    System.Windows.Shapes.Path pathCustomServerPlay = new()
                    {
                        Data = (PathGeometry)FindResource("PlayIcon"),
                        Stretch = Stretch.Uniform,
                        Width = 14,
                        Height = 14
                    };
                    Binding bindingCustomServerPlay = new("Foreground")
                    {
                        Source = playCustomServer,
                        Mode = BindingMode.OneWay
                    };
                    BindingOperations.SetBinding(pathCustomServerPlay, System.Windows.Shapes.Path.StrokeProperty, bindingCustomServerPlay);

                    playCustomServer.Icon = pathCustomServerPlay;
                    playCustomServer.Click += LaunchToCustomServerFromNotifyIcon;
                    notifyIconContextMenu.Items.Insert(3, playCustomServer);

                    if (notifyIconContextMenu.Items.Count == 5)
                    {
                        Separator separator = new()
                        {
                            Style = (Style)FindResource("SeparatorMenuItem"),
                            Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                            Margin = new Thickness(10, 2, 10, 10)
                        };
                        notifyIconContextMenu.Items.Insert(notifyIconContextMenu.Items.Count - 1, separator);
                    }

                    if (notifyIconContextMenu.Items.Count > 10)
                    {
                        MenuItem item = (MenuItem)notifyIconContextMenu.Items[notifyIconContextMenu.Items.Count - 3];
                        for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                        {
                            if (currentJsonRecent[i].CustomServerNameRecent == (string)item.Header)
                                currentJsonRecent.Remove(currentJsonRecent[i]);
                        }

                        notifyIconContextMenu.Items.RemoveAt(notifyIconContextMenu.Items.Count - 3);
                        newJsonRecentServers = JsonSerializer.Serialize(currentJsonRecent, jsonSerializerOptions);
                        File.WriteAllText(recentServersJsonFile, newJsonRecentServers);
                    }
                }
                catch (Exception e)
                {
                    CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
                }
            }));
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

        private async void LaunchClient(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.developerMode)
            {
                playButton.IsEnabled = false;
                playButton.SetResourceReference(ContentProperty, "item217");
            }

            if (!CheckGameVersionAndPath(this, false, true))
            {
                playButton.IsEnabled = true;
                playButton.SetResourceReference(ContentProperty, "item8");
                return;
            }

            string serverIp = string.Empty;
            string sessionId = string.Empty;
            int serverIndex = serverSelector.SelectedIndex;

            try
            {
                switch (serverIndex)
                {
                    case 0:
                        // sessionIdKey is the same as accountKey, not possible change the name without resetting users settings
                        // If connecting to H1Emu servers, check is an Account Key is set
                        if (string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                        {
                            MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item153").ToString(), this, false, true, true);

                            if (mbr != MessageBoxResult.Yes)
                                throw new Exception("emptyAccountKey");
                            else
                                throw new Exception("createAccountKey");
                        }

                        // If connecting to H1Emu servers, check Account Key validity
                        await AccountKeyUtil.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey);

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
                            if (item.CustomServerName == serverSelector.Text)
                            {
                                sessionId = $"{{\"sessionId\":\"{AccountKeyUtil.EncryptStringSHA256(Properties.Settings.Default.sessionIdKey)}\",\"gameVersion\":2}}";
                                serverIp = item.CustomServerIp;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "emptyAccountKey":
                    case "launchLocalServerFailed":
                        break;

                    case "createAccountKey":
                        SettingsWindow sw = new();
                        sw.settingsTabControl.SelectedIndex = 1;
                        sw.ShowDialog();
                        break;

                    default:
                        CustomMessageBox.Show($"{FindResource("item142")} \"{ex.Message}\"", this);
                        break;
                }

                playButton.IsEnabled = true;
                playButton.SetResourceReference(ContentProperty, "item8");
                return;
            }

            try
            {
                // Check that the patch is the latest version
                if (!Properties.Settings.Default.developerMode)
                {
                    if (!ApplyPatchClass.ApplyPatch())
                        return;
                }

                // Check that the launcher is the latest version
                if (SplashWindow.checkForUpdates)
                {
                    if (!await SplashWindow.CheckVersion(this))
                        return;
                }

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
                        Show();
                        Activate();
                    }

                    playButton.IsEnabled = true;
                    playButton.SetResourceReference(ContentProperty, "item8");

                    if (startSingleplayerServerProcess != null)
                        startSingleplayerServerProcess.Kill(true);
                };
                h1Process.Start();

                if (serverSelector.SelectedIndex != 0 && serverSelector.SelectedIndex != 1 && serverSelector.SelectedIndex != serverSelector.Items.Count - 1 && serverSelector.SelectedItem is ComboBoxItem)
                    AddServerToRecentList(serverSelector.Text);

                if (Properties.Settings.Default.autoMinimise && Visibility == Visibility.Visible)
                {
                    Hide();
                    new ToastContentBuilder().AddText(FindResource("item191").ToString()).Show();
                }
            }
            catch (Exception ex)
            {
                playButton.IsEnabled = true;
                playButton.SetResourceReference(ContentProperty, "item8");
                CustomMessageBox.Show($"{FindResource("item13")}\n\n{e.GetType().Name}: \"{ex.Message}\".", this);
            }
        }

        private void LaunchToCustomServerFromNotifyIcon(object sender, RoutedEventArgs e)
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

        private async void LauncherWindowLoaded(object sender, RoutedEventArgs e)
        {
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

            await DisplayVersionInformation();
            LoadServers();
            Carousel.BeginImageCarousel();
            CheckGameVersionAndPath(this, false, false);
        }

        public static string[] rawArgs;
        private async void LauncherWindowContentRendered(object sender, EventArgs e)
        {
            if (rawArgs != null)
                await ExecuteArguments(rawArgs);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                await AccountKeyUtil.CheckAccountKeyValidity(Properties.Settings.Default.sessionIdKey);
        }

        public async Task DisplayVersionInformation()
        {
            try
            {
                // Update version, date published and patch notes code
                HttpResponseMessage response = await SplashWindow.httpClient.GetAsync(Info.SERVER_JSON_API);

                // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                // Get latest release number, date published, and patch notes for server
                string jsonServer = await response.Content.ReadAsStringAsync();
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

        readonly double carouselButtonsAnimationDurationMilliseconds = 100;

        private void CarouselMouseEnter(object sender, MouseEventArgs e)
        {
            if (Carousel.playCarousel == null)
                return;

            Carousel.playCarousel.Pause();

            // Animation for fade in visibility next image button
            DoubleAnimation showImageControls = new(0.2, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)), FillBehavior.Stop)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            showImageControls.Completed += (o, s) => { nextImage.Opacity = 0.2; prevImage.Opacity = 0.2; };
            nextImage.BeginAnimation(Window.OpacityProperty, showImageControls);
            prevImage.BeginAnimation(Window.OpacityProperty, showImageControls);
        }

        private void CarouselMouseLeave(object sender, MouseEventArgs e)
        {
            if (Carousel.playCarousel == null)
                return;

            Carousel.playCarousel.Resume();

            // Animation for fade out visibility next image button
            DoubleAnimation hideImageControls = new(0, new Duration(TimeSpan.FromMilliseconds(carouselButtonsAnimationDurationMilliseconds)), FillBehavior.Stop)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            hideImageControls.Completed += (o, s) => { nextImage.Opacity = 0; prevImage.Opacity = 0; };
            nextImage.BeginAnimation(Window.OpacityProperty, hideImageControls);
            prevImage.BeginAnimation(Window.OpacityProperty, hideImageControls);
        }

        public void SelectDirectory(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog selectDirectory = new();
            if (!(bool)selectDirectory.ShowDialog())
                return;

            Properties.Settings.Default.activeDirectory = selectDirectory.FolderName;
            Properties.Settings.Default.Save();

            new Thread(() =>
            {
                CheckGameVersionAndPath(this, true, true);
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

        public bool CheckGameVersionAndPath(Window callingWindow, bool showSuccess = true, bool showErrors = true)
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

                    if (showErrors)
                    {
                        if (SettingsWindow.settingsInstance != null && SettingsWindow.settingsInstance.Visibility == Visibility.Visible)
                            CustomMessageBox.Show(FindResource("item14").ToString(), callingWindow);
                        else
                            CustomMessageBox.Show($"{FindResource("item14")}\n\n{FindResource("item9")}", callingWindow);
                    }
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

                    if (showErrors)
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
                    
                    if (showErrors)
                        CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), callingWindow, true, false, false, true);
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

                    if (showErrors)
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

                        if (showSuccess)
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

                        if (showErrors)
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

        private void LauncherWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
                launcherNotifyIcon.Visible = true;
            else
                launcherNotifyIcon.Visible = false;
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

        private void PatchNotesCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.CHANGELOG);
        }

        private void DiscordLinkCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.DISCORD_LINK);
        }

        private void ChineseLinkCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.CHANGELOG);
        }

        private void MinimiseToSystemTrayButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            new ToastContentBuilder().AddText(FindResource("item191").ToString()).Show();
        }

        private void MinimiseButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void LauncherClosed(object sender, EventArgs e)
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