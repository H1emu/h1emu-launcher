using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace H1EMU_Launcher
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        public AddServer()
        {
            InitializeComponent();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageFile.SetLanguageCode();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void ServerIpBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CustomMessageBox.result = true;
                this.Close();
            }
        }

        private void AddServerTopBar(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            this.Topmost = true;
            this.Close();
        }

        private void CloseAddServerMenu(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void ServerNameGotFocus(object sender, RoutedEventArgs e)
        {
            serverNameHint.Visibility = Visibility.Hidden;
        }

        private void ServerNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverNameBox.Text)) { serverNameHint.Visibility = Visibility.Visible; }
        }

        private void ServerIPGotFocus(object sender, RoutedEventArgs e)
        {
            serverIpHint.Visibility = Visibility.Hidden;
        }

        private void ServerIPLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverIpBox.Text)) { serverIpHint.Visibility = Visibility.Visible; }
        }
    }
}
