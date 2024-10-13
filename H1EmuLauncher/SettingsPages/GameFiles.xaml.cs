using H1EmuLauncher.Classes;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Drawing;
using System.Drawing.Imaging;
using Windows.ApplicationModel.Activation;

namespace H1EmuLauncher.SettingsPages
{
    public partial class GameFiles : Page
    {
        public static GameFiles gameFilesInstance;
        private ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        public static bool isExecutingTasks;

        public GameFiles()
        {
            InitializeComponent();
            gameFilesInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        //////////////////////
        /// Install Patch ///
        //////////////////////

        public void InstallPatchButton(object sender, RoutedEventArgs e)
        {
            if (!LauncherWindow.launcherInstance.CheckGameVersionAndPath(SettingsWindow.settingsInstance))
                return;

            var watch = Stopwatch.StartNew();
            ToggleButtons(false);

            new Thread(() =>
            {
                // Unzip all of the files to the root directory
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                    settingsProgressBar.IsIndeterminate = true;
                    settingsProgressText.Text = FindResource("item99").ToString();
                }));

                try
                {
                    if (Properties.Settings.Default.gameVersionString == "22dec2016")
                    {
                        // Extract main game patch
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", Properties.Resources.Game_Patch_2016);
                        ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", $"{Properties.Settings.Default.activeDirectory}", true);

                        // Extract voice chat patch
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Emu_Voice_Patch.zip", Properties.Resources.H1Emu_Voice_Patch);
                        ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Emu_Voice_Patch.zip", $"{Properties.Settings.Default.activeDirectory}", true);

                        // Extract Asset_256.pack for various fixes
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\Assets_256.pack", Properties.Resources.Assets_256);

                        // Extract modified BattlEye to provide custom anti-cheat and asset validation
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_BE.exe", Properties.Resources.H1Z1_BE);

                        // Extract custom H1Z1_FP (Fair Play) anticheat binary
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_FP.exe", Properties.Resources.H1Z1_FP);

                        // Extract FairPlay logo
                        Bitmap fairPlayLogo = new Bitmap(Properties.Resources.logo);
                        fairPlayLogo.Save($"{Properties.Settings.Default.activeDirectory}\\logo.bmp", ImageFormat.Bmp);
                    }
                    else if (Properties.Settings.Default.gameVersionString == "kotk")
                    {
                        // Extract main game patch
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", Properties.Resources.Game_Patch_KotK);
                        ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", $"{Properties.Settings.Default.activeDirectory}", true);
                    }

                    // Delete BattlEye folder to prevent Steam from trying to launch the game
                    if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                        Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);

                    // Replace users ClientConfig.ini with modified version
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

                    // Delete the .zip file, not needed anymore
                    Dispatcher.Invoke(new Action(delegate
                    {
                        settingsProgressText.Text = FindResource("item100").ToString();
                    }));

                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Emu_Voice_Patch.zip");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip");
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        if (Properties.Settings.Default.gameVersionString == "22dec2016")
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item96")}\n\n{e.Message}", LauncherWindow.launcherInstance);
                        else if (Properties.Settings.Default.gameVersionString == "kotk")
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item97")}\n\n{e.Message}", LauncherWindow.launcherInstance);
                    }));
                }

                if (Properties.Settings.Default.gameVersionString == "22dec2016")
                    Properties.Settings.Default.currentPatchVersion2016 = ApplyPatchClass.latestPatchVersion;
                else if (Properties.Settings.Default.gameVersionString == "kotk")
                    Properties.Settings.Default.currentPatchVersionKotK = ApplyPatchClass.latestPatchVersion;

                Properties.Settings.Default.Save();

                // Finish
                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;
                ToggleButtons(true);

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgressBar.IsIndeterminate = false;

                    if (Properties.Settings.Default.gameVersionString == "22dec2016")
                        CustomMessageBox.Show($"{FindResource("item103")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                    else if (Properties.Settings.Default.gameVersionString == "kotk")
                        CustomMessageBox.Show($"{FindResource("item104")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", SettingsWindow.settingsInstance);
                }));

            }).Start();
        }

        //////////////////////
        /// Install Server ///
        //////////////////////

        public void InstallServerButton(object sender, RoutedEventArgs e)
        {
            if (!LauncherWindow.launcherInstance.CheckGameVersionAndPath(SettingsWindow.settingsInstance))
                return;

            Button button = (Button)sender;
            if (button.Name == "latestButton")
            {
                MessageBoxResult dr = CustomMessageBox.Show(FindResource("item157").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, true, true, false, false);
                if (dr != MessageBoxResult.Yes)
                    return;
            }

            var watch = Stopwatch.StartNew();
            ToggleButtons(false);

            new Thread(() =>
            {
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

                    Process installServerProcess = new Process { StartInfo = cmdShell };
                    installServerProcess.Start();
                    using (StreamWriter sw = installServerProcess.StandardInput)
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
                    installServerProcess.WaitForExit(5000);

                    if (installServerProcess.HasExited)
                        throw new Exception(FindResource("item185").ToString());
                    else
                        installServerProcess.WaitForExit();
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
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles", true);
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
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Node.zip", Properties.Resources.Node_Files);
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master", true);
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

        public void ToggleButtons(bool enabled)
        {
            if (enabled)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    LauncherWindow.launcherInstance.directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    deleteSinglePlayerDataHyperLink.IsEnabled = true;
                    isExecutingTasks = false;

                    settingsProgressBar.Value = 0;
                    settingsProgressRowContent.Measure(new System.Windows.Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
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
                    patchButton.IsEnabled = false;
                    latestButton.IsEnabled = false;
                    stableButton.IsEnabled = false;
                    deleteSinglePlayerDataHyperLink.IsEnabled = false;
                    isExecutingTasks = true;

                    settingsProgressRow.Visibility = Visibility.Visible;
                    settingsProgressRowContent.Measure(new System.Windows.Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
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
