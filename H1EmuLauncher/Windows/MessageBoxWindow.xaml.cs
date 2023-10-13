using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void CloseH1Z1(object sender, RoutedEventArgs e)
        {
            foreach (var h1z1Processes in Process.GetProcessesByName("h1z1"))
                h1z1Processes.Kill();

            Topmost = true;
            Close();
        }

        private void CloseMessageBox(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void OkButton(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void MoveMessageBox(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainMsgBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                switch (Owner.Name)
                {
                    case "AddServer":
                        AddServerWindow.addServerInstance.UnfocusPropertiesAnimationShow.Begin();
                        break;
                    case "Launcher":
                        LauncherWindow.launcherInstance.UnfocusPropertiesAnimationShow.Begin();
                        break;
                    case "Settings":
                        SettingsWindow.settingsInstance.UnfocusPropertiesAnimationShow.Begin();
                        break;
                }
            }
        }

        public bool IsCompleted = false;

        private void MessageBoxClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                if (Owner != null)
                {
                    switch (Owner.Name)
                    {
                        case "AddServer":
                            AddServerWindow.addServerInstance.UnfocusPropertiesAnimationHide.Begin();
                            break;
                        case "Launcher":
                            LauncherWindow.launcherInstance.UnfocusPropertiesAnimationHide.Begin();
                            break;
                        case "Settings":
                            SettingsWindow.settingsInstance.UnfocusPropertiesAnimationHide.Begin();
                            break;
                    }
                }

                e.Cancel = true;
                Storyboard sb = FindResource("CloseMessageBox") as Storyboard;

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