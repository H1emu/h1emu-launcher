using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

namespace H1EMU_Redux
{
    /// <summary>
    /// Interaction logic for Launcher.xaml
    /// </summary>
    public partial class Launcher : Window
    {
        public static string recentDateServer;
        public static string latestUpdateVersionServer;


        public static ManualResetEvent ma = new ManualResetEvent(false);
        public static Launcher lncher;

        public Launcher()
        {
            InitializeComponent();

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(200d);
            fadeAnimation.From = 0.0d;
            fadeAnimation.To = 1.0d;
            MainLauncher.BeginAnimation(OpacityProperty, fadeAnimation);

            lncher = this;
        }

        private void LaunchServer(object sender, RoutedEventArgs e)
        {
            ma.Reset();

            new Thread(() => 
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || Properties.Settings.Default.activeDirectory == "Directory")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate 
                    {
                        CustomMessageBox.Show("You don't seem to have an active directory set.\n\nSet this in the Settings menu.");
                    });

                    return;
                }

                if (!Directory.Exists($"{Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master"))
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("You don't seem to have server files installed.\n\nInstall these in the Settings menu.");
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
                        CustomMessageBox.Show("Game either not found or not supported by H1Emu.");
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
                        sw.WriteLine($"SET PATH={Properties.Settings.Default.activeDirectory}\\H1emuServersFiles\\h1z1-server-QuickStart-master\\node-v16.2.0-win-x64");
                        sw.WriteLine($"cd /d {Properties.Settings.Default.activeDirectory}\\H1EmuServersFiles\\h1z1-server-QuickStart-master");
                        sw.WriteLine(serverVersion);
                    }
                }

            }).Start();
        }

        private void LaunchClientCustom(object sender, RoutedEventArgs e)
        {
            ma.Reset();

            new Thread(() => 
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || Properties.Settings.Default.activeDirectory == "Directory")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("You don't seem to have an active directory set.\n\nSet this in the Settings menu.");
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

                if (gameVersion == "22dec2016" || gameVersion == "15jan2015")
                {
                    bool result = true;

                    if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            DialogResult dialogResult = CustomMessageBox.ShowResult("The selected directory does not contain patch files.\n\nWould you like to continue anyway? (You won't be able to load into servers)");
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
                        StartInfo = new ProcessStartInfo(Properties.Settings.Default.activeDirectory + "\\H1Z1.exe", "sessionid=115 server=localhost:1115")
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
                        CustomMessageBox.Show("Game either not found or not supported by H1Emu.");
                    });

                    return;
                }
                else
                {
                    return;
                }

            }).Start();
        }

        private void LaunchClientTestServer(object sender, RoutedEventArgs e)
        {
            ma.Reset();

            new Thread(() => 
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.activeDirectory) || Properties.Settings.Default.activeDirectory == "Directory")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("You don't seem to have an active directory set.\n\nSet this in the Settings menu.");
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

                if (gameVersion == "15jan2015")
                {
                    bool result = true;

                    if (!File.Exists($"{Properties.Settings.Default.activeDirectory}\\dinput8.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\msvcp140d.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\ucrtbased.dll") || !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140d.dll") ||
                        !File.Exists($"{Properties.Settings.Default.activeDirectory}\\vcruntime140_1d.dll"))
                    {
                        Dispatcher.Invoke((MethodInvoker)delegate
                        {
                            DialogResult dialogResult = CustomMessageBox.ShowResult("The selected directory does not contain patch files.\n\nWould you like to continue anyway? (You won't be able to load into servers)");
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
                        StartInfo = new ProcessStartInfo(Properties.Settings.Default.activeDirectory + "\\H1Z1.exe", "sessionid=0 server=loginserver.h1emu.com:1115")
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            WorkingDirectory = Properties.Settings.Default.activeDirectory,
                            UseShellExecute = true
                        }
                    };

                    process.Start();
                }
                else if (gameVersion == "22dec2016")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("There is no test server for this version of the game currently. (December 2016)");
                    });

                    return;
                }
                else if (gameVersion != "processBeingUsed")
                {
                    Dispatcher.Invoke((MethodInvoker)delegate
                    {
                        CustomMessageBox.Show("Game either not found or not supported by H1Emu.");
                    });

                    return;
                }
                else
                {
                    return;
                }

            }).Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(recentDateServer) || !string.IsNullOrEmpty(latestUpdateVersionServer))
            {
                try
                {
                    var date = DateTime.ParseExact(recentDateServer, "G", CultureInfo.InvariantCulture);

                    titleUpdateText.Text = $"Update Version {latestUpdateVersionServer}";
                    datePublished.Text = $"({date:dd MMMM yyyy})";
                }
                catch { }
            }
            else
            {
                try
                {
                    var date = DateTime.ParseExact(Properties.Settings.Default.publishDate, "G", CultureInfo.InvariantCulture);

                    titleUpdateText.Text = $"Update Version {Properties.Settings.Default.latestServerVersion}";
                    datePublished.Text = $"({date:dd MMMM yyyy})";
                }
                catch { }
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley",
                UseShellExecute = true
            });
        }

        private void CloseLauncher(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(200d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainLauncher.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainLauncher.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            this.Close();
        }

        private void MainLauncher_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}