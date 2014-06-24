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
    /// Interaction logic for Volume.xaml
    /// </summary>
    public partial class Volume : UserControl
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
            ProgressView.Width = w;
            Console.WriteLine(per);
            try
            {
                per *= 100;
                String res = "";
                if (per >= 0)
                {
                    res = "Resources/SpeakerMute.png";
                }
                if (per >= 10)
                {
                    res = "Resources/SpeakerL.png";
                }
                if (per >= 50)
                {
                    res = "Resources/SpeakerM.png";
                }
                if (per >= 75)
                {
                    res = "Resources/SpeakerH.png";
                }
                MainWindow.INSTANCE.Speaker.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "\\" + res));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Volume()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += Volume_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Volume_MouseLeftButtonUp;
            this.MouseMove += Volume_MouseMove;
            this.MouseLeave += Volume_MouseLeave;
        }

        void Volume_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        Boolean mouseDown = false;
        void Volume_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown) updateVisualAndSoundVol();
        }

        void Volume_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            updateVisualAndSoundVol();
        }

        void Volume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            updateVisualAndSoundVol();
            /*MainWindow.INSTANCE.player.Position = TimeSpan.FromSeconds(val);
            Value = val;*/
        }

        public void updateVisualAndSoundVol()
        {
            Point pos = Mouse.GetPosition(MainWindow.INSTANCE.VolumeControl);
            float val = (float)((pos.X / ActualWidth) * Maximum); // Percentage clicked
            //val = 100 - val;
            //MainWindow.INSTANCE.player.Volume = val / 100F;
            Value = val;
            Console.WriteLine("VOL: " + val);
            Grooveshark.SetBassAttrib(Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, val / 100F);
        }
    }
}
