using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;

namespace H1EmuLauncher.Classes
{
    public static class CheckAccountKey
    {
        public static bool CheckAccountKeyValidity(string key)
        {
            try
            {
                HttpResponseMessage result = UpdateWindow.httpClient.GetAsync(new Uri($"{Info.ACCOUNT_KEY_CHECK_API}{key}")).Result;

                int statusCode = (int)result.StatusCode;

                switch (statusCode)
                {
                    case 200: // Valid key
                        return true;
                    case 500: // Invalid key
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item180")}".Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), LauncherWindow.launcherInstance);
                        }));

                        return false;
                    case 508: // Banned key
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item181")}", LauncherWindow.launcherInstance);
                        }));

                        return false;
                    default: // Other status code
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{LauncherWindow.launcherInstance.FindResource("item182")} '{statusCode}'.", LauncherWindow.launcherInstance);
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
