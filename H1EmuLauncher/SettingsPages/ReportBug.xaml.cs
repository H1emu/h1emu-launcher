using H1EmuLauncher.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H1EmuLauncher.SettingsPages
{
    /// <summary>
    /// Interaction logic for ReportBug.xaml
    /// </summary>
    public partial class ReportBug : Page
    {
        public static ReportBug reportBugInstance;

        public ReportBug()
        {
            InitializeComponent();
            reportBugInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void ReportBugGithubServer(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.SERVER_BUG_LINK,
                UseShellExecute = true
            });
        }

        private void ReportBugGithubLauncher(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Info.LAUNCHER_BUG_LINK,
                UseShellExecute = true
            });
        }
    }
}
