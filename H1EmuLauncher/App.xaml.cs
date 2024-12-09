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

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

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
            MessageBoxResult mbr = CustomMessageBox.Show($"An error occurred that prevented the application from starting: \"{(e.ExceptionObject as Exception).Message}\".\n\nDeleting the application data can sometimes fix this, would you like to try that and attempt to restart now?", null, false, true, true);
            if (mbr == MessageBoxResult.Yes)
            {
                DirectoryInfo di = new($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher");
                foreach (var file in di.GetFiles())
                    file.Delete();

                System.Windows.Forms.Application.Restart();
            }

            Environment.Exit(1);
        }
    }
}
