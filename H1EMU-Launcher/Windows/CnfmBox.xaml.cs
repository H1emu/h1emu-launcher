using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for CnfmBox.xaml
    /// </summary>

    public partial class CnfmBox : Window
    {
        public CnfmBox()
        {
            InitializeComponent();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageFile.SetLanguageCode();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            this.Topmost = true;
            this.Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Topmost = true;
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainCnfmBoxActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}
