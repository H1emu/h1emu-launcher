using H1EMU_Launcher.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for Splash.xaml
    /// </summary>

    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();

            //Set just language code ex: en-us, fr-ca from the settings
            SetLanguageFile.SetLanguageCode();

            //Adds the correct language file to the resource dictionary and then load it.
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void SplashScreenMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SplashScreenActivated(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}
