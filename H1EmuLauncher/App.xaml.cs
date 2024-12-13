using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class App
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                // Remove some characters
                SendArgumentsToRunningInstance(string.Join(' ', e.Args).Replace("h1emulauncher://", "").Replace("/\"", "").Replace("%20", " ").Split(' '));
                Environment.Exit(0);
            }

            if (e.Args.Length > 0)
            {
                LauncherWindow.rawArgs = e.Args;
            }

            StartPipeServer();

            // Lower the framerate of animations otherwise GPU usage is high
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

            // Delete old setup file
            if (File.Exists($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{UpdateWindow.installerFileName}"))
                File.Delete($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\{UpdateWindow.installerFileName}");

            // Delete old carousel images folder, no longer needed on newer versions of the launcher
            if (Directory.Exists($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\CarouselImages"))
                Directory.Delete($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher\\CarouselImages", true);

            SplashWindow sp = new();
            sp.Show();
        }

        private void StartPipeServer()
        {
            new Thread(() =>
            {
                while (true)
                {
                    NamedPipeServerStream pipeServer = new("H1EmuLauncherPipe", PipeDirection.In);
                    pipeServer.WaitForConnection();

                    StreamReader reader = new(pipeServer);
                    string args = reader.ReadToEnd();

                    Dispatcher.Invoke(new Action(delegate
                    {
                        LauncherWindow.launcherInstance.ExecuteArguments(args.Split(' '));
                    }));

                    pipeServer.Dispose();
                }
            }).Start();
        }

        private void SendArgumentsToRunningInstance(string[] args)
        {
            try
            {
                NamedPipeClientStream pipeClient = new(".", "H1EmuLauncherPipe", PipeDirection.Out);
                StreamWriter writer = new(pipeClient);

                pipeClient.Connect(1000);
                writer.Write(string.Join(' ', args));
                writer.Flush();
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"Error sending argumentsto active instance: \"{e.Message}\".");
            }
        }

        private void TextBoxContextMenuPasteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = (MenuItem)sender;
                ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
                UIElement box = contextMenu.PlacementTarget;
                ApplicationCommands.Paste.Execute(null, box);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item184")} \"{ex.Message}\".");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            {
                if (LauncherWindow.launcherInstance == null)
                {
                    MessageBoxResult mbr = CustomMessageBox.Show($"An exception occurred that prevented the application from starting: \"{(e.ExceptionObject as Exception).Message}\".\n\nDeleting the application data can sometimes fix this, would you like to try that and attempt to restart now?", null, false, true, true);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        DirectoryInfo di = new($"{Info.APPLICATION_DATA_PATH}\\H1Emu Launcher");
                        foreach (var file in di.GetFiles())
                            file.Delete();

                        System.Windows.Forms.Application.Restart();
                    }
                }

                CustomMessageBox.Show($"An unhandled exception occurred: \"{(e.ExceptionObject as Exception).Message}\".\n\nThe launcher will now exit.", LauncherWindow.launcherInstance);
                Environment.Exit(1);
            }
        }
    }
}
