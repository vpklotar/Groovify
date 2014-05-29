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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for SelectedTheme.xaml
    /// </summary>
    public partial class SelectedTheme : Window
    {
        public SelectedTheme()
        {
            InitializeComponent();
            Theme.ScanForThemes();
            for (int i = 0; i < Theme.Themes.Count; i++)
            {
                Themes.Items.Add(Theme.Themes[i]);
            }
            Themes.SelectedIndex = 0;
            Themes.SelectedItem = Theme.Themes[0];
            Themes.SelectionChanged += Themes_SelectionChanged;
            Themes_SelectionChanged(null, null);
        }

        public Theme CurrentTheme
        {
            get
            {
                return Theme.GetTheme(((Theme)Themes.SelectedItem).DisplayName);
            }
        }

        void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeValues.Items.Clear();
            for (int i = 0; i < CurrentTheme.GetKeyValPairs().Count; i++)
            {
                var key = CurrentTheme.GetKeyValPairs().Keys.ToArray()[i];
                var val = CurrentTheme.GetKeyValPairs().Values.ToArray()[i];
                ThemeValues.Items.Add(new ListViewItem() {Content = key + "::" + val});
            }
        }

        private void ThemeValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String currentItem = ThemeValues.SelectedItem.ToString();
            String[] split = currentItem.Split(new String[] { "::" }, StringSplitOptions.None);
            ChangeBox.Text = split[1];
        }

        private void ChangeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                String currentItem = ThemeValues.SelectedItem.ToString();
                String[] split = currentItem.Split(new String[] { "::" }, StringSplitOptions.None);
                ((ListViewItem)ThemeValues.Items[ThemeValues.SelectedIndex]).Content = split[0] + "::" + ChangeBox.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
