using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace H1EMU_Launcher
{
    public partial class Settings : Window
    {
        ProcessStartInfo cmdShell;
        public static ManualResetEvent installPatchResetEvent = new ManualResetEvent(false);

        public static string gameVersion { get; set; }
        public static Settings sttngs;

        public Settings()
        {
            InitializeComponent();
            sttngs = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(H1EMU_Launcher.Resources.SetLanguageFile.LoadFile());

            this.cmdShell = new ProcessStartInfo();
            cmdShell.FileName = "cmd.exe";
            cmdShell.RedirectStandardInput = true;
            cmdShell.UseShellExecute = false;
            cmdShell.CreateNoWindow = true;
        }

        //////////////////////
        /// Download Patch ///
        //////////////////////

        public void InstallPatch(object sender, RoutedEventArgs e)
        {
            new Thread(() => 
            {
                installPatchResetEvent.Reset();

                if (!CheckDirectory()) { return; }
                CheckGameVersion();

                switch (gameVersion)
                {
                    case "15jan2015":
                        try
                        {
                            CheckPatchVersion.DownloadLatestVersionNumber();
                            installPatchResetEvent.WaitOne();
                            ApplyPatch2015();
                        }
                        catch (Exception er)
                        {
                            Dispatcher.BeginInvoke((MethodInvoker)delegate
                            {
                                settingsProgressText.Text = FindResource("item93").ToString();
                                settingsProgress.Value = 0;
                                directoryButton.IsEnabled = true;
                                patchButton.IsEnabled = true;
                                latestButton.IsEnabled = true;
                                stableButton.IsEnabled = true;

                                if (er.Message == "No such host is known. (api.github.com:443)")
                                {
                                    CustomMessageBox.Show(FindResource("item95").ToString() + $" \"{er.Message}\"." + FindResource("item137").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                                }
                                else
                                {
                                    CustomMessageBox.Show(FindResource("item95").ToString() + $" \"{er.Message}\".");
                                }
                            });
                        }
                        break;
                    case "22dec2016":
                        try
                        {
                            CheckPatchVersion.DownloadLatestVersionNumber();
                            installPatchResetEvent.WaitOne();
                            ApplyPatch2016();
                        }
                        catch (Exception er)
                        {
                            Dispatcher.BeginInvoke((MethodInvoker)delegate
                            {
                                settingsProgressText.Text = FindResource("item93").ToString();
                                settingsProgress.Value = 0;
                                directoryButton.IsEnabled = true;
                                patchButton.IsEnabled = true;
                                latestButton.IsEnabled = true;
                                stableButton.IsEnabled = true;

                                if (er.Message == "No such host is known. (api.github.com:443)")
                                {
                                    CustomMessageBox.Show(FindResource("item97").ToString() + $" \"{er.Message}\"." + FindResource("item137").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                                }
                                else
                                {
                                    CustomMessageBox.Show(FindResource("item97").ToString() + $" \"{er.Message}\".");
                                }
                            });
                        }
                        break;
                    case "processBeingUsed":
                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                        });
                        break;
                    default:
                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item58").ToString());
                        });
                        break;
                }

            }).Start();
        }

        public void ApplyPatch2015()
        {
            var watch = Stopwatch.StartNew();

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
            });

            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption.

            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = FindResource("item76").ToString();
                });

                File.Delete($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\AssetPatch2015.zip");
            }

            // Download the patch .zip.

            string patch2015 = Classes.Info.GAME_PATCH_2015 + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = FindResource("item98").ToString() + $" {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString(Classes.Info.SERVER_JSON_API);
            wc.DownloadFileAsync(new Uri(patch2015), $"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");

            ma.WaitOne();

            // Unzip all of the files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item99").ToString();
                settingsProgress.IsIndeterminate = true;
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file from GitHub, not needed anymore.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item100").ToString();
            });

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");

            // Finish.

            watch.Stop();
            TimeSpan elapsedMs = watch.Elapsed;

            Properties.Settings.Default.currentPatchVersion2015 = CheckPatchVersion.latestPatchVersion2015;
            Properties.Settings.Default.Save();

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item101").ToString() + $" {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})";
                directoryButton.IsEnabled = true;
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                settingsProgress.IsIndeterminate = false;
                CustomMessageBox.Show(FindResource("item102").ToString());
            });
        }

        public void ApplyPatch2016()
        {
            var watch = Stopwatch.StartNew();

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
            });

            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption.

            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = "Deleting old patch...";
                });

                File.Delete($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");
            }

            // Download the patch .zip.

            string patch2016 = Classes.Info.GAME_PATCH_2016 + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = FindResource("item103").ToString() + $" {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString(Classes.Info.SERVER_JSON_API);
            wc.DownloadFileAsync(new Uri(patch2016), $"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            ma.WaitOne();

            // Unzip the patch files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item99").ToString();
                settingsProgress.IsIndeterminate = true;
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file, not needed anymore.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item100").ToString();
            });

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            // Delete the BattlEye folder to prevent Steam from launching

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item152").ToString();
            });

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
            {
                Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);
            }

            // Finish.

            watch.Stop();
            TimeSpan elapsedMs = watch.Elapsed;

            Properties.Settings.Default.currentPatchVersion2016 = CheckPatchVersion.latestPatchVersion2016;
            Properties.Settings.Default.Save();

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item101").ToString() + $" {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})";
                directoryButton.IsEnabled = true;
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                settingsProgress.IsIndeterminate = false;
                CustomMessageBox.Show(FindResource("item104").ToString());
            });
        }

        /////////////////////////////
        /// Install Server Latest ///
        /////////////////////////////

        public void DownloadServerLatest(object sender, RoutedEventArgs e)
        {
            DialogResult dr = CustomMessageBox.ShowResult(FindResource("item157").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
            if (dr != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            new Thread(() =>
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    if (!CheckDirectory()) { return; }
                    if (!DownloadMaster()) { return; }

                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgressText.Text = FindResource("item105").ToString();
                        settingsProgress.IsIndeterminate = true;
                    });

                    Process p = new Process();
                    p.StartInfo = cmdShell;
                    p.Start();

                    using (StreamWriter sw = p.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v{Classes.Info.NODEJS_VERSION}-win-x64");
                            sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                            sw.WriteLine("set INSTALL_TYPE=launcher");
                            sw.WriteLine("npm i --production h1z1-server@next");
                        }
                    }

                    p.WaitForExit();
                }
                catch (Exception er)
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgressText.Text = FindResource("item93").ToString();
                        settingsProgress.Value = 0;
                        directoryButton.IsEnabled = true;
                        patchButton.IsEnabled = true;
                        latestButton.IsEnabled = true;
                        stableButton.IsEnabled = true;
                        settingsProgress.IsIndeterminate = false;

                        if (er.Message == "No such host is known. (api.github.com:443)")
                        {
                            CustomMessageBox.Show(FindResource("item107").ToString() + $" \"{er.Message}\"." + FindResource("item137").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                        }
                        else
                        {
                            CustomMessageBox.Show(FindResource("item107").ToString() + $" \"{er.Message}\".");
                        }
                    });

                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = FindResource("item101").ToString() + $" {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})";
                    directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    settingsProgress.IsIndeterminate = false;
                    CustomMessageBox.Show(FindResource("item108").ToString());
                });

            }).Start();
        }

        /////////////////////////////
        /// Install Server Stable ///
        /////////////////////////////

        public void DownloadServerStable(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    if (!CheckDirectory()) { return; }
                    if (!DownloadMaster()) { return; }

                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgressText.Text = FindResource("item109").ToString();
                        settingsProgress.IsIndeterminate = true;
                    });

                    Process p = new Process();
                    p.StartInfo = cmdShell;
                    p.Start();

                    using (StreamWriter sw = p.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v{Classes.Info.NODEJS_VERSION}-win-x64");
                            sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                            sw.WriteLine("set INSTALL_TYPE=launcher");
                            sw.WriteLine("npm i --production h1z1-server@latest");
                        }
                    }

                    p.WaitForExit();
                }
                catch (Exception er)
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgressText.Text = FindResource("item93").ToString();
                        settingsProgress.Value = 0;
                        directoryButton.IsEnabled = true;
                        patchButton.IsEnabled = true;
                        latestButton.IsEnabled = true;
                        stableButton.IsEnabled = true;
                        settingsProgress.IsIndeterminate = false;

                        if (er.Message == "No such host is known. (api.github.com:443)")
                        {
                            CustomMessageBox.Show(FindResource("item111").ToString() + $" \"{er.Message}\"." + FindResource("item137").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                        }
                        else
                        {
                            CustomMessageBox.Show(FindResource("item111").ToString() + $" \"{er.Message}\".");
                        }
                    });

                    return;
                }

                watch.Stop();
                TimeSpan elapsedMs = watch.Elapsed;

                Dispatcher.BeginInvoke((MethodInvoker) delegate
                {
                    settingsProgressText.Text = FindResource("item101").ToString() + $" {elapsedMs.ToString($"hh\\hmm\\m\\ ss\\.ff\\s").TrimStart(' ', 'h', 'm', 's', '0')})";
                    directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    settingsProgress.IsIndeterminate = false;
                    CustomMessageBox.Show(FindResource("item112").ToString());
                });

            }).Start();
        }

        /////////////////////////////
        /// Download Server Files ///
        /////////////////////////////

        public void DeleteOldFiles()
        {
            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item113").ToString();
            });

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master"))
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = FindResource("item114").ToString();
                });

                DirectoryInfo dirInfo = new DirectoryInfo($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                var files = dirInfo.GetFiles();

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Minimum = 0;
                    settingsProgress.Value = 0;
                    settingsProgress.Maximum = files.Length;
                });

                foreach (FileInfo file in files)
                {
                    file.Delete();
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgress.Value += 1;
                    });
                }

                var dirs = dirInfo.GetDirectories();

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = 0;
                    settingsProgress.Maximum = dirs.Length;
                });

                foreach (DirectoryInfo dir in dirs)
                {
                    dir.Delete(true);
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        settingsProgress.Value += 1;
                    });
                }

                Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles", true);
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Maximum = 100;
                });
            }
        }

        public bool DownloadMaster()
        {
            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                directoryButton.IsEnabled = false;
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
            });

            // Delete old server files if they exist.

            DeleteOldFiles();

            // Download the latest server files.

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = FindResource("item115").ToString() + $" {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString(Classes.Info.SERVER_JSON_API);
            wc.DownloadFileAsync(new Uri(Classes.Info.SERVER_FILES), $"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");

            ma.WaitOne();

            // Unzip the server files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item116").ToString();
                settingsProgress.IsIndeterminate = true;
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles");
            }
            catch { }

            // Delete old .zip file, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");

            // Start download of NodeJS Standalone.

            InstallNodeJSStandalone();

            return true;
        }

        public void InstallNodeJSStandalone()
        {
            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgress.Value = 0;
                settingsProgress.IsIndeterminate = false;
            });

            // Delete old .zip file in the case of corruption.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node-v{Classes.Info.NODEJS_VERSION}-win-x64.zip");

            // Download the NodeJS files.

            string serverFiles = "https://nodejs.org/dist/v" + Classes.Info.NODEJS_VERSION + "/node-v" + Classes.Info.NODEJS_VERSION + "-win-x64.zip?" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = FindResource("item117").ToString() + $" {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString(Classes.Info.SERVER_JSON_API);
            wc.DownloadFileAsync(new Uri(serverFiles), $"{Properties.Settings.Default.activeDirectory}\\Node-v{Classes.Info.NODEJS_VERSION}-win-x64.zip");

            ma.WaitOne();

            // Unzip the node files.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = FindResource("item118").ToString();
                settingsProgress.IsIndeterminate = true;
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node-v{Classes.Info.NODEJS_VERSION}-win-x64.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
            }
            catch { }

            // Delete the old .zip file, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node-v{Classes.Info.NODEJS_VERSION}-win-x64.zip");
        }

        //////////////////////////
        /// Check Game Version ///
        //////////////////////////

        public void CheckGameVersionNewThread()
        {
            new Thread(() => 
            {
                CheckGameVersion();
                Launcher.ma.Set();

            }).Start();
        }

        public void CheckGameVersion()
        {
            gameVersion = "";

            if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = FindResource("item69").ToString();
                });

                return;
            }
            else
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = FindResource("item70").ToString();
                    directoryButton.IsEnabled = false;
                    patchButton.IsEnabled = false;
                    latestButton.IsEnabled = false;
                    stableButton.IsEnabled = false;
                });

                Hash.Crc32 crc32 = new Hash.Crc32();
                String hash = String.Empty;

                try
                {
                    using (FileStream fs = File.Open($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe", FileMode.Open))
                        foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();
                }
                catch (System.IO.IOException)
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        directoryButton.IsEnabled = true;
                        patchButton.IsEnabled = true;
                        latestButton.IsEnabled = true;
                        stableButton.IsEnabled = true;
                        currentGame.Text = FindResource("item120").ToString();
                    });

                    gameVersion = "processBeingUsed";
                    return;
                }

                switch (hash)
                {
                    case "53a3d98f": // 15th January 2015
                        gameVersion = "15jan2015";

                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = $"{FindResource("item122").ToString()} 2015";
                        });

                        break;
                    case "bc5b3ab6": // 22nd December 2016
                        gameVersion = "22dec2016";

                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = $"{FindResource("item122").ToString()} 2016";
                        });

                        break;
                    default:
                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = FindResource("item69").ToString();
                        });

                        break;
                }

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    directoryButton.IsEnabled = true;
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                });
            }
        }

        public void SettingsLoaded(object sender, RoutedEventArgs e)
        {
            Launcher.lncher.launcherBlur.Radius = 15;
            Launcher.lncher.launcherFade.Visibility = Visibility.Visible;

            currentVersion.Text = FindResource("item124").ToString() + $"{Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.')}";
            settingsProgressText.Text = FindResource("item19").ToString();

            new Thread(() => 
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) && Directory.Exists(Properties.Settings.Default.activeDirectory) && File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                    });
                }
                else if (!string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) && Directory.Exists(Properties.Settings.Default.activeDirectory) && !File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        directoryBox.Text = Properties.Settings.Default.activeDirectory;
                        currentGame.Text = FindResource("item69").ToString();
                    });

                    return;
                }
                else
                {
                    Properties.Settings.Default.activeDirectory = "Directory";
                    Properties.Settings.Default.Save();

                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        directoryBox.Text = FindResource("item75").ToString();
                        currentGame.Text = FindResource("item69").ToString();
                    });

                    return;
                }

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = FindResource("item70").ToString();
                });

                CheckGameVersion();

                if (gameVersion == "processBeingUsed")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(System.Windows.Application.Current.FindResource("item121").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                    });
                }
                else if (gameVersion != "15jan2015" && gameVersion != "22dec2016")
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        currentGame.Text = FindResource("item72").ToString();
                    });
                }

            }).Start();
        }

        public void SelectDirectory(object sender, RoutedEventArgs e)
        {
            new Thread(() => 
            {
                ManualResetEvent ma = new ManualResetEvent(false);

                FolderBrowserDialog directory = new FolderBrowserDialog();

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    if (directory.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                    { 
                        ma.Set();
                    }
                    else
                    {
                        return;
                    }
                });

                ma.WaitOne();

                Properties.Settings.Default.activeDirectory = directory.SelectedPath;
                Properties.Settings.Default.Save();

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    directoryBox.Text = Properties.Settings.Default.activeDirectory;
                });

                if (!CheckDirectory()) { return; }
                CheckGameVersion();

                if (gameVersion == "15jan2015")
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item71").ToString());
                    });
                }
                else if (gameVersion == "22dec2016")
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item74").ToString());
                    });
                }
                else if (gameVersion == "processBeingUsed")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine));
                    });

                    return;
                }
                else
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        currentGame.Text = FindResource("item72").ToString();
                        CustomMessageBox.Show(FindResource("item58").ToString());
                    });

                    return;
                }

            }).Start();
        }

        public void OpenDirectory(object sender, RoutedEventArgs e)
        {
            if (!CheckDirectory()) { return; }

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
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = FindResource("item69").ToString();
                    CustomMessageBox.Show(FindResource("item14").ToString());
                });

                return false;
            }

            return true;
        }

        private void AccountKeyClick(object sender, RoutedEventArgs e)
        {
            AccountKey ak = new AccountKey();
            ak.ShowDialog();
        }

        private void MainSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!directoryButton.IsEnabled || !patchButton.IsEnabled || !latestButton.IsEnabled || !stableButton.IsEnabled)
            {
                CustomMessageBox.Show(FindResource("item73").ToString());
                e.Cancel = true;
                return;
            }

            this.Topmost = true;

            Launcher.lncher.launcherBlur.Radius = 0;
            Launcher.lncher.launcherFade.Visibility = Visibility.Hidden;
        }

        public void CloseButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainSettingsActivated(object sender, EventArgs e)
        {
            settingsBlur.Radius = 0;
            settingsFade.Visibility = Visibility.Hidden;
        }
    }
}