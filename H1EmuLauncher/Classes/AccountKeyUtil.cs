using System;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System.Security.Cryptography;

namespace H1EmuLauncher.Classes
{
    internal class AccountKeyUtil
    {
        public static string GenerateNewAccountKey()
        {
            string generatedKey = string.Empty;
            Random random = new();

            for (int i = 0; i < 64; i++)
                generatedKey += Info.ALLOWED_ACCOUNT_KEY_CHARS[random.Next(Info.ALLOWED_ACCOUNT_KEY_CHARS.Length)];

            return generatedKey;
        }

        public static async Task<bool> CheckAccountKeyValidity(string key)
        {
            try
            {
                HttpResponseMessage response = await SplashWindow.httpClient.GetAsync($"{Info.ACCOUNT_KEY_CHECK_API}{HttpUtility.UrlEncode(key)}");
                switch ((int)response.StatusCode)
                {
                    case 200: // Valid key
                        return true;

                    case 401: // Unverified key
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item180")}".Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), LauncherWindow.launcherInstance, false, false, false, true, true);
                        }));
                        return false;

                    default: // Other status code
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item182")} '{(int)response.StatusCode}'.", LauncherWindow.launcherInstance);
                        }));
                        return false;
                }
            }
            catch (Exception e) // Other exception was thrown
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item183")} \"{e.Message}\"", LauncherWindow.launcherInstance);
                }));
                return false;
            }
        }

        public static string EncryptStringSHA256(string s)
        {
            string sb = string.Empty;
            Encoding enc = Encoding.UTF8;
            byte[] result = SHA256.HashData(enc.GetBytes(s));

            foreach (byte b in result)
                sb += b.ToString("x2");

            return sb;
        }
    }
}
