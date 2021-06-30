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

namespace H1EMU_Redux
{
    /// <summary>
    /// Interaction logic for CnfmBox.xaml
    /// </summary>
    public partial class CnfmBox : Window
    {
        public CnfmBox()
        {
            InitializeComponent();

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(20d);
            fadeAnimation.From = 0.0d;
            fadeAnimation.To = 1.0d;
            MainCnfmBox.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainCnfmBox.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainCnfmBox.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            CustomMessageBox.result = false ;
            this.Close();
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = true;
            this.Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.result = false;
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
