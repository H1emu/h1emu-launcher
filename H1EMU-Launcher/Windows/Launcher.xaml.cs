using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using H1EMU_Launcher.Resources;
using System.Text.Json.Serialization;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Net;
using Newtonsoft.Json;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for Launcher.xaml
    /// </summary>

    public partial class Launcher : Window
    {

#pragma warning disable SYSLIB0014 // Warning saying that WebClient is discontinued and not supported anymore.
        public static ManualResetEvent ma = new ManualResetEvent(false);
        public static Launcher lncher;

        public static string serverJsonFile;

        public class Server
        {
            [JsonPropertyName("Server Name")]
            public string SName { get; set; }
            [JsonPropertyName("Server Address")]
            public string SAddress { get; set; }
        }

        public Launcher()
        {
            InitializeComponent();
            lncher = this;

            //Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void ServerSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.lastServer = serverSelector.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void LoadServers()
        {
            try
            {
                List<Server> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<Server>>(File.ReadAllText(serverJsonFile));
                foreach (var item in currentjson)
                {
                    serverSelector.Items.Insert(serverSelector.Items.Count - 2, item.SName);
                }
            }
            catch { }

            try
            {
                serverSelector.SelectedIndex = Properties.Settings.Default.lastServer;
            }
            catch
            {
                serverSelector.SelectedIndex = 0;
            }
        }

        private void DeleteServer(object sender, RoutedEventArgs e)
        {
            string deleteName = "";
            string deleteIP = "";

            if (serverSelector.SelectedIndex == 0 || serverSelector.SelectedIndex == 1 || serverSelector.SelectedIndex == serverSelector.Items.Count - 1 || string.IsNullOrEmpty(serverSelector.Text))
            {
                CustomMessageBox.Show(FindResource("item146").ToString());
                return;
            }

            DialogResult dr = CustomMessageBox.ShowResult(FindResource("item147").ToString());
            if (dr != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            List<Server> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<Server>>(File.ReadAllText(serverJsonFile));

            int index = -1;

            foreach (var item in currentjson)
            {
                index++;

                if (item.SName == serverSelector.Text)
                {
                    deleteName = item.SName;
                    deleteIP = item.SAddress;
                    break;
                }
            }

            currentjson.Remove(currentjson[index]);

            var finalJson = System.Text.Json.JsonSerializer.Serialize<List<Server>>(currentjson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(serverJsonFile, finalJson);

            serverSelector.Items.Remove(serverSelector.SelectedItem);
            serverSelector.SelectedIndex = 0;
        }

        private void AddNewServer(object sender, MouseButtonEventArgs e)
        {
            DialogResult dr = CustomMessageBox.AddServer();
            if (dr != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            string serverName  = CustomMessageBox.newServerName;
            string serverIp  = CustomMessageBox.newServerIp;

            List<Server> currentjson = new List<Server>();

            try
            {
                if (serverName.Trim() == FindResource("item139").ToString() || serverName.Trim() == FindResource("item140").ToString() || serverName.Trim() == FindResource("item141").ToString())
                {
                    throw new Exception(FindResource("item143").ToString());
                }

                if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(serverIp))
                {
                    throw new Exception(FindResource("item151").ToString());
                }

                if (File.Exists(serverJsonFile))
                {
                    currentjson = System.Text.Json.JsonSerializer.Deserialize<List<Server>>(File.ReadAllText(serverJsonFile));
                    currentjson.Add(new Server()
                    {
                        SName = serverName.Trim(),
                        SAddress = serverIp.Trim().Replace(" ", "")
                    });

                    var newJson = System.Text.Json.JsonSerializer.Serialize<List<Server>>(currentjson, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(serverJsonFile, newJson);
                    serverSelector.Items.Insert(serverSelector.Items.Count - 2, serverName.Trim());
                    serverSelector.SelectedIndex = serverSelector.Items.Count - 3;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(FindResource("item142").ToString().Replace("{0}", ex.Message));
            }
        }

        public bool LaunchServer(string gameVersion)
        {
            ma.Reset();

            try
            {
                if (!Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master\\node_modules"))
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item52").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));
                    });

                    return false;
                }

                string serverVersion = "";

                if (gameVersion == "15jan2015")
                {
                    serverVersion = "npm start";
                }
                else if (gameVersion == "22dec2016")
                {
                    serverVersion = "npm run start-2016";
                }

                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v{Classes.Info.NODEJS_VERSION}-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine(serverVersion);
                    }
                }

                p.WaitForExit(5000);

                if (p.HasExited)
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item168").ToString().Replace("\n\n", Environment.NewLine + Environment.NewLine));
                    });

                    return false;
                }
            }
            catch (Exception er)
            {
                Dispatcher.Invoke((MethodInvoker)delegate
                {
                    CustomMessageBox.Show(FindResource("item53").ToString() + $" \"{er.Message}\"");
                });

                return false;
            }

            return true;
        }

        private void LaunchClient(object sender, RoutedEventArgs e)
        {
            ma.Reset();

            Settings settings = new Settings();

            if (!settings.CheckDirectory())
            {
                return;
            }

            string gameVersion = "";
            string serverIp = "";
            string sessionId = "0";

            new Thread(() =>
            {
                try
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        if (serverSelector.SelectedIndex == 0) { return; }
                        if (serverSelector.SelectedIndex == 1) { serverIp = "localhost:1115"; return; }
                    });

                    List<Server> currentjson = System.Text.Json.JsonSerializer.Deserialize<List<Server>>(File.ReadAllText(serverJsonFile));

                    foreach (var item in currentjson)
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            if (item.SName == serverSelector.Text)
                            {
                                serverIp = item.SAddress;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show($"Exception thrown: {ex.Message}");
                    });
                }

                try
                {
                    if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || Properties.Settings.Default.activeDirectory == "Directory")
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item51").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));
                        });

                        return;
                    }

                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        settings.CheckGameVersionNewThread();
                    });

                    ma.WaitOne();
                    ma.Reset();

                    gameVersion = Settings.gameVersion;

                    if (gameVersion == "22dec2016" || gameVersion == "15jan2015")
                    {
                        if (gameVersion == "22dec2016" && serverIp == "") { serverIp = "loginserver.h1emu.com:1116"; }
                        if (gameVersion == "15jan2015" && serverIp == "") { serverIp = "loginserver.h1emu.com:1115"; }

                        if (serverIp == "loginserver.h1emu.com:1116" || serverIp == "loginserver.h1emu.com:1115")
                        {
                            if (string.IsNullOrEmpty(Properties.Settings.Default.sessionIdKey))
                            {
                                Dispatcher.Invoke((MethodInvoker)delegate
                                {
                                    CustomMessageBox.Show(FindResource("item153").ToString());
                                });

                                return;
                            }
                            else
                            {
                                sessionId = Properties.Settings.Default.sessionIdKey;
                            }
                        }

                        CheckPatchVersion.CheckPatch();

                        ma.WaitOne();
                        ma.Reset();

                        bool result = true;

                        if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                            !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                            !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                        {
                            Dispatcher.Invoke((MethodInvoker)delegate
                            {
                                DialogResult dialogResult = CustomMessageBox.ShowResult(FindResource("item16").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));
                                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            });
                        }

                        if (!result) { return; }

                        if (serverIp == "localhost:1115")
                        {
                            if (!LaunchServer(gameVersion)) { return; }
                        }

                        Process process = new Process()
                        {
                            StartInfo = new ProcessStartInfo($"{Properties.Settings.Default.activeDirectory}\\H1Z1.exe", $"sessionid={sessionId} gamecrashurl={Classes.Info.GAME_CRASH_URL} server={serverIp}")
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                WorkingDirectory = Properties.Settings.Default.activeDirectory,
                                UseShellExecute = true
                            }
                        };

                        process.Start();
                    }
                    else if (gameVersion == "processBeingUsed")
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item121").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));
                        });

                        return;
                    }
                    else
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item58").ToString());
                        });

                        return;
                    }
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item13").ToString() + $" \"{er.Message}\"");
                    });
                }

            }).Start();
        }

        private void LauncherWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.firstTimeUse != 1) 
            {
                Disclaimer dc = new Disclaimer();
                dc.ShowDialog();
            }

            try
            {
                File.Delete($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\{MainWindow.downloadFileName}");
            }
            catch { }

            try
            {
                Directory.Delete($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages", true);
            }
            catch { }

            Directory.CreateDirectory($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages");
            Directory.CreateDirectory($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher");
            serverJsonFile = System.IO.Path.Combine($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher", "servers.json");
            if (!File.Exists(serverJsonFile)) { File.WriteAllText(serverJsonFile, "[]"); }
            
            LoadServers();
            Classes.Carousel.BeginImageCarousel();
            LangBox.SelectedIndex = Properties.Settings.Default.language;

            VersionInformation();
        }

        public void VersionInformation()
        {
            string latestVersion = null;
            string latestPatchNotes = null;
            string publishDate = null;

            try
            {
                // Update version, date published and patch notes code.

                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "d-fens HttpClient");

                string jsonServer = wc.DownloadString(new Uri(Classes.Info.SERVER_JSON_API));

                // Get latest release number and date published for server.

                var jsonDesServer = JsonConvert.DeserializeObject<dynamic>(jsonServer);
                latestVersion = jsonDesServer.tag_name;
                latestPatchNotes = jsonDesServer.body;
                publishDate = jsonDesServer.published_at;

                // Store the latest server version, date and patch notes in the case of no internet.

                Properties.Settings.Default.latestServerVersion = latestVersion;
                Properties.Settings.Default.patchNotes = latestPatchNotes;
                Properties.Settings.Default.publishDate = publishDate;
                Properties.Settings.Default.Save();
            }
            catch { }

            if (!string.IsNullOrEmpty(latestVersion) || !string.IsNullOrEmpty(latestPatchNotes) || !string.IsNullOrEmpty(publishDate))
            {
                try
                {
                    var date = DateTime.ParseExact(publishDate, "G", CultureInfo.InvariantCulture);

                    updateVersion.Text = $" {latestVersion}";
                    datePublished.Text = $"({date:dd MMMM yyyy})";
                    patchNotesBox.Document.Blocks.Clear();
                    patchNotesBox.Document.Blocks.Add(new Paragraph(new Run(latestPatchNotes)));
                }
                catch { }
            }
            else
            {
                try
                {
                    var date = DateTime.ParseExact(Properties.Settings.Default.publishDate, "G", CultureInfo.InvariantCulture);

                    updateVersion.Text = $" {Properties.Settings.Default.latestServerVersion}";
                    datePublished.Text = $"({date:dd MMMM yyyy})";
                    patchNotesBox.Document.Blocks.Clear();
                    patchNotesBox.Document.Blocks.Add(new Paragraph(new Run(Properties.Settings.Default.patchNotes)));
                }
                catch { }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedLanguage = LangBox.SelectedIndex;

            switch (selectedLanguage)
            {
                //Update and save settings
                case 0:
                    SetLanguageFile.SaveLang(0);
                    break;
                case 1:
                    SetLanguageFile.SaveLang(1);
                    break;
                case 2:
                    SetLanguageFile.SaveLang(2);
                    break;
                case 3:
                    SetLanguageFile.SaveLang(3);
                    break;
                case 4:
                    SetLanguageFile.SaveLang(4);
                    break;
                case 5:
                    SetLanguageFile.SaveLang(5);
                    break;
                case 6:
                    SetLanguageFile.SaveLang(6);
                    break;
                case 7:
                    SetLanguageFile.SaveLang(7);
                    break;
                case 8:
                    SetLanguageFile.SaveLang(8);
                    break;
                case 9:
                    SetLanguageFile.SaveLang(9);
                    break;
                case 10:
                    SetLanguageFile.SaveLang(10);
                    break;
                case 11:
                    SetLanguageFile.SaveLang(11);
                    break;
                default:
                    CustomMessageBox.Show("Error selecting language.");
                    return;
            }

            //Reload pages
            ContentDownloader.UpdateLang();
            SteamFrame.Refresh();
            playButton.Content = FindResource("item8").ToString();
        }

        public bool doContinue = true;

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            doContinue = true;
        }

        private void PrevImageClick(object sender, RoutedEventArgs e)
        {
            if (!doContinue) { return; }

            if (carouselProgressBar.Value != 3000)
            {
                Classes.Carousel.progress = 0;
                carouselProgressBar.Value = 0;
            }

            ButtonAutomationPeer peer = new ButtonAutomationPeer(fauxPrevImage);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();

            doContinue = false;
        }

        private void NextImageClick(object sender, RoutedEventArgs e)
        {
            if (!doContinue) { return; }

            if (carouselProgressBar.Value != 3000)
            {
                Classes.Carousel.progress = 0;
                carouselProgressBar.Value = 0;
            }

            ButtonAutomationPeer peer = new ButtonAutomationPeer(fauxNextImage);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();

            doContinue = false;
        }

        private void FauxPrevImageClick(object sender, RoutedEventArgs e)
        {
            Classes.Carousel.PreviousImage();
        }

        private void FauxNextImageClick(object sender, RoutedEventArgs e)
        {
            Classes.Carousel.NextImage();
        }

        private void CarouselMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Classes.Carousel.pauseCarousel.Reset();

            prevImage.Visibility = Visibility.Visible;
            nextImage.Visibility = Visibility.Visible;
        }

        private void CarouselMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Classes.Carousel.pauseCarousel.Set();

            prevImage.Visibility = Visibility.Hidden;
            nextImage.Visibility = Visibility.Hidden;
        }

        private void AboutHyperlink(object sender, RoutedEventArgs e)
        {
            AboutPage aboutPage = new AboutPage();
            aboutPage.ShowDialog();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void H1Hyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri.ToString(),
                UseShellExecute = true
            });
        }

        private void FullUpdatesHyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri.ToString(),
                UseShellExecute = true
            });
        }

        private void OpenDiscord(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Classes.Info.DISCORD_LINK,
                UseShellExecute = true
            });
        }

        private void LogoClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Classes.Info.LOGO_LINK,
                UseShellExecute = true
            });
        }

        private void patchNotesCopy(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(Classes.Info.CHANGELOG);
        }

        private void websiteLinkCopy(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(Classes.Info.WEBSITE);
        }

        private void discordLinkCopy(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(Classes.Info.DISCORD_LINK);
        }

        private void CloseLauncher(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainLauncher_Closed(object sender, EventArgs e)
        {
            Environment.Exit(69);
        }

        private void MiniButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainLauncherActivated(object sender, EventArgs e)
        {
            launcherBlur.Radius = 0;
            launcherFade.Visibility = Visibility.Hidden;
        }
    }
}