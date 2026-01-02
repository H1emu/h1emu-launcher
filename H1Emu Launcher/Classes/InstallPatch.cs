using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace H1Emu_Launcher.Classes
{
    class InstallPatchClass
    {
        public static async Task<bool> InstallPatch()
        {
            // Unzip all of the files to the root directory
            try
            {
                if (Properties.Settings.Default.selectedAssetPack == 0)
                    LauncherWindow.launcherInstance.playButton.SetResourceReference(Button.ContentProperty, "item150");
                else
                    LauncherWindow.launcherInstance.playButton.SetResourceReference(Button.ContentProperty, "item188");

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
                    Bitmap fairPlayLogo = new Bitmap(Properties.Resources.logo);
                    fairPlayLogo.Save($"{Properties.Settings.Default.activeDirectory}\\logo.bmp", ImageFormat.Bmp);

                    // Extract lz4.dll file patch for smaller data sizes using compression
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\lz4.dll", Properties.Resources.lz4);

                    // Extract patched Locale files
                    File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\Locale\\Locales.zip", Properties.Resources.Locales);
                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Locale\\Locales.zip", $"{Properties.Settings.Default.activeDirectory}\\Locale", true);

                    // Clean up Assets directory
                    foreach (var file in Directory.GetFiles($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets"))
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

                    // Download & extract asset files from the selected asset pack
                    string assetPackDownloadURL = string.Empty;

                    // Get the download URL for the selected asset pack
                    if (Properties.Settings.Default.selectedAssetPack == 0)
                        assetPackDownloadURL = Info.OFFICIAL_ASSET_PACK;
                    else
                    {
                        List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
                        assetPackDownloadURL = assetPackJson[Properties.Settings.Default.selectedAssetPack - 2].AssetPackURL;
                    }

                    // Delete any old installation files if they exist in case of corruption
                    if (File.Exists($"{Properties.Settings.Default.activeDirectory}\\Assets_Pack.zip"))
                        File.Delete($"{Properties.Settings.Default.activeDirectory}\\Assets_Pack.zip");

                    HttpResponseMessage downloadResponse = await SplashWindow.httpClient.GetAsync(assetPackDownloadURL);
                    // Throw an exception if we didn't get the correct response, with the first letter capitalised in the message
                    if (downloadResponse.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"{char.ToUpper(downloadResponse.ReasonPhrase.First())}{downloadResponse.ReasonPhrase.Substring(1)}");

                    long totalBytes = downloadResponse.Content.Headers.ContentLength ?? -1L;
                    using Stream contentStream = await downloadResponse.Content.ReadAsStreamAsync();
                    using (FileStream fileStream = new($"{Properties.Settings.Default.activeDirectory}\\Assets_Pack.zip", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        long totalBytesRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                        {
                            // Write the data to the file
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                            totalBytesRead += bytesRead;
                        }
                    };

                    ZipFile.ExtractToDirectory($"{Properties.Settings.Default.activeDirectory}\\Assets_Pack.zip", $"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets", true);
                }

                // Delete BattlEye folder to prevent Steam from trying to launch the game
                if (Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\BattlEye"))
                    Directory.Delete($"{Properties.Settings.Default.activeDirectory}\\BattlEye", true);

                // Replace users ClientConfig.ini with modified version
                File.WriteAllBytes($"{Properties.Settings.Default.activeDirectory}\\ClientConfig.ini", Properties.Resources.CustomClientConfig);

                // Delete any no longer needed files/old patches
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
            }
            catch (Exception e)
            {
                LauncherWindow.launcherInstance.playButton.IsEnabled = true;
                LauncherWindow.launcherInstance.playButton.SetResourceReference(Button.ContentProperty, "item8");

                if (Properties.Settings.Default.gameVersionString == "22dec2016")
                    CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item96")}\n\n{e.Message}", LauncherWindow.launcherInstance);
                return false;
            }

            LauncherWindow.launcherInstance.playButton.SetResourceReference(Button.ContentProperty, "item217");
            return true;
        }
    }
}