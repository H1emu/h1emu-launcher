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
            switch (Properties.Settings.Default.gameVersionString)
            {
                case "22dec2016":
                case "kotk":
                    if (Properties.Settings.Default.gameVersionString == "kotk" && Properties.Settings.Default.currentPatchVersionKotK != latestPatchVersion ||
                        Properties.Settings.Default.gameVersionString == "22dec2016" && Properties.Settings.Default.currentPatchVersion2016 != latestPatchVersion ||
                        Properties.Settings.Default.gameVersionString == "22dec2016" && Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye") ||
                        Properties.Settings.Default.gameVersionString == "22dec2016" && !Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoice") ||
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

                            if (Properties.Settings.Default.gameVersionString == "22dec2016" && string.IsNullOrEmpty(Properties.Settings.Default.currentPatchVersion2016) ||
                                Properties.Settings.Default.gameVersionString == "kotk" && string.IsNullOrEmpty(Properties.Settings.Default.currentPatchVersionKotK))
                                LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item150").ToString();
                            else
                                LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item188").ToString();
                        }));

                        ApplyPatch();
                    }
                    break;
                case "processBeingUsed":
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(LauncherWindow.launcherInstance.FindResource("item121").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), LauncherWindow.launcherInstance, false, false, true);
                    }));
                    break;
                default:
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        CustomMessageBox.Show(LauncherWindow.launcherInstance.FindResource("item58").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), LauncherWindow.launcherInstance);
                    }));
                    break;
            }

            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.playButton.IsEnabled = true;
                LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item8").ToString();
            }));
        }

        public static void ApplyPatch()
        {
            // Deletes old patch files if any of them are already in the directory, including the .zip file in case of corruption
            if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
            {
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2015.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
            }

            // Unzip all of the files to directory
            try
            {
                if (Properties.Settings.Default.gameVersionString == "22dec2016")
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", Properties.Resources.Game_Patch_2016);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip", $"{Properties.Settings.Default.activeDirectory}", true);
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoicePatch.zip", Properties.Resources.H1Emu_Voice_Patch);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoicePatch.zip", $"{Properties.Settings.Default.activeDirectory}", true);
                }
                else if (Properties.Settings.Default.gameVersionString == "kotk")
                {
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", Properties.Resources.Game_Patch_KotK);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip", $"{Properties.Settings.Default.activeDirectory}", true);
                }
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

            // Delete the .zip file, not needed anymore
            if (Properties.Settings.Default.gameVersionString == "22dec2016")
            {
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_2016.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\H1EmuVoicePatch.zip");
            }
            else if (Properties.Settings.Default.gameVersionString == "kotk")
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Game_Patch_KotK.zip");

            // Extra patch work for some versions
            if (Properties.Settings.Default.gameVersionString == "22dec2016" || Properties.Settings.Default.gameVersionString == "kotk")
            {
                // Delete BattlEye folder to prevent Steam from trying to launch the game
                if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);

                if (Properties.Settings.Default.gameVersionString == "22dec2016")
                {
                    // Extract Asset_256.pack to fix blackberries
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\Assets_256.pack", Properties.Resources.Assets_256);

                    // Extract modified BattlEye to provide custom anti-cheat and asset validation
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_BE.exe", Properties.Resources.H1Z1_BE);

                    // Extract custom H1Z1_FP (Fair Play) anticheat binary
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\H1Z1_FP.exe", Properties.Resources.H1Z1_FP);

                    // Extract FairPlay logo
                    Bitmap fairPlayLogo = new Bitmap(Properties.Resources.logo);
                    fairPlayLogo.Save($"{Properties.Settings.Default.activeDirectory}\\logo.bmp", ImageFormat.Bmp);
                }
            }

            // Replace users ClientConfig.ini with modified version
            File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

            // Finish
            if (Properties.Settings.Default.gameVersionString == "22dec2016")
                Properties.Settings.Default.currentPatchVersion2016 = latestPatchVersion;
            else if (Properties.Settings.Default.gameVersionString == "kotk")
                Properties.Settings.Default.currentPatchVersionKotK = latestPatchVersion;

            Properties.Settings.Default.Save();
        }
    }
}