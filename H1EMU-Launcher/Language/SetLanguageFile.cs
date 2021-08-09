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
        public static CultureInfo ci { get; set; }

        public static ResourceDictionary LoadFile()
        {
            ResourceDictionary dict = new ResourceDictionary();

            switch (ci.Name.ToLower())
            {
                case "en-us":
                    dict.Source = new Uri("Language/StringResources.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case "zh-cn":
                    dict.Source = new Uri("Language/StringResources.zh-cn.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case "hr":
                    dict.Source = new Uri("Language/StringResources.hr.xaml", UriKind.RelativeOrAbsolute);
                    break;
                case "fr":
                    dict.Source = new Uri("Language/StringResources.fr.xaml", UriKind.RelativeOrAbsolute);
                    break;
                default:
                    dict.Source = new Uri("Language/StringResources.xaml", UriKind.RelativeOrAbsolute);
                    break;
            }

            return dict;
        }


        public static void SetLanguageCode()
        {
            switch (Properties.Settings.Default.language)
            {
                case 0:
                    ci = new CultureInfo("en-us", false);
                    break;
                case 1:
                    ci = new CultureInfo("zh-cn", false);
                    break;
                case 2:
                    ci = new CultureInfo("hr", false);
                    break;
                case 3:
                    ci = new CultureInfo("fr", false);
                    break;
                default:
                    ci = new CultureInfo("en-us", false);
                    break;
            }

            Properties.Settings.Default.Save();
        }

        public static void SaveLang(int index)
        {
            Properties.Settings.Default.language = index;
            Properties.Settings.Default.Save();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageCode();

            Launcher.lncher.Resources.MergedDictionaries.Clear();
            Launcher.lncher.Resources.MergedDictionaries.Add(LoadFile());
        }
    }
}
