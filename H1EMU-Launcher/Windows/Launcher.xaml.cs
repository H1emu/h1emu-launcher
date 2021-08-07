using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using H1EMU_Launcher.Resources;
using System.Text.Json.Serialization;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for Launcher.xaml
    /// </summary>

    public partial class Launcher : Window
    {

#pragma warning disable SYSLIB0014 // Warning saying that WebClient is discontinued and not supported anymore.

        public static string recentDateServer;
        public static string latestUpdateVersionServer;
        public static string patchNotes;

        public static string serverJsonFile = null;
        public static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static ManualResetEvent ma = new ManualResetEvent(false);
        public static Launcher lncher;

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

            //Set just language code ex: en-us, fr-ca
            SetLanguageFile.SetLanguageCode();

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

        private void LaunchServer(object sender, RoutedEventArgs e)
        {
            ma.Reset();

            new Thread(() => 
            {
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

                    if (!Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master\\node_modules"))
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item52").ToString().Replace("\\" + "n" + "\\" + "n", Environment.NewLine + Environment.NewLine));
                        });

                        return;
                    }

                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        Settings settings = new Settings();
                        settings.CheckGameVersionNewThread();
                    });

                    ma.WaitOne();

                    string gameVersion = Settings.gameVersion;
                    string serverVersion = "";

                    if (gameVersion == "15jan2015")
                    {
                        serverVersion = "npm start";
                    }
                    else if (gameVersion == "22dec2016")
                    {
                        serverVersion = "npm run start-2016";
                    }
                    else if (gameVersion != "processBeingUsed")
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item14").ToString());
                        });

                        return;
                    }
                    else
                    {
                        return;
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
                            sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v16.6.0-win-x64");
                            sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                            sw.WriteLine(serverVersion);
                        }
                    }
                }
                catch (Exception er)
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show(FindResource("item53").ToString() + $" \"{er.Message}\"");
                    });
                }

            }).Start();
        }

        private void LaunchClient(object sender, RoutedEventArgs e)
        {
            ma.Reset();

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
                        Settings settings = new Settings();
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

                        Process process = new Process()
                        {
                            StartInfo = new ProcessStartInfo($"{Properties.Settings.Default.activeDirectory}\\H1Z1.exe", $"sessionid={sessionId} gamecrashurl=https://www.h1emu.com/us/game-error?code=G server={serverIp}")
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                WorkingDirectory = Properties.Settings.Default.activeDirectory,
                                UseShellExecute = true
                            }
                        };

                        process.Start();
                    }
                    else if (gameVersion != "processBeingUsed")
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            CustomMessageBox.Show(FindResource("item14").ToString());
                        });

                        return;
                    }
                    else
                    {
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
            LangBox.SelectedIndex = Properties.Settings.Default.language;

            Directory.CreateDirectory($"{appDataPath}\\H1EmuLauncher\\CarouselImages");
            Directory.CreateDirectory($"{appDataPath}\\H1EmuLauncher");
            serverJsonFile = System.IO.Path.Combine($"{appDataPath}\\H1EmuLauncher", "servers.json");
            if (!File.Exists(serverJsonFile)) { File.WriteAllText(serverJsonFile, "[]"); }
            LoadServers();

            Classes.Carousel.BeginImageCarousel();

            File.Delete($"{appDataPath}\\H1EmuLauncher\\{MainWindow.downloadFileName}");

            // Update version, date published and patch notes code.

            if (!string.IsNullOrEmpty(recentDateServer) || !string.IsNullOrEmpty(latestUpdateVersionServer) || !string.IsNullOrEmpty(patchNotes))
            {
                try
                {
                    var date = DateTime.ParseExact(recentDateServer, "G", CultureInfo.InvariantCulture);

                    updateVersion.Text = $" {latestUpdateVersionServer}";
                    datePublished.Text = $"({date:dd MMMM yyyy})";
                    patchNotesBox.Document.Blocks.Clear();
                    patchNotesBox.Document.Blocks.Add(new Paragraph(new Run(patchNotes)));
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
                case 0:
                    //Update and save settings
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
                default:
                    CustomMessageBox.Show("Error selecting language.");
                    return;
            }

            //Reload pages
            ContentDownloader.UpdateLang();
            SteamFrame.Refresh();
            playButton.Content = FindResource("item8").ToString();
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
                FileName = "https://discord.com/invite/RM6jNkj",
                UseShellExecute = true
            });
        }

        private void LogoClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley",
                UseShellExecute = true
            });
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

        private void PrevImageClick(object sender, RoutedEventArgs e)
        {
            Classes.Carousel.PreviousImage();
        }

        private void NextImageClick(object sender, RoutedEventArgs e)
        {
            Classes.Carousel.NextImage();
        }

        private void CarouselMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            prevImage.Visibility = Visibility.Visible;
            nextImage.Visibility = Visibility.Visible;
        }

        private void CarouselMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            prevImage.Visibility = Visibility.Hidden;
            nextImage.Visibility = Visibility.Hidden;
        }
    }
}