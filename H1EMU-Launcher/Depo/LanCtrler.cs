using System;
using System.Collections.Generic;
using System.IO;
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
            try
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
                LoadLanguageFile();
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }
        private static void LoadLanguageFile()
        { 
         string filepath = AppDomain.CurrentDomain.BaseDirectory + "//Languages";
            if (Directory.Exists(filepath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(filepath);
               FileInfo[] fileInfos= directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                {
                    if (fileInfo.FullName.Contains(".ini"))
                    {
                        string lanName = fileInfo.Name.Replace(".ini","");
                        using (StreamReader sr = new StreamReader(filepath))
                        {
                            while (!sr.EndOfStream)
                            {
                                string[] strs = sr.ReadLine().Split('|');//Splite By "|",The Format about load file at the end of this class file
                                if (LanguageCellsMap[lanName].ContainsKey(strs[0]))
                                {
                                    LanguageCellsMap[lanName][strs[0]] = strs[1];
                                }
                                else
                                {
                                    LanguageCellsMap[lanName].Add(strs[0], strs[1]);
                                }
                            }
                        }
                    }                
                }
            }
            
        
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

    }
    ///Load File Format 
    ///Split By "|"
    ///the FileName.Replace(".ini","") is the target language type,such as "zh-cn.ini" is Chinese,it is also as same as OS-Language type
    ///string[] words=StreamReader.ReadLine();
    ///words[0] is English-Word,it is the key in  LanguageCellsMap<string,string>
    ///words[0] is Translate-Word,it is the value in  LanguageCellsMap<string,string>
    #region 
    //"A new update is available, would you like to update now?|有新的更新可用，是否立即更新?"        
    #endregion
}
