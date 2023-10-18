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

        private void CloseH1Z1Click(object sender, RoutedEventArgs e)
        {
            foreach (var h1z1Processes in Process.GetProcessesByName("h1z1"))
                h1z1Processes.Kill();

            Topmost = true;
            Close();
        }

        private void ConfirmYesButtonClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.Yes;
            Topmost = true;
            Close();
        }

        private void ConfirmNoButtonClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.No;
            Topmost = true;
            Close();
        }

        private void MoveMessageBoxWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MessageBoxLoaded(object sender, RoutedEventArgs e)
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

        private void MessageBoxContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseMessageBoxWindow(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.OK;
            Topmost = true;
            Close();
        }

        private void MessageBoxClosing(object sender, System.ComponentModel.CancelEventArgs e)
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
        }
    }
}