using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class ReportBugWindow : Window
    {
        public static ReportBugWindow aboutPageInstance;

        public ReportBugWindow()
        {
            InitializeComponent();
            aboutPageInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            reportBugGitHubServerLink.Text = Info.SERVER_BUG_LINK;
            reportBugGitHubLauncherLink.Text = Info.LAUNCHER_BUG_LINK;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Close();
        }

        private void ReportBugGithubServer(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.SERVER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubServerLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.SERVER_BUG_LINK);
        }

        private void ReportBugGithubLauncher(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.LAUNCHER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubServerLauncherCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.LAUNCHER_BUG_LINK);
        }

        private void MainReportBugLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 15;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Visible;
        }

        private void MainReportBugClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherWindow.launcherInstance.launcherBlur.Radius = 0;
            LauncherWindow.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void MainReportBugActivated(object sender, EventArgs e)
        {
            reportBugBlur.Radius = 0;
            reportBugFade.Visibility = Visibility.Hidden;
        }
    }
}