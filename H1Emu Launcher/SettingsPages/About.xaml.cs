using System.Windows.Controls;
using H1Emu_Launcher.Classes;

namespace H1Emu_Launcher.SettingsPages
{
    public partial class About : Page
    {
        public static About aboutInstance;

        public About()
        {
            InitializeComponent();
            aboutInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }
    }
}
