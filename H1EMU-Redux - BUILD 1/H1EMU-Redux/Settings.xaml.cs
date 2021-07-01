using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace H1EMU_Redux
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        ProcessStartInfo cmdShell;
        public static string gameVersion { get; set; }

        public Settings()
        {
            InitializeComponent();

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 0.0d;
            fadeAnimation.To = 1.0d;
            MainSettings.BeginAnimation(OpacityProperty, fadeAnimation);

            this.cmdShell = new ProcessStartInfo();
            cmdShell.FileName = "cmd.exe";
            cmdShell.RedirectStandardInput = true;
            cmdShell.UseShellExecute = false;
        }

        //////////////////////
        /// Download Patch ///
        //////////////////////

        public void InstallPatch(object sender, RoutedEventArgs e)
        {
            new Thread(() => 
            {
                if (!CheckDirectory()) { return; }

                switch (gameVersion)
                {
                    case "15jan2015":
                        ApplyPatch2015();
                        break;
                    case "22dec2016":
                        ApplyPatch2016();
                        break;
                    default:
                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show("Game version not detected or not supported by H1Emu.");
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
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
            }

            // Actually downloading the patch .zip.

            string patch2015 = "https://github.com/H1emu/h1emu-patch/releases/latest/download/H1emu_patch.zip?" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = $"Downloading patch files for 2015... {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            try
            {
                wc.DownloadFileAsync(new Uri(patch2015), $"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    watch.Stop();
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show($"Unable to download files: \"{e.Message}\"");
                });

                return;
            }

            ma.WaitOne();

            // Unzip the patch files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = "Unzipping patch files...";
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file, not needed anymore.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = "Deleting .zip file...";
            });

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");

            // Finish.

            watch.Stop();
            double elapsedMs = (double)watch.ElapsedMilliseconds / 1000;

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = $"Done. (Elapsed time: {elapsedMs:0.00}s)";
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                CustomMessageBox.Show("Patch files for game version 2015 have been installed.");
            });
        }

        public void ApplyPatch2016()
        {
            var watch = Stopwatch.StartNew();

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
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

            // Actually downloading the patch .zip.

            string patch2016 = "https://github.com/H1emu/h1emu-patch-2016/releases/latest/download/H1emu_patch.zip?" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = $"Downloading patch files for 2016... {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            try
            {
                wc.DownloadFileAsync(new Uri(patch2016), $"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    watch.Stop();
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show($"Unable to download files: \"{e.Message}\"");
                });

                return;
            }

            ma.WaitOne();

            // Unzip the patch files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = "Unzipping patch files...";
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file, not needed anymore.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = "Deleting .zip file...";
            });

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            // Finish.

            watch.Stop();
            double elapsedMs = (double)watch.ElapsedMilliseconds / 1000;

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = $"Done. (Elapsed time: {elapsedMs:0.00}s)";
                patchButton.IsEnabled = true;
                latestButton.IsEnabled = true;
                stableButton.IsEnabled = true;
                CustomMessageBox.Show("Patch files for game version 2016 have been installed.");
            });
        }

        /////////////////////////////
        /// Install Server Latest ///
        /////////////////////////////

        public void DownloadServerLatest(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                var watch = Stopwatch.StartNew();

                if (!CheckDirectory()) { return; }

                if (!DownloadMaster()) { return; }
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = "Installing latest server...";
                });

                Process p = new Process();

                p.StartInfo = cmdShell;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v16.2.0-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine("npm i h1z1-server@latest");
                    }
                }

                p.WaitForExit();

                watch.Stop();
                double elapsedMs = (double)watch.ElapsedMilliseconds / 1000;

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = $"Done. (Elapsed time: {elapsedMs:0.00}s)";
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show("Latest server has been installed.");
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

                if (!CheckDirectory()) { return; }

                if (!DownloadMaster()) { return; }
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = "Installing stable server...";
                });

                Process p = new Process();

                p.StartInfo = cmdShell;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v16.2.0-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine("npm i");
                    }
                }

                p.WaitForExit();

                watch.Stop();
                double elapsedMs = (double)watch.ElapsedMilliseconds / 1000;

                Dispatcher.BeginInvoke((MethodInvoker) delegate
                {
                    settingsProgressText.Text = $"Done. (Elapsed time: {elapsedMs:0.00}s)";
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show("Stable server has been installed.");
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
                settingsProgressText.Text = "Deleting old files...";
            });

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master"))
            {
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
                patchButton.IsEnabled = false;
                latestButton.IsEnabled = false;
                stableButton.IsEnabled = false;
            });

            // Delete old server files if they exist.

            DeleteOldFiles();

            // Download the latest server files.

            string serverFiles = "https://github.com/H1emu/h1z1-server-QuickStart/archive/master.zip";

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = $"Downloading server files... {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            try
            {
                wc.DownloadFileAsync(new Uri(serverFiles), $"{Properties.Settings.Default.activeDirectory}\\H1Z1-Server-Quickstart-Master.zip");
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show($"Unable to download files: \"{e.Message}\"");
                });

                return false;
            }

            ma.WaitOne();

            // Unzip the server files to directory.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = $"Unzipping server files...";
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
            });

            // Delete old .zip file in the case of corruption.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node-v16.2.0-win-x64.zip");

            // Download the NodeJS files.

            string serverFiles = "https://nodejs.org/dist/v16.2.0/node-v16.2.0-win-x64.zip?" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            ManualResetEvent ma = new ManualResetEvent(false);

            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            wc.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgress.Value = e.ProgressPercentage;
                    settingsProgressText.Text = $"Downloading NodeJS standalone... {e.ProgressPercentage}%";
                });
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            try
            {
                wc.DownloadFileAsync(new Uri(serverFiles), $"{Properties.Settings.Default.activeDirectory}\\Node-v16.2.0-win-x64.zip");
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                    CustomMessageBox.Show($"Unable to download files: \"{e.Message}\"");
                });

                return;
            }

            ma.WaitOne();

            // Unzip the node files.

            Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                settingsProgressText.Text = $"Unzipping node files...";
            });

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Node-v16.2.0-win-x64.zip", $"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
            }
            catch { }

            // Delete the old .zip file, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Node-v16.2.0-win-x64.zip");
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
                if (!string.IsNullOrEmpty(Properties.Settings.Default.activeGame))
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        Properties.Settings.Default.activeGame = "- Game version not detected -";
                        Properties.Settings.Default.Save();
                        currentGame.Text = Properties.Settings.Default.activeGame;
                        gameVersion = "";
                    });
                }

                return;
            }
            else
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = "Detecting game version...";
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
                        currentGame.Text = "Please close H1Z1.";
                        CustomMessageBox.Show("H1Z1.exe is currently being used.\n\nClose any open instances of H1Z1.");
                    });
                    gameVersion = "processBeingUsed";
                    return;
                }

                switch (hash)
                {
                    case "53a3d98f": // 15th January 2015
                        gameVersion = "15jan2015";
                        Properties.Settings.Default.activeGame = "2015";
                        Properties.Settings.Default.Save();

                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = $"Current Game Version: {Properties.Settings.Default.activeGame}";
                        });

                        break;
                    case "bc5b3ab6": // 22nd December 2016
                        gameVersion = "22dec2016";
                        Properties.Settings.Default.activeGame = "2016";
                        Properties.Settings.Default.Save();

                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = $"Current Game Version: {Properties.Settings.Default.activeGame}";
                        });

                        break;
                    default:
                        Properties.Settings.Default.activeGame = "- Game version not detected -";
                        Properties.Settings.Default.Save();

                        Dispatcher.BeginInvoke((MethodInvoker)delegate
                        {
                            currentGame.Text = Properties.Settings.Default.activeGame;
                        });

                        break;
                }

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    patchButton.IsEnabled = true;
                    latestButton.IsEnabled = true;
                    stableButton.IsEnabled = true;
                });
            }
        }

        public void SettingsLoaded(object sender, RoutedEventArgs e)
        {
            new Thread(() => 
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    settingsProgressText.Text = "";
                });

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
                        currentGame.Text = "- Game Version Not Detected -";
                    });

                    return;
                }
                else
                {
                    Properties.Settings.Default.activeDirectory = "Directory";
                    Properties.Settings.Default.Save();

                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        directoryBox.Text = "Directory";
                        currentGame.Text = "- Game Version Not Detected -";
                    });

                    return;
                }

                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = "Detecting game version...";
                });

                CheckGameVersion();

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

                CheckGameVersion();

                if (gameVersion == "15jan2015")
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("Game version detected: 15 January 2015");
                    });
                }
                else if (gameVersion == "22dec2016")
                {
                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("Game version detected: 22nd December 2016");
                    });
                }
                else if (gameVersion != "processBeingUsed")
                {
                    Properties.Settings.Default.activeGame = "- Game version not detected -";
                    Properties.Settings.Default.Save();

                    Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        currentGame.Text = Properties.Settings.Default.activeGame;
                        CustomMessageBox.Show("Game version not detected or not supported by H1Emu.");
                    });
                }
                else
                {
                    return;
                }

            }).Start();
        }

        public bool CheckDirectory()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\h1z1.exe"))
            {
                Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    currentGame.Text = "- Game Version Not Detected -";
                    CustomMessageBox.Show("Game either not found or not supported by H1Emu.");
                });

                return false;
            }

            return true;
        }

        public void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDirectory()) { return; }

            Process.Start(new ProcessStartInfo
            {
                FileName = Properties.Settings.Default.activeDirectory,
                UseShellExecute = true
            });
        }

        public void CloseButton(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainSettings.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainSettings.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            this.Close();
        }

        public void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}