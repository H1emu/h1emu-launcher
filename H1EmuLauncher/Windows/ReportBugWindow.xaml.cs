using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());

            reportBugGitHubServerLink.Text = Info.SERVER_BUG_LINK;
            reportBugGitHubLauncherLink.Text = Info.LAUNCHER_BUG_LINK;
        }

        private void ReportBugTopBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseReportBug(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
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
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();
        }

        public bool IsCompleted = false;

        private void MainReportBugClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();

                e.Cancel = true;
                Storyboard sb = FindResource("CloseReportBug") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, o) =>
                    {
                        IsCompleted = true;
                        Close();
                    };

                    sb.Begin();
                }
            }
        }
    }
}