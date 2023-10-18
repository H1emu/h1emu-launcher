

using H1EmuLauncher.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;

namespace H1EmuLauncher.SettingsPages
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
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
            LanguageBox.SelectedIndex = Properties.Settings.Default.language;

            if (LanguageBox.SelectedIndex == 1)
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Visible;
            else
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Collapsed;

            if (Properties.Settings.Default.imageCarouselVisibility)
                imageCarouselToggleButton.IsChecked = true;

            if (Properties.Settings.Default.autoMinimise)
                autoMinimiseToggleButton.IsChecked = true;

            currentVersionNumber.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.')}";
        }

        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (LanguageBox.SelectedIndex == 1)
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Visible;
            else
                LauncherWindow.launcherInstance.chineseLink.Visibility = Visibility.Collapsed;

            try
            {
                switch (LanguageBox.SelectedIndex)
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

            // Reload pages
            LauncherWindow.launcherInstance.steamFramePanel.Refresh();
        }

        private void ImageCarouselToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (imageCarouselToggleButton.IsChecked == true)
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
    }
}
