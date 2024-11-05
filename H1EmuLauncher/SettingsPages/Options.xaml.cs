using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher.SettingsPages
{
    public partial class Options : Page
    {
        public static Options optionsInstance;

        public Options()
        {
            InitializeComponent();
            optionsInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void OptionsLoaded(object sender, RoutedEventArgs e)
        {
            languageBox.SelectedIndex = Properties.Settings.Default.language;

            if (Properties.Settings.Default.imageCarouselVisibility)
                imageCarouselVisibilityToggleButton.IsChecked = true;

            if (Properties.Settings.Default.autoMinimise)
                autoMinimiseToggleButton.IsChecked = true;

            if (Properties.Settings.Default.developerMode)
                developerModeToggleButton.IsChecked = true;

            currentVersionNumber.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.')}";
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!languageBox.IsLoaded)
                return;

            try
            {
                switch (languageBox.SelectedIndex)
                {
                    // Update and save settings
                    case 0:
                        SetLanguageFile.SaveLang(0);
                        break;
                    case 1:
                        SetLanguageFile.SaveLang(1);
                        break;
                    case 2:
                        SetLanguageFile.SaveLang(2);
                        break;
                    case 3:
                        SetLanguageFile.SaveLang(3);
                        break;
                    case 4:
                        SetLanguageFile.SaveLang(4);
                        break;
                    case 5:
                        SetLanguageFile.SaveLang(5);
                        break;
                    case 6:
                        SetLanguageFile.SaveLang(6);
                        break;
                    case 7:
                        SetLanguageFile.SaveLang(7);
                        break;
                    case 8:
                        SetLanguageFile.SaveLang(8);
                        break;
                    case 9:
                        SetLanguageFile.SaveLang(9);
                        break;
                    case 10:
                        SetLanguageFile.SaveLang(10);
                        break;
                    case 11:
                        SetLanguageFile.SaveLang(11);
                        break;
                    case 12:
                        SetLanguageFile.SaveLang(12);
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{FindResource("item142")} \"{ex.Message}\"", SettingsWindow.settingsInstance);
                return;
            }

            if (languageBox.SelectedIndex == 1)
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Visible;
            else
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Collapsed;

            // Reload pages
            LauncherWindow.launcherInstance.steamFramePanel.Refresh();
        }

        private void ImageCarouselVisibilityToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (imageCarouselVisibilityToggleButton.IsChecked == true)
            {
                // Show image carousel
                Carousel.playCarousel.Begin();
                LauncherWindow.launcherInstance.imageCarousel.Visibility = Visibility.Visible;

                Properties.Settings.Default.imageCarouselVisibility = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                // Hide image carousel
                Carousel.playCarousel.Stop();
                LauncherWindow.launcherInstance.imageCarousel.Visibility = Visibility.Hidden;

                Properties.Settings.Default.imageCarouselVisibility = false;
                Properties.Settings.Default.Save();
            }
        }

        private void AutoMinimiseToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (autoMinimiseToggleButton.IsChecked == true)
            {
                Properties.Settings.Default.autoMinimise = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.autoMinimise = false;
                Properties.Settings.Default.Save();
            }
        }

        private void DeveloperModeToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (developerModeToggleButton.IsChecked == true)
            {
                Properties.Settings.Default.developerMode = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.developerMode = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
