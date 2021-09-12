using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace H1EMU_Launcher.Resources
{
    public static class SetLanguageFile
    {
        public static ResourceDictionary LoadFile()
        {
            ResourceDictionary dict = new ResourceDictionary();

            switch (Properties.Settings.Default.language)
            {
                case 0:
                    dict.Source = new Uri("Language/StringResources.en.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 1:
                    dict.Source = new Uri("Language/StringResources.zh-CN.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 2:
                    dict.Source = new Uri("Language/StringResources.hr.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case 3:
                    dict.Source = new Uri("Language/StringResources.fr.xaml", UriKind.RelativeOrAbsolute);
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
                default:
                    dict.Source = new Uri("Language/StringResources.en.xaml", UriKind.RelativeOrAbsolute);
                    break;
            }

            return dict;
        }

        public static void SaveLang(int index)
        {
            Properties.Settings.Default.language = index;
            Properties.Settings.Default.Save();

            Launcher.lncher.Resources.MergedDictionaries.Clear();
            Launcher.lncher.Resources.MergedDictionaries.Add(LoadFile());
        }
    }
}
