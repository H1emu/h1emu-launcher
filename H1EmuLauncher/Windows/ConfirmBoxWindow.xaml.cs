using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class ConfirmBoxWindow : Window
    {
        public ConfirmBoxWindow()
        {
            InitializeComponent();

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            Topmost = true;
            Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            Topmost = true;
            Close();
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            Topmost = true;
            Close();
        }

        private void ConfirmBoxMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MainCnfmBoxLoaded(object sender, RoutedEventArgs e)
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

        private void ConfirmBoxClosing(object sender, System.ComponentModel.CancelEventArgs e)
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
                Storyboard sb = FindResource("CloseConfirmBox") as Storyboard;

                if (sb != null)
                {
                    sb.Completed += (s, _) =>
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