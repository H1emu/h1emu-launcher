using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Language Controller
    /// Switch Language
    /// </summary>
    public static class LanCtrler
    {
        private static string m_CurrentLanguage = "en-US";
        //key1:LanguageType,key2:Code,key3:Words
        public static Dictionary<string, Dictionary<string, string>> LanguageCellsMap = new Dictionary<string, Dictionary<string, string>>();

        public static string CurrentLanguage { get => m_CurrentLanguage; set => m_CurrentLanguage = value; }

        public static void Init()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "zh-CN")
            { 
                m_CurrentLanguage = "zh-CN";
            }
            else
            {
                m_CurrentLanguage = "en-US";
            }
            Zh_cn zh_Cn = new Zh_cn();
            LanguageCellsMap = new Dictionary<string, Dictionary<string, string>>()
            {
                { "zh-CN",zh_Cn.LanMap},
            };
            #region LoadFile
            //string filePath = AppDomain.CurrentDomain.BaseDirectory + "//Settings//LanguageMap.txt";
            //using (StreamReader sr = new StreamReader(filePath))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        string[] strs = sr.ReadLine().Split('\t');
            //        if (LanguageCellsMap["EN"].ContainsKey(strs[0]))
            //        {
            //            LanguageCellsMap["EN"][strs[0]] = strs[1];
            //        }
            //        else
            //        {
            //            LanguageCellsMap["EN"].Add(strs[0], strs[1]);
            //        }
            //        if (LanguageCellsMap["CN"].ContainsKey(strs[0]))
            //        {
            //            LanguageCellsMap["CN"][strs[0]] = strs[2];
            //        }
            //        else
            //        {
            //            LanguageCellsMap["CN"].Add(strs[0], strs[2]);
            //        }
            //    }
            //}
            #endregion
        }
        public static string GetWords(string key)
        {
            if (m_CurrentLanguage == "zh-CN")
            {
                return LanguageCellsMap[m_CurrentLanguage][key];
            }
            else
            {
                return key;
            }
        }

        //public static void ChangeLanguage(Control control)
        //{
        //    if (m_CurrentLanguage == "EN")
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        foreach (Control ctrl in control.Controls)
        //        {
        //            ChangeLanguage(ctrl);
        //            control.Text = string.IsNullOrEmpty(LanCtler.GetWords(control.Name)) ? control.Text : LanCtler.GetWords(control.Name);
        //        }
        //    }
        //}
    }
}
