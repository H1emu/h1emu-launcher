using H1EmuLauncher.Classes;
using H1EmuLauncher;
using System;
using System.Threading.Tasks;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// This is a default implementation of <see cref="IAuthenticator"/> to ease of use.
    ///
    /// This implementation will prompt user to enter 2-factor authentication codes in the H1EmuLauncher app.
    /// </summary>
    public class UserAuthenticator : IAuthenticator
    {
        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync( bool previousCodeWasIncorrect )
        {
            if (previousCodeWasIncorrect)
            {
                CustomMessageBox.Show("\"The previous 2-factor auth code you provided is incorrect.\"", LauncherWindow.launcherInstance);
                Console.Error.WriteLine("The previous 2-factor auth code you have provided is incorrect.");
            }

            Console.Error.Write("STEAM GUARD! Please enter your 2-factor auth code from your authenticator app: ");

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\2FA.xaml", UriKind.Relative));
                H1EmuLauncher.SteamFramePages._2FA.twoFacInstruction = 1;
            }));

            while (string.IsNullOrEmpty(H1EmuLauncher.SteamFramePages._2FA.code))
            {
                if (H1EmuLauncher.SteamFramePages._2FA.code != null)
                {
                    break;
                }
            }

            return Task.FromResult(H1EmuLauncher.SteamFramePages._2FA.code!);
        }

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync( string email, bool previousCodeWasIncorrect )
        {
            if (previousCodeWasIncorrect)
            {
                Console.Error.WriteLine("The previous 2-factor auth code you provided is incorrect.");
            }

            Console.Error.Write($"STEAM GUARD! Please enter the auth code sent to the email at {email}: ");

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\2FA.xaml", UriKind.Relative));
                H1EmuLauncher.SteamFramePages._2FA.twoFacInstruction = 2;
            }));

            while (string.IsNullOrEmpty(H1EmuLauncher.SteamFramePages._2FA.code))
            {
                if (H1EmuLauncher.SteamFramePages._2FA.code != null)
                {
                    break;
                }
            }

            return Task.FromResult(H1EmuLauncher.SteamFramePages._2FA.code!);
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            Console.Error.WriteLine( "STEAM GUARD! Use the Steam Mobile App to confirm your sign in..." );

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                LauncherWindow.launcherInstance.steamFramePanel.Navigate(new Uri("..\\SteamFramePages\\2FA.xaml", UriKind.Relative));
                H1EmuLauncher.SteamFramePages._2FA.twoFacInstruction = 3;
            }));

            return Task.FromResult( true );
        }
    }
}
