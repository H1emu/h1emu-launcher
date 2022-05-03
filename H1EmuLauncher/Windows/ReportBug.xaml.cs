using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class ReportBug : Window
    {
        public static ReportBug aboutPageInstance;

        public ReportBug()
        {
            InitializeComponent();
            aboutPageInstance = this;
            Owner = Launcher.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            reportBugGitHubServerLink.Text = Info.GITHUB_SERVER_BUG_LINK;
            reportBugGitHubLauncherLink.Text = Info.GITHUB_LAUNCHER_BUG_LINK;
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
                FileName = Info.GITHUB_SERVER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubServerLinkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.GITHUB_SERVER_BUG_LINK);
        }

        private void ReportBugGithubLauncher(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.GITHUB_LAUNCHER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubServerLauncherCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Info.GITHUB_LAUNCHER_BUG_LINK);
        }

        private void MainReportBugLoaded(object sender, RoutedEventArgs e)
        {
            Launcher.launcherInstance.launcherBlur.Radius = 15;
            Launcher.launcherInstance.launcherFade.Visibility = Visibility.Visible;
        }

        private void MainReportBugClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Launcher.launcherInstance.launcherBlur.Radius = 0;
            Launcher.launcherInstance.launcherFade.Visibility = Visibility.Hidden;
        }

        private void MainReportBugActivated(object sender, EventArgs e)
        {
            aboutPageBlur.Radius = 0;
            aboutPageFade.Visibility = Visibility.Hidden;
        }
    }
}