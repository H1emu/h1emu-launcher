using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher
{
    public partial class SettingsWindow : Window
    {
        public static SettingsWindow settingsInstance;

        public SettingsWindow()
        {
            InitializeComponent();
            settingsInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        public void MoveSettingsWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void SwitchToAccountKeyTab(string accountKeyArgument)
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

        public static string newAccountKey;
        private void SettingsWindowContentRendered(object sender, EventArgs e)
        {
            Top = (LauncherWindow.launcherInstance.Top + LauncherWindow.launcherInstance.Height / 2) - (Height / 2);

            if (!string.IsNullOrEmpty(newAccountKey))
                SettingsPages.AccountKey.accountKeyInstance.accountKeyBoxPassword.Password = newAccountKey;
        }

        public void CloseSettingsWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsLoaded(object sender, RoutedEventArgs e)
        {
            FocusEffects.BeginUnfocusAnimation(Owner);
        }

        private void SettingsWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible && SettingsPages.GameFiles.isExecutingTasks)
            {
                e.Cancel = true;
                CustomMessageBox.Show(FindResource("item73").ToString(), this);
                return;
            }

            settingsInstance = null;
            FocusEffects.BeginFocusAnimation(Owner);
        }
    }
}