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
    /// Interaction logic for AboutPage.xaml
    /// </summary>

    public partial class AboutPage : Window
    {
        public AboutPage()
        {
            InitializeComponent();

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 0.0d;
            fadeAnimation.To = 1.0d;
            MainAbout.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void MainAbout_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(100d);
            fadeAnimation.From = 1.0d;
            fadeAnimation.To = 0.0d;
            MainAbout.BeginAnimation(OpacityProperty, fadeAnimation);

            while (MainAbout.Opacity != 0) { System.Windows.Forms.Application.DoEvents(); }

            e.Cancel = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
