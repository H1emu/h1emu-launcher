using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Windows.Controls;
using H1EmuLauncher.Classes;
using System.Windows.Media.Animation;

namespace H1EmuLauncher
{
    public partial class SettingsWindow : Window
    {
        readonly ProcessStartInfo cmdShell = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        public static string accountKeyArgument;
        public static bool openAccountKeyPage;
        public static SettingsWindow settingsInstance;
        public Storyboard UnfocusPropertiesAnimationShow;
        public Storyboard UnfocusPropertiesAnimationHide;

        public SettingsWindow()
        {
            InitializeComponent();
            settingsInstance = this;
            Owner = LauncherWindow.launcherInstance;

            UnfocusPropertiesAnimationShow = FindResource("UnfocusPropertiesShow") as Storyboard;
            UnfocusPropertiesAnimationHide = FindResource("UnfocusPropertiesHide") as Storyboard;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        public void MoveSettingsWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void SwitchToAccountKeyTab()
        {
            if (settingsInstance.settingsTabControl.SelectedIndex == 1)
            {
                SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxHint.Visibility = Visibility.Hidden;

                if (SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxText.Visibility == Visibility.Visible)
                    SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxText.Text = accountKeyArgument;
                else
                    SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxPassword.Password = accountKeyArgument;
            }
            else
            {
                settingsInstance.settingsTabControl.SelectedIndex = 1;
                SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxPassword.Password = accountKeyArgument;
            }

            LauncherWindow.rawArgs = null;
            openAccountKeyPage = false;
            accountKeyArgument = null;
        }

        private void SettingsTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (settingsTabControl.SelectedIndex)
            {
                case 0:
                    Title = $"{FindResource("item206")} - {FindResource("item195")}";
                    break;
                case 1:
                    Title = $"{FindResource("item206")} - {FindResource("item196")}";
                    break;
                case 2:
                    Title = $"{FindResource("item206")} - {FindResource("item197")}";
                    break;
                case 3:
                    Title = $"{FindResource("item206")} - {FindResource("item198")}";
                    break;
                case 4:
                    Title = $"{FindResource("item206")} - {FindResource("item199")}";
                    break;
            }
        }

        public void SettingsWindowLoaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();
        }

        private void SettingsWindowContentRendered(object sender, EventArgs e)
        {
            Top = (LauncherWindow.launcherInstance.Top + LauncherWindow.launcherInstance.Height / 2) - (Height / 2);

            // If accountkey argument was specified launch the accountkey window with the argument value
            if (openAccountKeyPage)
            {
                settingsTabControl.SelectedIndex = 1;
                SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxPassword.Password = accountKeyArgument;
                LauncherWindow.rawArgs = null;
                openAccountKeyPage = false;
                accountKeyArgument = null;
            }
        }

        public void CloseSettingsWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible && SettingsPages.GameFiles.isExecutingTasks)
            {
                CustomMessageBox.Show(FindResource("item73").ToString(), this);
                e.Cancel = true;
                return;
            }

            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();
            LauncherWindow.launcherInstance.launcherFade.IsHitTestVisible = false;
            settingsInstance = null;
        }
    }
}