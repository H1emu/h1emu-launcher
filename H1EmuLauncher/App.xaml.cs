using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;

namespace H1EmuLauncher
{
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) 
            {
                File.WriteAllText($"{Classes.Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\args.txt", string.Join(" ", e.Args));
                Environment.Exit(0);
            }

            if (e.Args.Length > 0)
            {
                LauncherWindow.rawArgs = e.Args;
                LauncherWindow.executeArguments = true;
            }

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 60 });

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(Classes.SetLanguageFile.LoadFile());

            new UpdateWindow();
        }
    }
}
