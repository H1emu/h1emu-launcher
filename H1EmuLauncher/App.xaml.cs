using H1EmuLauncher.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace H1EmuLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1 && e.Args[0] == "INSTALLER")
            {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Environment.Exit(0);
            }

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                File.WriteAllText($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", string.Join(" ", e.Args));
                Environment.Exit(0);
            }

            LauncherWindow.rawArgs = e.Args;
            new UpdateWindow();
        }
    }
}
