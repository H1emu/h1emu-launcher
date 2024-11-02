using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    public static class CheckAccountKey
    {
        public static async Task<bool> CheckAccountKeyValidity(string key)
        {
            try
            {
                HttpResponseMessage result = await UpdateWindow.httpClient.GetAsync($"{Info.ACCOUNT_KEY_CHECK_API}{key}");
                switch ((int)result.StatusCode)
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
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item182")} '{(int)result.StatusCode}'.", LauncherWindow.launcherInstance);
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
    }
}
