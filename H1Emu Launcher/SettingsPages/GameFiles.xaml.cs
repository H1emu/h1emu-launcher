using H1Emu_Launcher.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace H1Emu_Launcher.SettingsPages
{
    public partial class GameFiles : Page
    {
        public static GameFiles gameFilesInstance;
        public static readonly ProcessStartInfo cmdShell = new()
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

        public void InstallPatchButton(object sender, RoutedEventArgs e)
        {
            if (!LauncherWindow.launcherInstance.CheckGameVersionAndPath(SettingsWindow.settingsInstance, false, true))
                return;

            var watch = Stopwatch.StartNew();
            ToggleButtons(false);

            new Thread(async () =>
            {
                // Unzip all of the files to the root directory
                Dispatcher.Invoke(new Action(delegate
                {
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

                        // Extract modified Sound Bank files
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Resources\\Audio\\pc9\\SoundBanks\\Sound_Banks.zip", Properties.Resources.Sound_Banks);
                        ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Resources\\Audio\\pc9\\SoundBanks\\Sound_Banks.zip", $"{Properties.Settings.Default.activeDirectory}\\Resources\\Audio\\pc9\\SoundBanks", true);

                        // Extract voice chat patch
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoiceClient.exe", Properties.Resources.H1EmuVoiceClient);

                        // Extract modified BattlEye to provide custom anti-cheat and asset validation
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_BE.exe", Properties.Resources.H1Z1_BE);

                        // Extract custom H1Z1_FP (Fair Play) anticheat binary
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_FP.exe", Properties.Resources.H1Z1_FP);

                        // Extract FairPlay logo
                        Bitmap fairPlayLogo = new(Properties.Resources.logo);
                        fairPlayLogo.Save($"{Properties.Settings.Default.activeDirectory}\\logo.bmp", ImageFormat.Bmp);

                        // Extract lz4.dll file patch for smaller data sizes using compression
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\lz4.dll", Properties.Resources.lz4);

                        // Extract patched Locale files
                        File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Locale\\Locales.zip", Properties.Resources.Locales);
                        ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Locale\\Locales.zip", $"{Properties.Settings.Default.activeDirectory}\\Locale", true);

                        // Clean up Assets directory
                        foreach (string file in Directory.GetFiles($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets"))
                        {
                            string fileName = Path.GetFileName(file);
                            if (!fileName.EndsWith(".pack", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(file);
                                continue;
                            }
                            if (fileName.StartsWith("Assets_", StringComparison.OrdinalIgnoreCase))
                            {
                                // Try to parse the number between "Assets_" and ".pack"
                                if (!int.TryParse(fileName[7..^5], out int packNum) || packNum > 255)
                                    File.Delete(file);
                            }
                        }

                        Dispatcher.Invoke(new Action(delegate
                        {
                            settingsProgressText.Text = FindResource("item54").ToString();
                        }));

                        // Query the asset pack source JSON
                        string assetPackJsonURL = string.Empty;

                        // Get the download URL for the selected asset pack
                        if (Properties.Settings.Default.selectedAssetPack == 0)
                            assetPackJsonURL = Info.OFFICIAL_ASSET_PACK;
                        else
                        {
                            List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
                            assetPackJsonURL = assetPackJson[Properties.Settings.Default.selectedAssetPack - 2].AssetPackURL;
                        }

                        // Deserialise the JSON into an object
                        HttpResponseMessage response = await SplashWindow.httpClient.GetAsync(assetPackJsonURL, HttpCompletionOption.ResponseHeadersRead);

                        // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                        if (response.StatusCode != HttpStatusCode.OK)
                            throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                        // Get latest release number and date published for app.
                        string jsonAssetPack = await response.Content.ReadAsStringAsync();
                        JsonEndPoints.AssetPack.Root jsonAssetPackDes = JsonSerializer.Deserialize<JsonEndPoints.AssetPack.Root>(jsonAssetPack);

                        Dispatcher.Invoke(new Action(delegate
                        {
                            settingsProgressBar.IsIndeterminate = false;
                        }));

                        // For each asset in the JSON, download the asset file
                        foreach (JsonEndPoints.AssetPack.Asset item in jsonAssetPackDes.assets)
                        {
                            // Deserialise the JSON into an object
                            HttpResponseMessage responseDownloadURL = await SplashWindow.httpClient.GetAsync(item.url, HttpCompletionOption.ResponseHeadersRead);
                            
                            // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                            if (responseDownloadURL.StatusCode != HttpStatusCode.OK)
                                throw new Exception($"{char.ToUpper(responseDownloadURL.ReasonPhrase.First())}{responseDownloadURL.ReasonPhrase.Substring(1)}");

                            long totalBytes = responseDownloadURL.Content.Headers.ContentLength ?? -1L;
                            using Stream contentStream = await responseDownloadURL.Content.ReadAsStreamAsync();
                            using (FileStream fileStream = new($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\{item.filename}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                byte[] buffer = new byte[8192];
                                long totalBytesRead = 0;
                                int bytesRead;

                                while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                                {
                                    // Write the data to the file
                                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                                    totalBytesRead += bytesRead;

                                    // Update the progress bar
                                    if (totalBytes > 0)
                                    {
                                        float progressPercentage = (float)totalBytesRead * 100 / totalBytes;

                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            settingsProgressBar.Value = progressPercentage;
                                            settingsProgressText.Text = $"{FindResource("item54").ToString().Replace("...", "")} \"{item.filename}\"... {progressPercentage:0.00}%";
                                        }));
                                    }
                                }
                            };
                        }

                        Dispatcher.Invoke(new Action(delegate
                        {
                            settingsProgressBar.IsIndeterminate = true;
                        }));
                    }

                    // Delete BattlEye folder to prevent Steam from trying to launch the game
                    if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                        Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);

                    // Replace users ClientConfig.ini with modified version
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

                    // Delete any no longer needed files/old patches
                    Dispatcher.Invoke(new Action(delegate
                    {
                        settingsProgressText.Text = FindResource("item100").ToString();
                    }));

                    if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoice"))
                        Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoice", true);
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Resources\\Audio\\pc9\\SoundBanks\\Sound_Banks.zip");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Locale\\Locales.zip");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoiceClient.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoiceClient.runtimeconfig.json");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.Asio.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.Core.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.Midi.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.Wasapi.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\NAudio.WinMM.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\websocket-sharp.dll");
                    File.Delete($"{Properties.Settings.Default.activeDirectory}\\Assets_Pack.zip");
                }
                catch (Exception e)
                {
                    watch.Stop();
                    ToggleButtons(true);
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        if (Properties.Settings.Default.gameVersionString == "22dec2016")
                            CustomMessageBox.Show($"{FindResource("item96")}\n\n{e.Message}", SettingsWindow.settingsInstance);
                    }));
                    return;
                }

                // Finish
                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;
                ToggleButtons(true);

                Dispatcher.Invoke(new Action(delegate
                {
                    if (Properties.Settings.Default.gameVersionString == "22dec2016")
                        CustomMessageBox.Show($"{FindResource("item103")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString().Replace("{0}", $"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' '))}", SettingsWindow.settingsInstance);
                }));
            }).Start();
        }

        public void InstallServerButton(object sender, RoutedEventArgs e)
        {
            if (!LauncherWindow.launcherInstance.CheckGameVersionAndPath(SettingsWindow.settingsInstance, false, true))
                return;

            Button button = (Button)sender;
            if (button.Name == "latestButton")
            {
                MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item157").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, true, true);
                if (mbr != MessageBoxResult.Yes)
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
                        settingsProgressBar.IsIndeterminate = true;

                        if (button.Name == "latestButton")
                            settingsProgressText.Text = FindResource("item105").ToString();
                        else
                            settingsProgressText.Text = FindResource("item109").ToString();
                    }));

                    Process installServerProcess = new() { StartInfo = cmdShell };
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
                    watch.Stop();
                    ToggleButtons(true);
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (button.Name == "latestButton")
                            CustomMessageBox.Show($"{FindResource("item107")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                        else
                            CustomMessageBox.Show($"{FindResource("item111")} \"{e.Message}\".", SettingsWindow.settingsInstance);
                    }));
                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;
                ToggleButtons(true);

                Dispatcher.Invoke(new Action(delegate
                {
                    if (button.Name == "latestButton")
                        CustomMessageBox.Show($"{FindResource("item108")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString().Replace("{0}", $"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' '))}", SettingsWindow.settingsInstance);
                    else
                        CustomMessageBox.Show($"{FindResource("item112")}{Environment.NewLine}{Environment.NewLine}{FindResource("item101").ToString().Replace("{0}", $"{elapsedMs.Minutes}m {elapsedMs.Seconds}.{elapsedMs.Milliseconds.ToString().Remove(1)}s".TrimStart('0', 'm').TrimStart(' '))}", SettingsWindow.settingsInstance);
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
                settingsProgressBar.IsIndeterminate = true;
            }));

            File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", Properties.Resources.Server_Quickstart);
            ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles", true);

            // Delete old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");

            // Unzip node files
            Dispatcher.Invoke(new Action(delegate
            {
                settingsProgressText.Text = FindResource("item118").ToString();
            }));

            File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Node.zip", Properties.Resources.Node_Files);
            ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServerFiles\\h1z1-server-QuickStart-master", true);

            // Delete the old .zip file, not needed anymore
            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node.zip");
            return true;
        }

        public int foldersToDelete = 0;

        public void DeleteOldFiles()
        {
            Dispatcher.Invoke(new Action(delegate
            {
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
                    settingsProgressBar.IsIndeterminate = false;
                    settingsProgressRowContent.Measure(new System.Windows.Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
                    DoubleAnimation hide = new(settingsProgressRowContent.DesiredSize.Height, 0, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn }
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
                    LauncherWindow.launcherInstance.directoryButton.IsEnabled = false;
                    patchButton.IsEnabled = false;
                    latestButton.IsEnabled = false;
                    stableButton.IsEnabled = false;
                    deleteSinglePlayerDataHyperLink.IsEnabled = false;
                    isExecutingTasks = true;

                    settingsProgressBar.Value = 0;
                    settingsProgressRow.Visibility = Visibility.Visible;
                    settingsProgressRowContent.Measure(new System.Windows.Size(settingsProgressRow.MaxWidth, settingsProgressRow.MaxHeight));
                    DoubleAnimation show = new(0, settingsProgressRowContent.DesiredSize.Height, new Duration(TimeSpan.FromMilliseconds(150)))
                    {
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
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

            MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item178").ToString().Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), SettingsWindow.settingsInstance, false, true, true);
            if (mbr != MessageBoxResult.Yes)
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
