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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        private float _max = 100;
        public float Maximum
        {
            get
            {
                return _max;
            }
            set
            {
                _max = value;
                updateView();
            }
        }
        private float _value = 0;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                updateView();
            }
        }

        private void updateView()
        {
            float per = Value / Maximum;
            int w = (int)(per * ActualWidth);
            System.Windows.Media.Animation.DoubleAnimation da = new System.Windows.Media.Animation.DoubleAnimation();
            da.From = ProgressView.ActualWidth;
            da.To = w;
            da.Duration = TimeSpan.FromMilliseconds(500);
            ProgressView.BeginAnimation(Label.WidthProperty, da);
        }

        public ProgressControl()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = Mouse.GetPosition(MainWindow.INSTANCE.progress);
            int val = (int)((pos.X / ActualWidth) * Maximum); // Percentage clicked
            MainWindow.INSTANCE.player.Position = TimeSpan.FromSeconds(val);
            Value = val;
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
