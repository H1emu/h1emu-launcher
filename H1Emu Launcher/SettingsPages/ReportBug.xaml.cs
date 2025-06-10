using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SettingsPages
{
    public partial class ReportBug : Page
    {
        public static ReportBug reportBugInstance;

        public ReportBug()
        {
            InitializeComponent();
            reportBugInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void ReportBugGithubServer(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.SERVER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubLauncher(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.LAUNCHER_BUG_LINK,
                UseShellExecute = true
            });
        }
    }
}
