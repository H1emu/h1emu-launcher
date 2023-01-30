using System;
using System.Net;
using System.Windows;

#pragma warning disable SYSLIB0014 // Type or member is obsolete (WebClient)

namespace H1EmuLauncher.Classes
{
    public static class CheckAccountKey
    {
        public static bool CheckAccountKeyValidity(string key)
        {
            WebClient wc = new();
            wc.Headers.Add("User-Agent", "d-fens HttpClient");

            try
            {
                // If no exception is thrown then key is valid
                string check = wc.DownloadString(new Uri($"https://www.h1emu.com/us/thermos/keyvalidator/?key={key}"));
                return true;
            }

            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                int code = (int)response.StatusCode;

                switch (code)
                {
                    case 508: // Banned key
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{Application.Current.FindResource("item180")}", LauncherWindow.launcherInstance);
                        }));

                        break;
                    case 500: // Invalid key
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            CustomMessageBox.Show($"{Application.Current.FindResource("item181")}".Replace("\\n\\n", $"{Environment.NewLine}{Environment.NewLine}"), LauncherWindow.launcherInstance);
                        }));

                        break;
                }
            }
            catch (Exception ex) // Other exception was thrown
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    CustomMessageBox.Show($"{Application.Current.FindResource("item182")} \"{ex.Message}\"", LauncherWindow.launcherInstance);
                }));
            }

            return false;
        }
    }
}
