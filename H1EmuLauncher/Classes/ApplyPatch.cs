using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

namespace H1EmuLauncher.Classes
{
    class ApplyPatchClass
    {
        public static string latestPatchVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');

        public static void CheckPatch()
        {
            if (Properties.Settings.Default.gameVersionString == "22dec2016" && Properties.Settings.Default.currentPatchVersion2016 != latestPatchVersion ||
                Properties.Settings.Default.gameVersionString == "22dec2016" && Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye") ||
                Properties.Settings.Default.gameVersionString == "22dec2016" && !File.Exists($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\Assets_256.pack") ||
                !File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") ||
                !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") ||
                !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    LauncherWindow.launcherInstance.playButton.IsEnabled = false;

                    if (Properties.Settings.Default.gameVersionString == "22dec2016" && string.IsNullOrEmpty(Properties.Settings.Default.currentPatchVersion2016))
                        LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item150").ToString();
                    else
                        LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item188").ToString();
                }));

                ApplyPatch();
            }

            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.playButton.IsEnabled = true;
                LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item8").ToString();
            }));
        }

        public static void ApplyPatch()
        {
            // Unzip all of the files to the root directory
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

                // Delete BattlEye folder to prevent Steam from trying to launch the game
                if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);

                // Replace users ClientConfig.ini with modified version
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

                // Delete the .zip file, not needed anymore
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1Emu_Voice_Patch.zip");
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    if (Properties.Settings.Default.gameVersionString == "22dec2016")
                        CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item96")}\n\n{e.Message}", LauncherWindow.launcherInstance);
                }));
                return;
            }

            // Finish
            if (Properties.Settings.Default.gameVersionString == "22dec2016")
                Properties.Settings.Default.currentPatchVersion2016 = latestPatchVersion;

            Properties.Settings.Default.Save();
        }
    }
}