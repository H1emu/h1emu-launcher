using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Reflection;

namespace H1EmuLauncher.Classes
{
    class ApplyPatchClass
    {
        public static string gameVersionString;
        public static string latestPatchVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');

        public static void CheckPatch()
        {
            gameVersionString = SettingsWindow.gameVersionString;

            switch (gameVersionString)
            {
                case "15jan2015":
                case "22dec2016":
                    if (gameVersionString == "15jan2015" && Properties.Settings.Default.currentPatchVersion2015 != latestPatchVersion || gameVersionString == "22dec2016" && Properties.Settings.Default.currentPatchVersion2016 != latestPatchVersion ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            LauncherWindow.launcherInstance.playButton.IsEnabled = false;

                            if (gameVersionString == "15jan2015" && string.IsNullOrEmpty(Properties.Settings.Default.currentPatchVersion2015) || gameVersionString == "22dec2016" && string.IsNullOrEmpty(Properties.Settings.Default.currentPatchVersion2016))
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
                        CustomMessageBox.Show(LauncherWindow.launcherInstance.FindResource("item121").ToString().Replace("\\n\\n", Environment.NewLine + Environment.NewLine), LauncherWindow.launcherInstance, true);
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
            // Deletes old patch files if any of them are already in the directory, including the .zip in the case of corruption
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
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\Patch2016.zip");
                File.Delete($"{Properties.Settings.Default.activeDirectory}\\AssetPatch2015.zip");
            }

            // Unzip all of the files to directory
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
            if (gameVersionString == "15jan2015")
                Properties.Settings.Default.currentPatchVersion2015 = latestPatchVersion;
            else
                Properties.Settings.Default.currentPatchVersion2016 = latestPatchVersion;

            Properties.Settings.Default.Save();
        }
    }
}