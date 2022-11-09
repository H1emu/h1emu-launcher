using System;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    public static class SetLanguageFile
    {
        public static ResourceDictionary LoadFile()
        {
            ResourceDictionary dict = new();

            dict.Source = Properties.Settings.Default.language switch
            {
                0 => new Uri("Language/StringResources.en.xaml", UriKind.RelativeOrAbsolute),
                1 => new Uri("Language/StringResources.zh-CN.xaml", UriKind.RelativeOrAbsolute),
                2 => new Uri("Language/StringResources.hr.xaml", UriKind.RelativeOrAbsolute),
                3 => new Uri("Language/StringResources.fr.xaml", UriKind.RelativeOrAbsolute),
                4 => new Uri("Language/StringResources.tr-TR.xaml", UriKind.RelativeOrAbsolute),
                5 => new Uri("Language/StringResources.pl-PL.xaml", UriKind.RelativeOrAbsolute),
                6 => new Uri("Language/StringResources.ru-RU.xaml", UriKind.RelativeOrAbsolute),
                7 => new Uri("Language/StringResources.nl-NL.xaml", UriKind.RelativeOrAbsolute),
                8 => new Uri("Language/StringResources.bg-BG.xaml", UriKind.RelativeOrAbsolute),
                9 => new Uri("Language/StringResources.de-DE.xaml", UriKind.RelativeOrAbsolute),
                10 => new Uri("Language/StringResources.pt-PT.xaml", UriKind.RelativeOrAbsolute),
                11 => new Uri("Language/StringResources.es-ES.xaml", UriKind.RelativeOrAbsolute),
                _ => new Uri("Language/StringResources.en.xaml", UriKind.RelativeOrAbsolute),
            };

            return dict;
        }

        public static void SaveLang(int index)
        {
            Properties.Settings.Default.language = index;
            Properties.Settings.Default.Save();

            Launcher.launcherInstance.Resources.MergedDictionaries.Clear();
            Launcher.launcherInstance.Resources.MergedDictionaries.Add(LoadFile());
        }
    }
}