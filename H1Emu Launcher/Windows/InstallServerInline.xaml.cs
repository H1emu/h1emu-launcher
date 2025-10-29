using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher
{
    public partial class InstallServerInline : Window
    {
        public static InstallServerInline serverInstallOptionsInstance;
        public static bool isExecutingTasks;

        public InstallServerInline()
        {
            InitializeComponent();
            serverInstallOptionsInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void InstallServerInlineButton(object sender, RoutedEventArgs e)
        {
            if (!LauncherWindow.launcherInstance.CheckGameVersionAndPath(SettingsWindow.settingsInstance, false, true))
                return;

            Button button = (Button)sender;
            if (button.Name == "latestButton")
            {
                MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item157").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), this, false, true, true);
                if (mbr != MessageBoxResult.Yes)
                    return;
            }

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
                            installServerProgressBar.IsIndeterminate = true;
                        }
                        else
                        {
                            settingsProgressText.Text = FindResource("item109").ToString();
                            LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            installServerProgressBar.IsIndeterminate = true;
                        }

                    }));

                    Process installServerProcess = new()
                    {
                        StartInfo = SettingsPages.GameFiles.cmdShell
                    };
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
                catch (Exception e)
                {
                    ToggleButtons(true);

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        installServerProgressBar.IsIndeterminate = false;

                        if (button.Name == "latestButton")
                            CustomMessageBox.Show($"{FindResource("item107")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                        else
                            CustomMessageBox.Show($"{FindResource("item111")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                    }));
                    return;
                }

                ToggleButtons(true);
                CustomMessageBox.buttonPressed = MessageBoxResult.Yes;
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    installServerProgressBar.IsIndeterminate = false;
                    installServerProgressBar.Value = 0;
                    Topmost = true;
                    Close();
                }));
            }).Start();
        }

        public bool ExtractMaster()
        {
            // Delete old server files if they exist
            DeleteOldFiles();

            // Unzip the server files to directory
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item116").ToString();
                LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                installServerProgressBar.IsIndeterminate = true;
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
                installServerProgressBar.IsIndeterminate = true;
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
                    installServerProgressBar.Maximum = 100;
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
                    installServerProgressBar.Value = foldersToDelete;
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
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    isExecutingTasks = false;

                    installServerProgressBar.Value = 0;
                    installServerProgressRowContent.Measure(new Size(installServerProgressRow.MaxWidth, installServerProgressRow.MaxHeight));
                    DoubleAnimation hide = new(installServerProgressRowContent.DesiredSize.Height, 0, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn }
                    };
                    hide.Completed += (s, o) => installServerProgressRow.Visibility = Visibility.Collapsed;
                    installServerProgressRow.BeginAnimation(HeightProperty, hide);
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.taskbarIcon.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                    latestButton.IsEnabled = false;
                    stableButton.IsEnabled = false;
                    isExecutingTasks = true;

                    installServerProgressRow.Visibility = Visibility.Visible;
                    installServerProgressRowContent.Measure(new Size(installServerProgressRow.MaxWidth, installServerProgressRow.MaxHeight));
                    DoubleAnimation show = new(0, installServerProgressRowContent.DesiredSize.Height, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                    };
                    installServerProgressRow.BeginAnimation(HeightProperty, show);
                }));
            }
        }

        private void MoveServerInstallOptionsWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ServerInstallOptionsContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseServerInstallOptionsWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void ServerInstallOptionsLoaded(object sender, RoutedEventArgs e)
        {
            FocusEffects.BeginUnfocusAnimation(Owner);
        }

        private void ServerInstallOptionsClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible && isExecutingTasks)
            {
                CustomMessageBox.Show(FindResource("item73").ToString(), this);
                e.Cancel = true;
                return;
            }

            serverInstallOptionsInstance = null;
            FocusEffects.BeginFocusAnimation(Owner);
        }
    }
}
