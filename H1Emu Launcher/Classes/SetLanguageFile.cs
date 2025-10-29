using System;
using System.Windows;
using H1Emu_Launcher.SettingsPages;

namespace H1Emu_Launcher.Classes
{
    public static class SetLanguageFile
    {
        public static ResourceDictionary LoadFile()
        {
            ResourceDictionary dict = new();

            switch (Properties.Settings.Default.language)
            {
                case 0:
                    dict.Source = new Uri("Language/StringResources.en-EN.xaml", UriKind.Relative);
                    break;
                case 1:
                    dict.Source = new Uri("Language/StringResources.zh-CN.xaml", UriKind.Relative);
                    break;
                case 2:
                    dict.Source = new Uri("Language/StringResources.hr-HR.xaml", UriKind.Relative);
                    break;
                case 3:
                    dict.Source = new Uri("Language/StringResources.fr-FR.xaml", UriKind.Relative);
                    break;
                case 4:
                    dict.Source = new Uri("Language/StringResources.tr-TR.xaml", UriKind.Relative);
                    break;
                case 5:
                    dict.Source = new Uri("Language/StringResources.pl-PL.xaml", UriKind.Relative);
                    break;
                case 6:
                    dict.Source = new Uri("Language/StringResources.ru-RU.xaml", UriKind.Relative);
                    break;
                case 7:
                    dict.Source = new Uri("Language/StringResources.nl-NL.xaml", UriKind.Relative);
                    break;
                case 8:
                    dict.Source = new Uri("Language/StringResources.bg-BG.xaml", UriKind.Relative);
                    break;
                case 9:
                    dict.Source = new Uri("Language/StringResources.de-DE.xaml", UriKind.Relative);
                    break;
                case 10:
                    dict.Source = new Uri("Language/StringResources.pt-PT.xaml", UriKind.Relative);
                    break;
                case 11:
                    dict.Source = new Uri("Language/StringResources.es-ES.xaml", UriKind.Relative);
                    break;
                case 12:
                    dict.Source = new Uri("Language/StringResources.se-SE.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Language/StringResources.en-EN.xaml", UriKind.Relative);
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

            SettingsWindow.settingsInstance.Resources.MergedDictionaries.Clear();
            SettingsWindow.settingsInstance.Resources.MergedDictionaries.Add(LoadFile());

            GameFiles.gameFilesInstance.Resources.MergedDictionaries.Clear();
            GameFiles.gameFilesInstance.Resources.MergedDictionaries.Add(LoadFile());

            AccountKey.accountKeyInstance.Resources.MergedDictionaries.Clear();
            AccountKey.accountKeyInstance.Resources.MergedDictionaries.Add(LoadFile());

            Options.optionsInstance.Resources.MergedDictionaries.Clear();
            Options.optionsInstance.Resources.MergedDictionaries.Add(LoadFile());

            ReportBug.reportBugInstance.Resources.MergedDictionaries.Clear();
            ReportBug.reportBugInstance.Resources.MergedDictionaries.Add(LoadFile());

            About.aboutInstance.Resources.MergedDictionaries.Clear();
            About.aboutInstance.Resources.MergedDictionaries.Add(LoadFile());

            Application.Current.Resources.MergedDictionaries.Clear();
        }
    }
}