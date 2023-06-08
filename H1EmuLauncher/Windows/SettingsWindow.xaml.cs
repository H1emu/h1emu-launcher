using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Windows.Controls;
using H1EmuLauncher.Classes;
using System.Windows.Media.Animation;

namespace H1EmuLauncher
{
    public partial class SettingsWindow : Window
    {
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
        public static SettingsWindow settingsInstance;
        public Storyboard UnfocusPropertiesAnimationShow;
        public Storyboard UnfocusPropertiesAnimationHide;

        public SettingsWindow()
        {
            InitializeComponent();
            settingsInstance = this;
            Owner = LauncherWindow.launcherInstance;

            UnfocusPropertiesAnimationShow = FindResource("UnfocusPropertiesShow") as Storyboard;
            UnfocusPropertiesAnimationHide = FindResource("UnfocusPropertiesHide") as Storyboard;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
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
                        try
                        {
                            ApplyPatch();
                        }
                        catch (Exception er)
                        {
                            Dispatcher.Invoke(new Action(delegate
                            {
                                EnableButtons();

                                if (gameVersionString == "15jan2015")
                                    CustomMessageBox.Show($"{FindResource("item95")} \"{er.Message}\".", this);
                                else
                                    CustomMessageBox.Show($"{FindResource("item97")} \"{er.Message}\".", this);
                            }));
                        }
                        break;
                    case "processBeingUsed":
                        Dispatcher.Invoke(new Action(delegate
                        {
                            EnableButtons();

                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, true);
                        }));
                        break;
                    default:
                        Dispatcher.Invoke(new Action(delegate
                        {
                            EnableButtons();

                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                        }));
                        break;
                }

            }).Start();
        }

        public void ApplyPatch()
        {
            var watch = Stopwatch.StartNew();

            DisableButtons();

            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption
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
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\AssetPatch2015.zip");
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
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip", Properties.Resources.Game_Patch_2015);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip", $"{Properties.Settings.Default.activeDirectory}");
                }
                else
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip", Properties.Resources.Game_Patch_2016);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip", $"{Properties.Settings.Default.activeDirectory}");
                }
            }
            catch { }

            // Delete the .zip file, not needed anymore
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item100").ToString();
            }));

            if (gameVersionString == "15jan2015")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
            else
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            // Extract Asset_256.pack to fix blackberries & delete BattlEye folder to prevent Steam from trying to launch the game (2016 only)
            if (gameVersionString == "22dec2016")
            {
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\Assets_256.pack", Properties.Resources.Assets_256);
                
                if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                {
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);
                }
            }

            // Replace users ClientConfig.ini with modified version
            File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

            // Finish
            watch.Stop();
            TimeSpan elapsedMs = watch.Elapsed;

            if (gameVersionString == "15jan2015")
                Properties.Settings.Default.currentPatchVersion2015 = ApplyPatchClass.latestPatchVersion;
            else
                Properties.Settings.Default.currentPatchVersion2016 = ApplyPatchClass.latestPatchVersion;

            Properties.Settings.Default.Save();

            EnableButtons();

            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                settingsProgressBar.IsIndeterminate = false;

                if (gameVersionString == "15jan2015")
                    CustomMessageBox.Show($"{FindResource("item102")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", this);
                else
                    CustomMessageBox.Show($"{FindResource("item104")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", this);
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
                    EnableButtons();

                    CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, true);
                }));

                return;
            }
            else if (string.IsNullOrEmpty(gameVersionString))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    EnableButtons();

                    CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                }));

                return;
            }

            Button button = (Button)sender;

            if (button.Name == "latestButton")
            {
                MessageBoxResult dr = CustomMessageBox.ShowResult(FindResource("item157").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                if (dr != MessageBoxResult.Yes)
                    return;
            }

            DisableButtons();

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
                    EnableButtons();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        settingsProgressBar.IsIndeterminate = false;

                        if (button.Name == "latestButton")
                            CustomMessageBox.Show($"{FindResource("item107")} \"{er.Message}\".", this);
                        else
                            CustomMessageBox.Show($"{FindResource("item111")} \"{er.Message}\".", this);
                    }));

                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;

                EnableButtons();

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgressBar.IsIndeterminate = false;
                    settingsProgressBar.Value = 0;

                    if (button.Name == "latestButton")
                        CustomMessageBox.Show($"{FindResource("item108")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", this);
                    else
                        CustomMessageBox.Show($"{FindResource("item112")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101")} {$"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' ')})", this);
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
                    CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\".", this);
                }));

                return;
            }

            switch (hash)
            {
                case "53a3d98f": // 15th January 2015
                    gameVersionString = "15jan2015";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} 2015";
                    }));

                    break;
                case "bc5b3ab6": // 22nd December 2016
                    gameVersionString = "22dec2016";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} 2016";
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

        public void SettingsLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();

            currentVersion.Text = $"{FindResource("item124")}{Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.')}";

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
                        CustomMessageBox.Show(LauncherWindow.launcherInstance.FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, true);
                    }));
                }
                else if (gameVersionString != "15jan2015" && gameVersionString != "22dec2016")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = FindResource("item72").ToString();
                    }));
                }

            }).Start();
        }

        public void SelectDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog directory = new();

            Dispatcher.Invoke(new Action(delegate
            {
                if (directory.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
            }));

            Properties.Settings.Default.activeDirectory = directory.SelectedPath;
            Properties.Settings.Default.Save();

            directoryBox.Text = Properties.Settings.Default.activeDirectory;

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
                            CustomMessageBox.Show(FindResource("item71").ToString(), this);
                        }));

                        break;
                    case "22dec2016":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item74").ToString(), this);
                        }));

                        break;
                    case "processBeingUsed":

                        Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, true);
                        }));

                        break;
                    default:

                        Dispatcher.Invoke(new Action(delegate
                        {
                            currentGame.Text = FindResource("item72").ToString();
                            CustomMessageBox.Show(FindResource("item58").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
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
                        CustomMessageBox.Show(FindResource("item14").ToString(), this);
                    else
                        CustomMessageBox.Show($"{FindResource("item14")}\n\n{FindResource("item9")}", LauncherWindow.launcherInstance);
                }));

                return false;
            }

            return true;
        }

        public void EnableButtons()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                directoryButton.IsEnabled = true;
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                deleteSinglePlayerDataHyperLink.IsEnabled= true;
                isExecutingTasks = false;

                settingsProgressBar.Value = 0;
                settingsProgressRowContent.Measure(new Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
                DoubleAnimation hide = new(settingsProgressRowContent.DesiredSize.Height, 0, new Duration(TimeSpan.FromMilliseconds(150)))
                {
                    AccelerationRatio = 0.4,
                    DecelerationRatio = 0.4
                };
                hide.Completed += (s, _) => settingsProgressRow.Visibility = Visibility.Collapsed;
                settingsProgressRow.BeginAnimation(HeightProperty, hide);
            }));
        }

        public void DisableButtons()
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

        private void AccountKeyClick(object sender, RoutedEventArgs e)
        {
            AccountKeyWindow ak = new();
            ak.ShowDialog();
        }

        private void DeleteSingleplayerDataClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists($"{Info.APPLICATION_DATA_PATH}\\h1emu"))
            {
                CustomMessageBox.Show(FindResource("item177").ToString(), this);
                return;
            }

            MessageBoxResult dr = CustomMessageBox.ShowResult(FindResource("item178").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
            if (dr != MessageBoxResult.Yes)
                return;

            try
            {
                Directory.Delete($"{Info.APPLICATION_DATA_PATH}\\h1emu", true);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {ex.Message}", this);
                return;
            }

            CustomMessageBox.Show(FindResource("item179").ToString(), this);
        }

        public bool IsCompleted = false;

        private void SettingsClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible && isExecutingTasks)
            {
                CustomMessageBox.Show(FindResource("item73").ToString(), this);
                e.Cancel = true;
                return;
            }

            if (!IsCompleted)
            {
                LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();

                e.Cancel = true;

                if (FindResource("CloseSettings") is Storyboard sb)
                {
                    sb.Completed += (s, _) =>
                    {
                        IsCompleted = true;
                        Close();
                    };

                    sb.Begin();
                }
            }
        }

        private void SettingsContentRendered(object sender, EventArgs e)
        {
            // If accountkey argument was specified launch the accountkey window with the argument value
            if (launchAccountKeyWindow)
            {
                AccountKeyWindow ak = new();
                ak.accountKeyBox.Password = SteamFrame.Login.GetParameter(LauncherWindow.rawArgs, "-accountkey", "");
                LauncherWindow.rawArgs = null;
                ak.accountKeyHint.Visibility = Visibility.Hidden;
                ak.ShowDialog();

                launchAccountKeyWindow = false;
            }
        }

        public void CloseButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}