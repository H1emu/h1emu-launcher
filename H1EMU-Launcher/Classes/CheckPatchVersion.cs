using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Windows.Threading;
using System.Windows;

namespace H1EMU_Launcher
{
    class CheckPatchVersion
    {
        public static ManualResetEvent mainMa = new ManualResetEvent(false);

        public static string gameVersion;
        public static string latestPatchVersion2015;
        public static string latestPatchVersion2016;

        public static void DownloadLatestVersionNumber()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");
            string jsonPatch2015 = wc.DownloadString("https://api.github.com/repos/H1emu/h1emu-patch/releases/latest");
            var jsonDesPatch2015 = JsonConvert.DeserializeObject<dynamic>(jsonPatch2015);
            string versionNumber = jsonDesPatch2015.tag_name;
            string versionNumberSub = versionNumber.Substring(1);
            latestPatchVersion2015 = versionNumberSub;

            WebClient wc2 = new WebClient();
            wc2.Headers.Add("User-Agent", "d-fens HttpClient");
            string jsonPatch2016 = wc2.DownloadString("https://api.github.com/repos/H1emu/h1emu-patch-2016/releases/latest");
            var jsonDesPatch2016 = JsonConvert.DeserializeObject<dynamic>(jsonPatch2016);
            latestPatchVersion2016 = jsonDesPatch2016.tag_name;

            Settings.installPatchResetEvent.Set();
            mainMa.Set();
        }

        public static void CheckPatch()
        {
            mainMa.Reset();

            try
            {
                DownloadLatestVersionNumber();
                mainMa.WaitOne();

                gameVersion = Settings.gameVersion;

                if (gameVersion == "15jan2015")
                {
                    if (Properties.Settings.Default.currentPatchVersion2015 != latestPatchVersion2015 || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                    {
                        try
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                            {
                                Launcher.lncher.playButton.IsEnabled = false;
                                Launcher.lncher.playButton.Content = System.Windows.Application.Current.FindResource("item150").ToString();
                            });

                            ApplyPatch2015();
                        }
                        catch { }
                    }
                }
                else if (gameVersion == "22dec2016")
                {
                    if (Properties.Settings.Default.currentPatchVersion2016 != latestPatchVersion2016 || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                    {
                        try
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                            {
                                Launcher.lncher.playButton.IsEnabled = false;
                                Launcher.lncher.playButton.Content = System.Windows.Application.Current.FindResource("item150").ToString();
                            });

                            ApplyPatch2016();
                        }
                        catch { }
                    }
                }
                else if (gameVersion != "processBeingUsed")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(System.Windows.Application.Current.FindResource("item14").ToString());
                    });

                    return;
                }
                else
                {
                    return;
                }
            }
            catch { }

            Launcher.ma.Set();

            System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
            {
                Launcher.lncher.playButton.IsEnabled = true;
                Launcher.lncher.playButton.Content = System.Windows.Application.Current.FindResource("item8").ToString();
            });
        }

        public static void ApplyPatch2015()
        {
            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption.

            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
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

            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString("https://api.github.com/repos/QuentinGruber/h1z1-server/releases/latest");
            wc.DownloadFileAsync(new Uri(patch2015), $"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");

            ma.WaitOne();

            // Unzip all of the files to directory.

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file from GitHub, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2015.zip");

            /*

            // Unzip asset patch files

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\AssetPatch2015.zip", $"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets");
            }
            catch { }

            // Delete the downloaded asset .zip file, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\AssetPatch2015.zip");

            */

            // Finish

            Properties.Settings.Default.currentPatchVersion2015 = latestPatchVersion2015;
            Properties.Settings.Default.Save();

            Launcher.ma.Set();
        }

        public static void ApplyPatch2016()
        {
            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption.

            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
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

            wc.DownloadFileCompleted += (s, e) =>
            {
                ma.Set();
            };

            var connectionTest = wc.DownloadString("https://api.github.com/repos/QuentinGruber/h1z1-server/releases/latest");
            wc.DownloadFileAsync(new Uri(patch2016), $"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            ma.WaitOne();

            // Unzip the patch files to directory.

            try
            {
                ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip", $"{Properties.Settings.Default.activeDirectory}");
            }
            catch { }

            // Delete the downloaded .zip file, not needed anymore.

            File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");

            // Delete the BattlEye folder to prevent Steam from launching

            if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
            {
                Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);
            }

            // Finish.

            Properties.Settings.Default.currentPatchVersion2016 = latestPatchVersion2016;
            Properties.Settings.Default.Save();

            Launcher.ma.Set();
        }
    }
}
