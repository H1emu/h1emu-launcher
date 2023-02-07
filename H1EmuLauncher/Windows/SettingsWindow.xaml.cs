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

namespace H1EmuLauncher
{
    public partial class SettingsWindow : Window
    {
        ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        public static SettingsWindow settingsInstance;
        public static string gameVersion { get; set; }
        public static bool launchAccountKeyWindow;

        public SettingsWindow()
        {
            InitializeComponent();
            settingsInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it
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

                switch (gameVersion)
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

                                if (gameVersion == "15jan2015")
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

                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
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
                settingsProgress.IsIndeterminate = true;
                settingsProgressText.Text = FindResource("item99").ToString();
            }));

            try
            {
                if (gameVersion == "15jan2015")
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

            if (gameVersion == "15jan2015")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
            else
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            // Extract Asset_256.pack to fix blackberries & delete BattlEye folder to prevent Steam from trying to launch the game (2016 only)
            if (gameVersion == "22dec2016")
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

            if (gameVersion == "15jan2015")
                Properties.Settings.Default.currentPatchVersion2015 = ApplyPatchClass.latestPatchVersion;
            else
                Properties.Settings.Default.currentPatchVersion2016 = ApplyPatchClass.latestPatchVersion;

            Properties.Settings.Default.Save();

            EnableButtons();

            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                settingsProgress.IsIndeterminate = false;

                if (gameVersion == "15jan2015")
                    CustomMessageBox.Show($"{FindResource("item102")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString()} {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})", this);
                else
                    CustomMessageBox.Show($"{FindResource("item104")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString()} {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})", this);
            }));
        }

        //////////////////////
        /// Install Server ///
        //////////////////////

        public void InstallServer(object sender, RoutedEventArgs e)
        {
            if (!CheckDirectory())
                return;

            if (gameVersion == "processBeingUsed")
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    EnableButtons();

                    CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                }));

                return;
            }
            else if (string.IsNullOrEmpty(gameVersion))
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
                            settingsProgress.IsIndeterminate = true;
                        }
                        else
                        {
                            settingsProgressText.Text = FindResource("item109").ToString();
                            LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            settingsProgress.IsIndeterminate = true;
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
                            sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v{Info.NODEJS_VERSION}-win-x64");
                            sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
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
                    {
                        if (button.Name == "latestButton")
                            Dispatcher.Invoke(new Action(delegate
                            {
                                CustomMessageBox.Show(FindResource("item107").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}").Replace(":", ".").Replace("：", "."), this);
                            }));
                        else
                            Dispatcher.Invoke(new Action(delegate
                            {
                                CustomMessageBox.Show(FindResource("item111").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}").Replace(":", ".").Replace("：", "."), this);
                            }));

                        return;
                    }
                    else
                        p.WaitForExit();
                }
                catch (Exception er)
                {
                    EnableButtons();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        settingsProgress.IsIndeterminate = false;

                        if (button.Name == "latestButton")
                            CustomMessageBox.Show($"{FindResource("item107")} \"{er.Message}\".", this);
                        else
                            CustomMessageBox.Show($"{FindResource("item111").ToString()} \"{er.Message}\".", this);
                    }));

                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;

                EnableButtons();

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgress.IsIndeterminate = false;
                    settingsProgress.Value = 0;

                    if (button.Name == "latestButton")
                        CustomMessageBox.Show($"{FindResource("item108").ToString()}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString()} {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})", this);
                    else
                        CustomMessageBox.Show($"{FindResource("item112").ToString()}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString()} {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})", this);
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
                settingsProgress.IsIndeterminate = true;
            }));

            try
            {
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", Properties.Resources.Server_Quickstart);
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles");
            }
            catch { }

            // Delete old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");

            // Unzip node files
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item118").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                settingsProgress.IsIndeterminate = true;
            }));

            try
            {
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Node.zip", Properties.Resources.Node_18_12_1);
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
            }
            catch { }

            // Delete the old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node.zip");

            return true;
        }

        public void DeleteOldFiles()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item113").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            }));

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master"))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgressText.Text = FindResource("item114").ToString();
                }));

                DirectoryInfo dirInfo = new($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                var files = dirInfo.GetFiles();

                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgress.Minimum = 0;
                    settingsProgress.Value = 0;
                    settingsProgress.Maximum = files.Length;
                }));

                foreach (FileInfo file in files)
                {
                    file.Delete();
                    Dispatcher.Invoke(new Action(delegate
                    {
                        settingsProgress.Value += 1;
                    }));
                }

                var dirs = dirInfo.GetDirectories();

                Dispatcher.Invoke(new Action(delegate
                {
                    settingsProgress.Value = 0;
                    settingsProgress.Maximum = dirs.Length;
                }));

                foreach (DirectoryInfo dir in dirs)
                {
                    dir.Delete(true);
                    Dispatcher.Invoke(new Action(delegate
                    {
                        settingsProgress.Value += 1;
                    }));
                }

                try
                {
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles", true);
                }
                catch
                {
                    CustomMessageBox.Show(FindResource("item168").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                    return;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    settingsProgress.Maximum = 100;
                }));
            }
        }

        //////////////////////////
        /// Check Game Version ///
        //////////////////////////

        public void CheckGameVersion()
        {
            gameVersion = "";

            Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
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
            String hash = String.Empty;

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
                }));

                EnableButtons();

                gameVersion = "processBeingUsed";
                return;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    CustomMessageBox.Show($"{FindResource("item142")} \"{e.Message}\".");
                }));

                EnableButtons();

                return;
            }

            switch (hash)
            {
                case "53a3d98f": // 15th January 2015
                    gameVersion = "15jan2015";

                    Dispatcher.Invoke(new Action(delegate
                    {
                        currentGame.Text = $"{FindResource("item122")} 2015";
                    }));

                    break;
                case "bc5b3ab6": // 22nd December 2016
                    gameVersion = "22dec2016";

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
            }));

            EnableButtons();
        }

        public void SettingsLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 15;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Visible;

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

                if (gameVersion == "processBeingUsed")
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(Application.Current.FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
                    }));
                }
                else if (gameVersion != "15jan2015" && gameVersion != "22dec2016")
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

                switch (gameVersion)
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
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this);
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

                    if (this != null && this.Visibility == Visibility.Visible)
                        CustomMessageBox.Show(FindResource("item14").ToString(), this);
                    else
                        CustomMessageBox.Show($"{FindResource("item14")}\n\n{FindResource("item9")}", this);
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
                settingsProgress.Value = 0;
                settingsProgressRow.Visibility = Visibility.Collapsed;
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
                settingsProgressRow.Visibility = Visibility.Visible;
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

        private void MainSettingsClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible && !directoryButton.IsEnabled)
            {
                CustomMessageBox.Show(FindResource("item73").ToString(), this);
                e.Cancel = true;
                return;
            }

            Topmost = true;

            LauncherWindow.launcherInstance.launcherBlur.Radius = 0;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void MainSettingsActivated(object sender, EventArgs e)
        {
            settingsBlur.Radius = 0;
            settingsFade.Visibility = Visibility.Hidden;
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void MainSettingsContentRendered(object sender, EventArgs e)
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