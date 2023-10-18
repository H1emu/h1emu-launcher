using H1EmuLauncher.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EmuLauncher.SettingsPages
{
    /// <summary>
    /// Interaction logic for GameFiles.xaml
    /// </summary>
    public partial class GameFiles : Page
    {
        public static GameFiles gameFilesInstance;
        readonly ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        public static string gameVersionString { get; set; }
        public static bool launchAccountKeyWindow;
        public static bool isExecutingTasks;

        public GameFiles()
        {
            InitializeComponent();
            gameFilesInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void GameFilesPageLoaded(object sender, RoutedEventArgs e)
        {
            if (isExecutingTasks)
            {
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
                deleteSinglePlayerDataHyperLink.IsEnabled = false;
                return;
            }

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
                else if (gameVersionString != "15jan2015" && gameVersionString != "22dec2016" && gameVersionString != "kotk")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = FindResource("item72").ToString();
                    }));
                }

            }).Start();
        }

        //////////////////////
        /// Install Patch ///
        //////////////////////

        public void InstallPatch(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                if (!CheckDirectory())
                    return;

                switch (gameVersionString)
                {
                    case "15jan2015":
                    case "22dec2016":
                    case "kotk":
                        try
                        {
                            ApplyPatch();
                        }
                        catch (Exception er)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                ToggleButtons(true);

                                if (gameVersionString == "15jan2015")
                                    CustomMessageBox.Show($"{FindResource("item95")} \"{er.Message}\".", SettingsWindow.settingsInstance);
                                else if (gameVersionString == "22dec2016")
                                    CustomMessageBox.Show($"{FindResource("item96")} \"{er.Message}\".", SettingsWindow.settingsInstance);
                                else if (gameVersionString == "kotk")
                                    CustomMessageBox.Show($"{FindResource("item97")} \"{er.Message}\".", SettingsWindow.settingsInstance);
                            }));
                        }
                        break;
                    case "processBeingUsed":
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ToggleButtons(true);

                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, false, true);
                        }));
                        break;
                    default:
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ToggleButtons(true);

                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance);
                        }));
                        break;
                }

            }).Start();
        }

        public void ApplyPatch()
        {
            var watch = Stopwatch.StartNew();

            ToggleButtons(false);

            // Deletes old patch files if any of them are already in the directory, including the .zip in case of corruption
            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgressText.Text = FindResource("item76").ToString();
                }));

                File.Delete($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2015.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
            }

            // Unzip all of the files to directory
            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                settingsProgressBar.IsIndeterminate = true;
                settingsProgressText.Text = FindResource("item99").ToString();
            }));

            try
            {
                if (gameVersionString == "15jan2015")
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2015.zip", Properties.Resources.Game_Patch_2015);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2015.zip", $"{Properties.Settings.Default.activeDirectory}");
                }
                else if (gameVersionString == "22dec2016")
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", Properties.Resources.Game_Patch_2016);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", $"{Properties.Settings.Default.activeDirectory}");
                }
                else if (gameVersionString == "kotk")
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", Properties.Resources.Game_Patch_KotK);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", $"{Properties.Settings.Default.activeDirectory}");
                }
            }
            catch { }

            // Delete the .zip file, not needed anymore
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item100").ToString();
            }));

            if (gameVersionString == "15jan2015")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2015.zip");
            else if (gameVersionString == "22dec2016")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
            else if (gameVersionString == "kotk")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip");

            // Extra patch work for some versions
            if (gameVersionString == "22dec2016" || gameVersionString == "kotk")
            {
                // Delete BattlEye folder to prevent Steam from trying to launch the game
                if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                {
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);
                }

                if (gameVersionString == "22dec2016")
                {
                    // Extract Asset_256.pack to fix blackberries
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\Assets_256.pack", Properties.Resources.Assets_256);
                }
            }

            // Replace users ClientConfig.ini with modified version
            File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

            // Finish
            watch.Stop();
            TimeSpan elapsedMs = watch.Elapsed;

            if (gameVersionString == "15jan2015")
                Properties.Settings.Default.currentPatchVersion2015 = ApplyPatchClass.latestPatchVersion;
            else if (gameVersionString == "22dec2016")
                Properties.Settings.Default.currentPatchVersion2016 = ApplyPatchClass.latestPatchVersion;
            else if (gameVersionString == "kotk")
                Properties.Settings.Default.currentPatchVersionKotK = ApplyPatchClass.latestPatchVersion;

            Properties.Settings.Default.Save();

            ToggleButtons(true);

            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                settingsProgressBar.IsIndeterminate = false;

                if (gameVersionString == "15jan2015")
                    CustomMessageBox.Show($"{FindResource("item102")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                else if (gameVersionString == "22dec2016")
                    CustomMessageBox.Show($"{FindResource("item103")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                else if (gameVersionString == "kotk")
                    CustomMessageBox.Show($"{FindResource("item104")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
            }));
        }

        //////////////////////
        /// Install Server ///
        //////////////////////

        public void InstallServer(object sender, RoutedEventArgs e)
        {
            if (!CheckDirectory())
                return;

            if (gameVersionString == "processBeingUsed")
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    ToggleButtons(true);

                    CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, false, true);
                }));

                return;
            }
            else if (string.IsNullOrEmpty(gameVersionString))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    ToggleButtons(true);

                    CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance);
                }));

                return;
            }

            Button button = (Button)sender;

            if (button.Name == "latestButton")
            {
                MessageBoxResult dr = CustomMessageBox.Show(FindResource("item157").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, true, true, false, false);
                if (dr != MessageBoxResult.Yes)
                    return;
            }

            ToggleButtons(false);

            new Thread(() =>
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    if (!ExtractMaster())
                        return;

                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (button.Name == "latestButton")
                        {
                            settingsProgressText.Text = FindResource("item105").ToString();
                            LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            settingsProgressBar.IsIndeterminate = true;
                        }
                        else
                        {
                            settingsProgressText.Text = FindResource("item109").ToString();
                            LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            settingsProgressBar.IsIndeterminate = true;
                        }

                    }));

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
                            sw.WriteLine("set INSTALL_TYPE=launcher");

                            Dispatcher.Invoke(new Action(delegate
                            {
                                if (button.Name == "latestButton")
                                    sw.WriteLine("npm i --production h1z1-server@next");
                                else
                                    sw.WriteLine("npm i --production h1z1-server@latest");
                            }));
                        }
                    }

                    p.WaitForExit(5000);

                    if (p.HasExited)
                        throw new Exception(FindResource("item185").ToString());
                    else
                        p.WaitForExit();
                }
                catch (Exception er)
                {
                    ToggleButtons(true);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        settingsProgressBar.IsIndeterminate = false;

                        if (button.Name == "latestButton")
                            CustomMessageBox.Show($"{FindResource("item107")} \"{er.Message}\".", SettingsWindow.settingsInstance);
                        else
                            CustomMessageBox.Show($"{FindResource("item111")} \"{er.Message}\".", SettingsWindow.settingsInstance);
                    }));

                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;

                ToggleButtons(true);

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgressBar.IsIndeterminate = false;
                    settingsProgressBar.Value = 0;

                    if (button.Name == "latestButton")
                        CustomMessageBox.Show($"{FindResource("item108")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                    else
                        CustomMessageBox.Show($"{FindResource("item112")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                }));

            }).Start();
        }

        /////////////////////////////
        /// Extract Server Files ///
        /////////////////////////////

        public bool ExtractMaster()
        {
            // Delete old server files if they exist
            DeleteOldFiles();

            // Unzip the server files to directory
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item116").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                settingsProgressBar.IsIndeterminate = true;
            }));

            try
            {
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", Properties.Resources.Server_Quickstart);
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles");
            }
            catch { }

            // Delete old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");

            // Unzip node files
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item118").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                settingsProgressBar.IsIndeterminate = true;
            }));

            try
            {
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Node.zip", Properties.Resources.Node_18_12_1);
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master");
            }
            catch { }

            // Delete the old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node.zip");

            return true;
        }

        public int foldersToDelete = 0;

        public void DeleteOldFiles()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                settingsProgressText.Text = FindResource("item113").ToString();
            }));

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgressText.Text = FindResource("item114").ToString();
                }));

                RecursiveDeleteFolders(new DirectoryInfo($"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles"));
                foldersToDelete = 0;

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgressBar.Maximum = 100;
                }));
            }
        }

        public void RecursiveDeleteFolders(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (DirectoryInfo dir in baseDir.EnumerateDirectories())
            {
                foldersToDelete++;

                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgressBar.Value = foldersToDelete;
                }));

                RecursiveDeleteFolders(dir);
            }

            baseDir.Delete(true);
        }

        //////////////////////////
        /// Check Game Version ///
        //////////////////////////

        public void CheckGameVersion()
        {
            gameVersionString = string.Empty;

            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
                isExecutingTasks = true;
            }));

            if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    currentGame.Text = FindResource("item69").ToString();
                }));

                return;
            }

            Dispatcher.Invoke(new Action(delegate
            {
                currentGame.Text = FindResource("item70").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
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
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    isExecutingTasks = false;
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                }));

                gameVersionString = "processBeingUsed";
                return;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    isExecutingTasks = false;
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                }));

                return;
            }

            switch (hash)
            {
                case "53a3d98f": // Just Survive: 15th January 2015
                    gameVersionString = "15jan2015";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} 2015";
                    }));

                    break;
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
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                directoryButton.IsEnabled = true;
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                isExecutingTasks = false;
            }));
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
                    case "15jan2015":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item71").ToString(), SettingsWindow.settingsInstance);
                        }));

                        break;
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

        public void ToggleButtons(bool enabled)
        {
            if (enabled)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    deleteSinglePlayerDataHyperLink.IsEnabled = true;
                    isExecutingTasks = false;

                    settingsProgressBar.Value = 0;
                    settingsProgressRowContent.Measure(new Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
                    DoubleAnimation hide = new(settingsProgressRowContent.DesiredSize.Height, 0, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        AccelerationRatio = 0.4,
                        DecelerationRatio = 0.4
                    };
                    hide.Completed += (s, o) => settingsProgressRow.Visibility = Visibility.Collapsed;
                    settingsProgressRow.BeginAnimation(HeightProperty, hide);
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                    directoryButton.IsEnabled = false;
                    patchButton.IsEnabled = false;
                    latestButton.IsEnabled = false;
                    stableButton.IsEnabled = false;
                    deleteSinglePlayerDataHyperLink.IsEnabled = false;
                    isExecutingTasks = true;

                    settingsProgressRow.Visibility = Visibility.Visible;
                    settingsProgressRowContent.Measure(new Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
                    DoubleAnimation show = new(0, settingsProgressRowContent.DesiredSize.Height, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        AccelerationRatio = 0.4,
                        DecelerationRatio = 0.4
                    };
                    settingsProgressRow.BeginAnimation(HeightProperty, show);
                }));
            }
        }

        private void DeleteSingleplayerDataClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists($"{Info.APPLICATION_DATA_PATH}\\h1emu"))
            {
                CustomMessageBox.Show(FindResource("item177").ToString(), SettingsWindow.settingsInstance);
                return;
            }

            MessageBoxResult dr = CustomMessageBox.Show(FindResource("item178").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, true, true, false, false);
            if (dr != MessageBoxResult.Yes)
                return;

            deleteSinglePlayerDataHyperLink.IsEnabled = false;

            try
            {
                Directory.Delete($"{Info.APPLICATION_DATA_PATH}\\h1emu", true);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", SettingsWindow.settingsInstance);
                return;
            }
            finally
            {
                deleteSinglePlayerDataHyperLink.IsEnabled = true;
            }

            CustomMessageBox.Show(FindResource("item179").ToString(), SettingsWindow.settingsInstance);
        }
    }
}
