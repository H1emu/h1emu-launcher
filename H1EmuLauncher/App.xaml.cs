using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;

namespace H1EmuLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                File.WriteAllText($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", string.Join(" ", e.Args));
                Environment.Exit(0);
            }

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 30 });

            LauncherWindow.rawArgs = e.Args;
            new UpdateWindow();
        }
    }
}
