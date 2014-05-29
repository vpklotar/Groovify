using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;

namespace Launcher
{
    public class Theme : System.ComponentModel.INotifyPropertyChanged
    {
        public static String LOAD_THEME = "Default";
        public static Theme CurrentTheme;
        private Settings settings;
        private static SolidColorBrush _PlaylistHBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        public String DisplayName = "";
        public static SolidColorBrush PlaylistHBrush
        {
            get
            {
                return _PlaylistHBrush;
            }
            set
            {
                _PlaylistHBrush = value;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
        private static SolidColorBrush _SongHBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        public static SolidColorBrush SongHBrush
        {
            get
            {
                return _SongHBrush;
            }
            set
            {
                _SongHBrush = value;
                CurrentTheme.OnPropertyChanged("SongHBrush");
                CurrentTheme.OnPropertyChanged("SongItemBG");
                CurrentTheme.OnPropertyChanged("SongItemBGHex");
            }
        }
        private static SolidColorBrush _SongItemBG = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        public static SolidColorBrush SongItemBG
        {
            get
            {
                return _SongHBrush;
            }
            set
            {
                _SongHBrush = value;
                CurrentTheme.OnPropertyChanged("SongItemBG");
                CurrentTheme.OnPropertyChanged("SongItemBGHex");
            }
        }

        public static SolidColorBrush SongAlternationIndex
        {
            get
            {
                return CurrentTheme.GetBrush("SongAlternationBG");
            }
            set
            {
                _SongHBrush = value;
                CurrentTheme.OnPropertyChanged("SongAlternationIndex");
            }
        }

        public static Color SongItemBGC
        {
            get
            {
                return _SongHBrush.Color;
            }
        }
        public static String SongItemBGHex
        {
            get
            {
                Color c = SongItemBG.Color;
                String r = "#FF" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
                return r;
            }
            set
            {
                CurrentTheme.OnPropertyChanged("SongItemBG");
                CurrentTheme.OnPropertyChanged("SongItemBGHex");
            }
        }

        public static SolidColorBrush PlaylistAlternationIndex
        {
            get
            {
                return CurrentTheme.GetBrush("PlaylistAlternationIndex");
            }
        }

        public Theme()
        {
            if (CurrentTheme == null) CurrentTheme = this;
            this.DisplayName = "Default";
        }

        public Theme(String Name)
        {
            settings = new Settings("Themes\\" + Name + ".the");
            this.DisplayName = Name;
        }

        public SolidColorBrush GetBrush(String key)
        {
            if (settings != null)
            {
                String s = settings.GetString(key);
                var c = System.Drawing.ColorTranslator.FromHtml(s);
                return new SolidColorBrush(Color.FromArgb(255, c.R, c.G, c.B));
                /*SolidColorBrush c = new SolidColorBrush(Color.FromScRgb(float.Parse(parts[0]) / 255F, float.Parse(parts[1]) / 255F, float.Parse(parts[2]) / 255F, float.Parse(parts[3]) / 255F));
                return c;*/
            }
            else
            {
                settings = new Settings("Themes\\" + LOAD_THEME + ".the");
                return GetBrush(key);
            }
        }

        public static Theme GetTheme(String name)
        {
            for (int i = 0; i < Themes.Count; i++)
            {
                if (Themes[i].DisplayName == name) return Themes[i];
            }
            return null;
        }

        public void SaveTheme()
        {
            if (settings != null) settings.Save();
        }

        public static List<Theme> Themes = new List<Theme>();

        private static Boolean HasThemeBeenAdded(String name)
        {
            for (int i = 0; i < Themes.Count; i++)
            {
                if (Themes[i].DisplayName == name) return true;
            }
            return false;
        }

        public static void ScanForThemes()
        {
            var di = new DirectoryInfo("Themes");
            if (!Theme.HasThemeBeenAdded("Default")) Themes.Add(new Theme("Default"));
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Extension.ToLower() == ".the")
                {
                    String name = fi.Name;
                    name = name.Substring(0, name.Length - 4);
                    if(!Theme.HasThemeBeenAdded(name)) Themes.Add(new Theme(name));
                }
            }
        }

        public static void Apply()
        {
            Theme t = new Theme(LOAD_THEME);
            CurrentTheme = t;

            MainWindow.INSTANCE.TopPanel.Background = t.GetBrush("BGTopPanel");
            MainWindow.INSTANCE.CurrentlyPlayingLabel.Foreground = t.GetBrush("FGCurrentlyPlayingLabel");
            MainWindow.INSTANCE.TimeSpanView.Foreground = t.GetBrush("FGTimeSpanView");
            MainWindow.INSTANCE.SearchBox.Foreground = t.GetBrush("FGSearchBox");

            MainWindow.INSTANCE.NewPlaylistHolder.Background = t.GetBrush("BGPlaylistView");
            MainWindow.INSTANCE.Playlists.Background = t.GetBrush("BGPlaylistView");
            MainWindow.INSTANCE.Playlists.Foreground = t.GetBrush("FGPlaylistView");

            MainWindow.INSTANCE.NewPlaylistBox.Foreground = t.GetBrush("FGNewPlaylistBox");

            MainWindow.INSTANCE.Songs.Background = t.GetBrush("BGSongsView");
            MainWindow.INSTANCE.Songs.Foreground = t.GetBrush("FGSongsView");
            MainWindow.INSTANCE.Songs.BorderBrush = t.GetBrush("SongsViewAlternationIndex");

            Theme._PlaylistHBrush = t.GetBrush("PlaylistHBrush");
            Theme._SongHBrush = t.GetBrush("SongHBrush");

            MainWindow.INSTANCE.Songs.InvalidateVisual();
            MainWindow.INSTANCE.UpdateLayout();

            MainWindow.INSTANCE.InvalidateVisual();

            MainWindow.INSTANCE.Songs.Items.Clear();
            if (Grooveshark.Playlists.Count > 0)
            {
                var p = Grooveshark.Playlists[0];
                for (int i = 0; i < p.getSongs().Count; i++)
                {
                    MainWindow.INSTANCE.Songs.Items.Add(p.getSongs()[i]);
                }
            }
        }

        public Dictionary<String, String> GetKeyValPairs()
        {
            return settings.GetKeyValPairs();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
