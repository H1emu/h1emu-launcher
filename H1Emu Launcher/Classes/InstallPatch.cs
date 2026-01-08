using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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
                LauncherWindow.launcherInstance.playButton.SetResourceReference(Button.ContentProperty, "item150");

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

                    // Get the download URL for the selected asset pack
                    string assetPackJsonURL = string.Empty;

                    if (Properties.Settings.Default.selectedAssetPack == 0)
                        assetPackJsonURL = Info.OFFICIAL_ASSET_PACK;
                    else
                    {
                        List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
                        assetPackJsonURL = assetPackJson[Properties.Settings.Default.selectedAssetPack - 2].AssetPackURL;
                    }

                    // Query the asset pack JSON URL
                    HttpResponseMessage response = await SplashWindow.httpClient.GetAsync(assetPackJsonURL, HttpCompletionOption.ResponseHeadersRead);

                    // Throw an exception if we didn't get the correct response, with the first letter in the message capitalised
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"{char.ToUpper(response.ReasonPhrase.First())}{response.ReasonPhrase.Substring(1)}");

                    // Deserialise the asset pack JSON into an object
                    string jsonAssetPack = await response.Content.ReadAsStringAsync();
                    JsonEndPoints.AssetPackJson.Root jsonAssetPackDes = JsonSerializer.Deserialize<JsonEndPoints.AssetPackJson.Root>(jsonAssetPack);

                    List<string> verifiedAssets = [];
                    for (int i = 0; i <= 255; i++)
                        verifiedAssets.Add($"Assets_{i:D3}.pack");

                    // For each asset in the JSON, download the asset file
                    foreach (JsonEndPoints.AssetPackJson.Asset item in jsonAssetPackDes.assets)
                    {
                        bool isDownloadNeeded = false;

                        if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\{item.filename}"))
                            isDownloadNeeded = true;
                        else
                        {
                            using var sha256 = SHA256.Create();
                            using var stream = File.OpenRead($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets\\{item.filename}");

                            byte[] hash = sha256.ComputeHash(stream);

                            string hashHex = Convert.ToHexString(hash); // .NET 5+

                            if (!hashHex.Equals(item.hash.Replace("sha256:", ""), StringComparison.OrdinalIgnoreCase))
                                isDownloadNeeded = true;

                        }

                        if (isDownloadNeeded)
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

                                    // Update the play button text to show the progress
                                    if (totalBytes > 0)
                                    {
                                        float progressPercentage = (float)totalBytesRead * 100 / totalBytes;
                                        LauncherWindow.launcherInstance.playButton.Content = LauncherWindow.launcherInstance.FindResource("item150") + $" {progressPercentage:0.00}%";
                                    }
                                }
                            };
                        }

                        verifiedAssets.Add(item.filename);
                    }

                    // Make sure that only the default game assets and the newly installed asset pack is the only thing in the "Assets" folder
                    foreach (string file in Directory.GetFiles($"{Properties.Settings.Default.activeDirectory}\\Resources\\Assets"))
                    {
                        string fileName = Path.GetFileName(file);
                        if (!verifiedAssets.Contains(fileName))
                            File.Delete(file);
                    }
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