using System;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    public static class SetLanguageFile
    {
        public static ResourceDictionary LoadFile()
        {
            ResourceDictionary dict = new();

            switch (Properties.Settings.Default.language)
            {
                case 0:
                    dict.Source = new Uri("Language/StringResources.en-EN.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 1:
                    dict.Source = new Uri("Language/StringResources.zh-CN.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 2:
                    dict.Source = new Uri("Language/StringResources.hr-HR.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 3:
                    dict.Source = new Uri("Language/StringResources.fr-FR.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 4:
                    dict.Source = new Uri("Language/StringResources.tr-TR.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 5:
                    dict.Source = new Uri("Language/StringResources.pl-PL.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 6:
                    dict.Source = new Uri("Language/StringResources.ru-RU.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 7:
                    dict.Source = new Uri("Language/StringResources.nl-NL.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 8:
                    dict.Source = new Uri("Language/StringResources.bg-BG.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 9:
                    dict.Source = new Uri("Language/StringResources.de-DE.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 10:
                    dict.Source = new Uri("Language/StringResources.pt-PT.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 11:
                    dict.Source = new Uri("Language/StringResources.es-ES.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 12:
                    dict.Source = new Uri("Language/StringResources.se-SE.xaml", UriKind.RelativeOrAbsolute);
                    break;
                default:
                    dict.Source = new Uri("Language/StringResources.en-EN.xaml", UriKind.RelativeOrAbsolute);
                    break;
            }

            return dict;
        }

        public static void SaveLang(int index)
        {
            Properties.Settings.Default.language = index;
            Properties.Settings.Default.Save();

            LauncherWindow.launcherInstance.Resources.MergedDictionaries.Clear();
            LauncherWindow.launcherInstance.Resources.MergedDictionaries.Add(LoadFile());
        }
    }
}