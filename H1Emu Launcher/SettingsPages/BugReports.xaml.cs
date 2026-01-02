using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher.SettingsPages
{
    public partial class BugReports : Page
    {
        public static BugReports bugReportsInstance;

        public BugReports()
        {
            InitializeComponent();
            bugReportsInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void BugReportsGithubServer(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.SERVER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void BugReportsGithubLauncher(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.LAUNCHER_BUG_LINK,
                UseShellExecute = true
            });
        }
    }
}
