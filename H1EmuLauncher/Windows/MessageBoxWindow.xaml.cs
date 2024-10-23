using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class MessageBoxWindow : Window
    {
        public static MessageBoxWindow messageBoxInstance;

        public MessageBoxWindow()
        {
            InitializeComponent();
            messageBoxInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private async void CloseH1Z1Click(object sender, RoutedEventArgs e)
        {
            killH1Z1Button.IsEnabled = false;

            foreach (Process h1z1Processes in Process.GetProcessesByName("h1z1"))
                h1z1Processes.Kill(true);

            await Task.Run(() => {
                WaitForGameClosure:
                    if (Process.GetProcessesByName("h1z1").Length > 0)
                        goto WaitForGameClosure;
            });

            // Check the game version again once the binary is free
            LauncherWindow.launcherInstance.CheckGameVersionAndPath(LauncherWindow.launcherInstance);

            killH1Z1Button.IsEnabled = true;
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

        private void MessageBoxContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseMessageBoxWindow(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void MessageBoxWindowLoaded(object sender, RoutedEventArgs e)
        {
            FocusEffects.BeginUnfocusAnimation(Owner);
        }

        private void MessageBoxWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FocusEffects.BeginFocusAnimation(Owner);
        }
    }
}