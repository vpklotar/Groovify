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
using SciLorsGroovesharkAPI.Groove;
using System.Windows.Controls.Primitives;

using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Net;
using System.IO;
using Un4seen.Bass;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow INSTANCE;
        public List<GroovesharkSongObject> playedSongs = new List<GroovesharkSongObject>();
        public int playedSongsIndex = 0;
        public int playingIndex = 0;
        public static String state = "Stopped";
        public static bool repeat = false, shuffle = false;
        public static GroovesharkPlaylistObject currentPlaylist = null;
        public static GroovesharkPlaylistObject searchList = null;
        private TimeSpan mediaLength = TimeSpan.FromSeconds(0);

        private SolidColorBrush _SongSelectedHex = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public SolidColorBrush SongSelectedHex
        {
            get
            {
                return _SongSelectedHex;
            }
            set
            {
                _SongSelectedHex = value;
            }
        }

        public MainWindow()
        {
            //System.Windows.MessageBox.Show("WIN TEST 1");
            InitializeComponent();
            INSTANCE = this;
            this.Loaded += MainWindow_Loaded;
            //this.Closed += MainWindow_Closed;
            this.SizeChanged += MainWindow_SizeChanged;
            this.Songs.MouseDoubleClick += Songs_MouseDoubleClick;
            //System.Windows.MessageBox.Show("WIN TEST 2");
            Grooveshark.GetAllLocalyCachedPlaylists();
            //System.Windows.MessageBox.Show("WIN TEST 3");

            //bool res = User32.RegisterHotKey(new WindowInteropHelper(this).Handle, this.PersistId, 0x0000, (int)System.Windows.Forms.Keys.MediaPlayPause);

            this.MouseDown += MainWindow_MouseDown;
            this.SourceInitialized += OnSourceInitialized;
            this.KeyDown += MainWindow_KeyDown;
            //System.Windows.MessageBox.Show("WIN TEST 4");
            DownloadPool.DownloadComplete += DownloadPool_DownloadComplete;
            DownloadPool.ProgressChanged += DownloadPool_ProgressChanged;

            ContextMenu songsContext = new System.Windows.Controls.ContextMenu();
            MenuItem GetGroovifyURL = new MenuItem();
            GetGroovifyURL.Header = "Get groovify URL";
            GetGroovifyURL.Click += GetGroovifyURL_Click;
            songsContext.Items.Add(GetGroovifyURL);

            MenuItem Download = new MenuItem();
            Download.Header = "Download";
            Download.Click += Download_Click;
            songsContext.Items.Add(Download);

            Songs.ContextMenu = songsContext;

            ContextMenu playlistContext = new System.Windows.Controls.ContextMenu();

            MenuItem pDownload = new MenuItem();
            pDownload.Header = "Download";
            pDownload.Click += pDownload_Click;
            playlistContext.Items.Add(pDownload);
            Playlists.ContextMenu = playlistContext;
            Playlists.AllowDrop = true;

            ContextMenu TopPanelContext = new System.Windows.Controls.ContextMenu();
            MenuItem Theme = new MenuItem();
            Theme.Header = "Select Theme";
            Theme.Click += Theme_Click;
            TopPanelContext.Items.Add(Theme);
            TopPanel.ContextMenu = TopPanelContext;
            //System.Windows.MessageBox.Show("WIN TEST 5");

            System.Threading.Timer t = new System.Threading.Timer(updatePos);
            t.Change(0, 1000);
            //System.Windows.MessageBox.Show("WIN TEST 6");
            //Launcher.Theme.Apply();
            //System.Windows.MessageBox.Show("WIN TEST 7");
        }

        void Theme_Click(object sender, RoutedEventArgs e)
        {
            new SelectedTheme().Show();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Grooveshark.SavePlaylists();
        }

        void pDownload_Click(object sender, RoutedEventArgs e)
        {
            if (Playlists.SelectedItem != null)
            {
                ((GroovesharkPlaylistObject)Playlists.SelectedItem).Download();
            }
        }
        String downloading = "";
        void Download_Click(object sender, RoutedEventArgs e)
        {
            if (Songs.SelectedItem != null)
            {
                GroovesharkSongObject song = (GroovesharkSongObject)Songs.SelectedItem;
                DownloadPool.AddSongToDownloadQueue(song);
                DownloadPool.Download();
                downloading = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + song.Artist + " - " + song.Album + " - " + song.Name + ".mp3";
            }
        }

        void DownloadPool_ProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("[" + DownloadPool.DownloadIndex + "] " + e.ProgressPercentage);
            MainWindow.INSTANCE.Dispatcher.BeginInvoke(new Action(() =>
            {
                MainWindow.INSTANCE.player.MediaFailed += player_MediaFailed;
                MainWindow.INSTANCE.player.Source = new Uri(downloading);
            }));
        }

        void DownloadPool_DownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("SONG " + (DownloadPool.DownloadIndex + 1) + " / " + DownloadPool.TotalDownloadsInQueue + " Downloaded!");
        }

        void song_DownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("DOWNLOAD COMPLETED!");
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                try
                {
                    if (state == "Playing")
                    {
                        Grooveshark.Pause();
                        state = "Paused";
                        PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                        return;
                    }
                    Grooveshark.Play();
                    state = "Playing";
                    PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay.png"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Right)
            {
                playNext();
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Left)
            {
                playPrevius();
            }
        }

        void GetGroovifyURL_Click(object sender, RoutedEventArgs e)
        {
            if (Songs.SelectedItem != null)
            {
                GroovesharkSongObject song = (GroovesharkSongObject)Songs.SelectedItem;
                //Clipboard.SetText("GURL::" + song.SongID + "::" + song.Name + "::" + song.Artist + "::" + song.CoverArtFileName);
                Clipboard.SetText(Grooveshark.LINK_HOST + Grooveshark.Base64Encode(song.SongID + "::" + song.GetBaseName() + "::" + song.Artist + "::" + song.CoverArtFileName));
            }
        }

        void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            /*progress.Value = 0;
            progress.Maximum = player.NaturalDuration.HasTimeSpan ? (float)player.NaturalDuration.TimeSpan.TotalSeconds : 0;
            if (player.NaturalDuration.HasTimeSpan)
            {
                mediaLength = player.NaturalDuration.TimeSpan;
            }
            else
            {
                mediaLength = TimeSpan.FromSeconds(0);
            }*/
        }

        private void updatePos(object state)
        {
            if (Grooveshark.ChannelIsActive == BASSActive.BASS_ACTIVE_PLAYING)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    progress.Maximum = Grooveshark.LengthSecounds;
                    progress.Value = Grooveshark.PositionSecounds;
                    int totalMin = 0;
                    int totalSec = (int)Grooveshark.PositionSecounds;
                    while (totalSec > 60)
                    {
                        totalMin++;
                        totalSec -= 60;
                    }
                    String s = totalSec.ToString();
                    if (s.Length < 2) s = "0" + s;

                    CurrentTimeLabel.Content = totalMin  +":" + s;
                    int tMin = 0;
                    int tSec = (int)(Grooveshark.LengthSecounds - Grooveshark.PositionSecounds);
                    while (tSec > 60)
                    {
                        tMin++;
                        tSec -= 60;
                    }
                    String vSec = tSec.ToString();
                    if (vSec.Length < 2)
                    {
                        vSec = "0" + vSec;
                    }
                    CurrentTimeLeftLabel.Content = "-" + tMin + ":" + vSec;
                }));
            }
            else if (Grooveshark.ChannelIsActive == BASSActive.BASS_ACTIVE_STOPPED && CurrentlyPlayingSong != null)
            {
                if (Grooveshark.LengthSecounds - Grooveshark.PositionSecounds <= 3) playNext();
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Un4seen.Bass.Bass.LoadMe("Bass.dll");
            if (!Un4seen.Bass.Bass.BASS_Init(-1, 44100, Un4seen.Bass.BASSInit.BASS_DEVICE_LATENCY, Grooveshark.Handle))
            {
                MessageBox.Show("Error occured during init of BASS.\r\n" + Un4seen.Bass.Bass.ERROR);
            }
            else
            {
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Console.WriteLine(info.ToString());
            }

            PlaylistColumns.Columns[0].Width = Playlists.ActualWidth;
            foreach (var o in SongColumns.Columns)
            {
                o.Width = Songs.ActualWidth / SongColumns.Columns.Count;
            }
            this.SizeChanged += MainWindow_SizeChanged;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((Object o) =>
            {
                //Grooveshark.init();
            }));

            short atom = Win32.GlobalAddAtom("Groovify");
            Win32.RegisterHotKey(new WindowInteropHelper(this).Handle, atom, 0x0000, (uint)System.Windows.Forms.Keys.MediaPlayPause);
            Win32.RegisterHotKey(new WindowInteropHelper(this).Handle, atom, 0x0000, (uint)System.Windows.Forms.Keys.MediaStop);
            Win32.RegisterHotKey(new WindowInteropHelper(this).Handle, atom, 0x0000, (uint)System.Windows.Forms.Keys.MediaPreviousTrack);
            Win32.RegisterHotKey(new WindowInteropHelper(this).Handle, atom, 0x0000, (uint)System.Windows.Forms.Keys.MediaNextTrack);

            WindowInteropHelper wih = new WindowInteropHelper(this);
            HwndSource hWndSource = HwndSource.FromHwnd(wih.Handle);
            hWndSource.AddHook(WndProc);

            this.Title = "Groovify";

            this.NewVersionAvilable();

        }

        void hook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {

        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlaylistColumns.Columns[0].Width = Playlists.ActualWidth;
            foreach (var o in SongColumns.Columns)
            {
                o.Width = Songs.ActualWidth / SongColumns.Columns.Count;
            }

            if (!isMaxmimized && WindowState == System.Windows.WindowState.Maximized)
            {
                isMaxmimized = true;
                ToggleMaximize();
                ToggleMaximize();
            }
            else if (isMaxmimized && WindowState == System.Windows.WindowState.Normal)
            {
                //ToggleMaximize();
            }
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !MouseInside(progress) && !MouseInside(VolumeControl) && !MouseInside(Cover))
            {
                DragMove();
            }
        }

        private bool MouseInside(Control control)
        {
            Point p = Mouse.GetPosition(null);
            Point t = Mouse.GetPosition(control);
            double x = control.Margin.Left;
            double y = control.Margin.Top;
            double width = control.Margin.Right - control.Margin.Left;
            double height = control.Margin.Bottom - control.Margin.Top;
            if (t.X < control.ActualWidth && t.Y < control.ActualHeight && t.X >= 0 && t.Y >= 0) return true;
            return false;
        }

        private bool MouseInside(Decorator control)
        {
            Point p = Mouse.GetPosition(null);
            Point t = Mouse.GetPosition(control);
            double x = control.Margin.Left;
            double y = control.Margin.Top;
            double width = control.Margin.Right - control.Margin.Left;
            double height = control.Margin.Bottom - control.Margin.Top;
            if (t.X < control.ActualWidth && t.Y < control.ActualHeight && t.X >= 0 && t.Y >= 0) return true;
            return false;
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Min_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                return;
            }
            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void Hide_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Play()
        {

        }

        private void populatePlaylists()
        {

        }

        private void Playlists_MouseUp(object sender, MouseButtonEventArgs e)
        {
            populateSongs();
        }

        public void populateSongs()
        {
            Songs.Items.Clear();
            GroovesharkPlaylistObject playlist = (GroovesharkPlaylistObject)Playlists.SelectedItem;
            if (playlist == null || playlist.getSongs() == null || playlist.getSongs().Count == 0) return;
            for (int i = 0; i < playlist.getSongs().Count; i++)
            {
                Songs.Items.Add(playlist.getSongs()[i]);
            }
        }

        private void Songs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        public void playSelectedItem()
        {
            if (Songs.SelectedItem != null)
            {
                currentPlaylist = ((GroovesharkPlaylistObject)Playlists.SelectedItem);
                if (currentPlaylist == null && searchList == null && Playlists.Items.Count > 0) currentPlaylist = (GroovesharkPlaylistObject)Playlists.Items[0];
                if (currentPlaylist == null) currentPlaylist = searchList;
                GroovesharkSongObject o = (((GroovesharkSongObject)Songs.SelectedItem));
                playingIndex = currentPlaylist.getSongs().IndexOf(o);
                this.playSong(o);
            }
        }

        private void playSong(GroovesharkSongObject o, int t = 0)
        {
            CurrentlyPlayingSong = o;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((Object oo) =>
            {
                playedSongs.Add(o);
                playedSongsIndex = playedSongs.Count - 2;
                bool next = true;
                int i = 0;
                String songURL = "";
                do
                {
                    try
                    {
                        songURL = o.getStreamURL();
                        if (songURL == "")
                        {
                            next = true;
                        }
                        else if (songURL == "-1")
                        {
                            t++;
                            if (t >= 1 || t >= currentPlaylist.getSongs().Count)
                            {
                                songURL = o.GetStreamURLUnofical();
                                if (songURL != null && songURL != "" && songURL != String.Empty)
                                {

                                    next = false;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Playing next song! Got a url return of -1.");
                                    playNext(t);
                                    return;
                                }
                            }
                        }
                        next = false;
                    }
                    catch (System.UriFormatException ex)
                    {
                        Console.WriteLine("[WARNING]" + i.ToString() + ": " + ex.Message);
                        Console.WriteLine("Returned url: " + songURL);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[SEVERE]" + i.ToString() + ": " + ex.Message);
                        MessageBox.Show(ex.Message.ToString());
                        i++;
                    }
                } while (next && i < 5);

                MainWindow.INSTANCE.Dispatcher.Invoke(new Action(() =>
                {
                    Console.WriteLine("SongURL: " + songURL);
                    if (songURL == "") return;
                    Grooveshark.Play(songURL);
                    Grooveshark.contentLength = o.EstimatedDuration;
                    state = "Playing";
                }));
            }));
        }

        public void playNext(int t = 0)
        {
            if (t >= 3 || t >= currentPlaylist.getSongs().Count)
            {
                Grooveshark.Stop();
                state = "Stopped";
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                return;
            }
            if (shuffle)
            {
                playingIndex = new Random().Next(0, currentPlaylist.getSongs().Count - 1);
            }
            else
            {
                if (playingIndex >= currentPlaylist.getSongs().Count - 1)
                {
                    if (repeat)
                    {
                        playingIndex = 0;
                        playSong(currentPlaylist.getSongs()[playingIndex], t);
                        return;
                    }
                    else
                    {
                        Grooveshark.Stop();
                        state = "Stopped";
                        PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                    }
                }
            }
            if (playingIndex == currentPlaylist.getSongs().Count - 1)
            {
                Grooveshark.Stop();
                state = "Stopped";
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                return;
            }
            else
            {
                playingIndex++;
            }
            playSong(currentPlaylist.getSongs()[playingIndex], t);
        }

        public void playPrevius()
        {
            if (playedSongsIndex < 0 || playedSongsIndex > playedSongs.Count)
            {
                Grooveshark.Stop();
                state = "Stopped";
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                return;
            }
            GroovesharkSongObject o = playedSongs[playedSongsIndex];
            Grooveshark.Play(o.getStreamURL());
            state = "Playing";

            // UI Stuff
            PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay.png"));
            CurrentlyPlayingLabel.Content = o.Name;
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("http://images.gs-cdn.net/static/albums/70_" + (o.CoverArtFileName == "" ? "album.jpg" : o.CoverArtFileName))));
            Cover.Background = b;

            playedSongsIndex--;
        }

        private void Close_MouseEnter(object sender, MouseEventArgs e)
        {
            Close.Fill = new SolidColorBrush(Color.FromArgb(255, 209, 54, 54));
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e)
        {
            Close.Fill = new SolidColorBrush(Color.FromArgb(255, 163, 168, 178));
        }

        private void Min_MouseLeave(object sender, MouseEventArgs e)
        {
            Min.Fill = new SolidColorBrush(Color.FromArgb(255, 163, 168, 178));
        }

        private void Min_MouseEnter(object sender, MouseEventArgs e)
        {
            Min.Fill = new SolidColorBrush(Color.FromArgb(255, 54, 178, 209));
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                GroovesharkPlaylistObject o = Grooveshark.search(SearchBox.Text);
                searchList = currentPlaylist = o;
                Songs.Items.Clear();
                for (int i = 0; i < o.getSongs().Count; i++)
                {
                    Songs.Items.Add(o.getSongs()[i]);
                }
            }
        }

        private void Playlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Playlists.SelectedItem != null)
            {
                //currentPlaylist = (GroovesharkPlaylistObject)Playlists.SelectedItem;
                populateSongs();
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search")
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "")
            {
                SearchBox.Text = "Search";
            }
        }

        private void PlayButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (state == "Playing")
                {
                    Grooveshark.Pause();
                    state = "Paused";
                    PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                    return;
                }
                Grooveshark.Play();
                state = "Playing";
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay.png"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void player_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            state = "Playing";
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            playNext();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            playPrevius();
        }

        private void PlayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay.png"));
        }

        private void PlayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (state == "Paused" || state == "Stopped")
            {
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
            }
            else if (state == "Playing")
            {
                PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPaused.png"));
            }
        }

        private void RepeatButton_MouseEnter(object sender, MouseEventArgs e)
        {
            RepeatButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyRepeat.png"));
        }

        private void RepeatButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (repeat == false) RepeatButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyRepeat_dark.png"));
        }

        private void ShuffleButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ShuffleButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyShuffle.png"));
        }

        private void ShuffleButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (shuffle == false) ShuffleButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyShuffle_dark.png"));
        }

        private void ShuffleButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            shuffle = !shuffle;
        }

        private void RepeatButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            repeat = !repeat;
        }

        private void NextButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            playNext();
        }

        private void Songs_MouseMove(object sender, MouseEventArgs e)
        {

        }

        bool mouseDown = false;
        private void Songs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
        }

        private void Songs_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && Songs.SelectedItem != null)
            {
                DragDrop.DoDragDrop(Songs, Songs.SelectedItem, DragDropEffects.All);
                mouseDown = false;
            }
        }

        private void Songs_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void Playlists_DragEnter(object sender, DragEventArgs e)
        {
            //MessageBox.Show(e.Data.GetType().ToString());
            String[] formats = e.Data.GetFormats();
            if (formats[0].ToString() == "Launcher.GroovesharkSongObject")
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void Playlists_Drop(object sender, DragEventArgs e)
        {
            try
            {
                String[] formats = e.Data.GetFormats();
                if (formats[0].ToString() == "Launcher.GroovesharkSongObject")
                {
                    GroovesharkSongObject song = (GroovesharkSongObject)e.Data.GetData(formats[0].ToString());
                    var test = Playlists.InputHitTest(e.GetPosition(Playlists));
                    var res = GetVisualParent<ListViewItem>(test).Content;
                    if (res != null)
                    {
                        GroovesharkPlaylistObject par = (GroovesharkPlaylistObject)res;
                        Grooveshark.addSongToPlaylist(par, song);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Drag drop failed: " + ex.Message);
            }
        }

        public T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        private void Songs_KeyUp(object sender, KeyEventArgs e)
        {
            if (Songs.SelectedItem != null && e.Key == Key.Delete)
            {
                GroovesharkSongObject song = (GroovesharkSongObject)Songs.SelectedItem;
                Grooveshark.removeSongFromPlaylist((GroovesharkPlaylistObject)Playlists.SelectedItem, song);
            }
            else if (Songs.SelectedItem != null && e.Key == Key.Return)
            {
                playSelectedItem();
            }
        }

        private void Cover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (playedSongs.Count < 1 || currentPlaylist == null) return;
            Songs.SelectedItem = playedSongs[playedSongs.Count - 1];

            Songs.ScrollIntoView(playedSongs[playedSongs.Count - 1]);
        }

        private void Cover_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Songs.Items.Clear();
            Playlists.SelectedItem = currentPlaylist;
            GroovesharkPlaylistObject playlist = currentPlaylist;
            if (playlist == null || playlist.getSongs() == null || playlist.getSongs().Count == 0) return;
            for (int i = 0; i < playlist.getSongs().Count; i++)
            {
                Songs.Items.Add(playlist.getSongs()[i]);
            }
        }

        private void NewPlaylistBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Grooveshark.createPlaylist(NewPlaylistBox.Text);
            }
        }

        private void Playlists_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && Playlists.SelectedItem != null)
            {
                Grooveshark.removePlaylist((GroovesharkPlaylistObject)Playlists.SelectedItem);
            }
        }

        private void Playlists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Playlists.SelectedItem != null)
            {
                var playlist = (GroovesharkPlaylistObject)Playlists.SelectedItem;
                currentPlaylist = playlist;
                playingIndex = -1;
                playNext();
            }
        }

        private void PlayerBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void player_BufferingEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = currentPlaylist.getSongs()[playingIndex];
                Grooveshark.client.GetMusicStream((int)c.SongID, c.ArtistID).MarkAsDownloaded();
            }
            catch (Exception ex)
            {
                // Do nothing as it often just stops the music at worse
            }
        }

        private void NewPlaylistBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewPlaylistBox.Text == "New playlist")
            {
                NewPlaylistBox.Text = "";
                //NewPlaylistBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        private void NewPlaylistBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NewPlaylistBox.Text == "")
            {
                NewPlaylistBox.Text = "New playlist";
                //NewPlaylistBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        private void progress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void progress_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Grooveshark.GetAutoComplete(SearchBox.Text);
        }

        private void Songs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Songs_Drop(object sender, DragEventArgs e)
        {
            try
            {
                GroovesharkPlaylistObject currentPlaylist = (GroovesharkPlaylistObject)Playlists.SelectedItem;
                String[] formats = e.Data.GetFormats();
                if (formats[0].ToString() == "Launcher.GroovesharkSongObject")
                {
                    GroovesharkSongObject song = (GroovesharkSongObject)e.Data.GetData(formats[0].ToString());
                    var test = Songs.InputHitTest(e.GetPosition(Songs));
                    var li = GetVisualParent<ListViewItem>(test);
                    if (li != null && li.Content != null)
                    {
                        GroovesharkSongObject par = (GroovesharkSongObject)li.Content;
                        int addToIndex = currentPlaylist.getSongs().IndexOf(par);
                        currentPlaylist.getSongs().Remove(song);
                        currentPlaylist.getSongs().Insert(addToIndex, song);
                        Songs.Items.Clear();
                        populateSongs();
                        Grooveshark.updatePlaylistIDS(currentPlaylist);
                    }
                    else // The item was dropped at end of playlist so change order so it is at the end
                    {
                        currentPlaylist.getSongs().Remove(song);
                        Songs.Items.Remove(song);
                        currentPlaylist.getSongs().Add(song);
                        Songs.Items.Add(song);
                        Songs.Items.Clear();
                        populateSongs();
                        Grooveshark.updatePlaylistIDS(currentPlaylist);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Drag drop failed: " + ex.Message);
            }
        }

        private void Songs_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                String[] formats = e.Data.GetFormats();
                if (formats[0].ToString() == "Launcher.GroovesharkSongObject")
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Drag drop failed: " + ex.Message);
            }
        }

        private bool isMaxmimized = false;
        private double bleft, btop, bwidth, bheight;
        public void ToggleMaximize()
        {
            isMaxmimized = !isMaxmimized;
            this.Topmost = false;
            if (isMaxmimized)
            {
                bleft = this.Left;
                btop = this.Top;
                bwidth = this.Width;
                bheight = this.Height;

                //this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                //this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                //this.Left = 0;
                //this.Top = 0;
                //this.WindowState = System.Windows.WindowState.Maximized;

                Window window = Window.GetWindow(this);
                var wih = new WindowInteropHelper(window);
                IntPtr hwnd = wih.Handle;
                WmGetMinMaxInfo(hwnd, IntPtr.Zero);
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.Left = bleft;
                this.Top = btop;
                this.Width = bwidth;
                this.Height = bheight;
            }
        }

        private void state_MouseEnter(object sender, MouseEventArgs e)
        {
            State.Fill = new SolidColorBrush(Color.FromArgb(255, 54, 209, 98));
        }

        private void state_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isMaxmimized) State.Fill = new SolidColorBrush(Color.FromArgb(255, 163, 168, 178));
        }

        private void State_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleMaximize();
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            if (!isMaxmimized) return;
            //var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            var mmi = new MINMAXINFO();

            // Adjust the maximized size and position to fit the work area of the correct monitor
            IntPtr monitor = MonitorFromWindow(hwnd, (int)MonitorFromWindowFlags.MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                this.Left = rcMonitorArea.Left + (rcWorkArea.Left - rcMonitorArea.Left);
                this.Top = rcMonitorArea.Top + (rcWorkArea.Top - rcMonitorArea.Top);
                this.Width = mmi.ptMaxSize.X;
                this.Height = mmi.ptMaxSize.Y;
            }

            //Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var window = sender as Window;

            if (window != null)
            {
                IntPtr handle = (new WindowInteropHelper(window)).Handle;
                HwndSource.FromHwnd(handle).AddHook(WindowProc);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public Point ptReserved;

            public Point ptMaxSize;

            public Point ptMaxPosition;

            public Point ptMinTrackSize;

            public Point ptMaxTrackSize;
        } ;

        public enum MonitorFromWindowFlags
        {
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            public RECT rcMonitor;

            public RECT rcWork;

            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;

            public static readonly RECT Empty;

            public int Width
            {
                get
                {
                    return Math.Abs(this.Right - this.Left);
                } // Abs needed for BIDI OS
            }

            public int Height
            {
                get
                {
                    return this.Bottom - this.Top;
                }
            }

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                this.Left = rcSrc.Left;
                this.Top = rcSrc.Top;
                this.Right = rcSrc.Right;
                this.Bottom = rcSrc.Bottom;
            }

            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return this.Left >= this.Right || this.Top >= this.Bottom;
                }
            }

            public override string ToString()
            {
                if (this == Empty)
                {
                    return "RECT {Empty}";
                }
                return "RECT { left : " + this.Left + " / top : " + this.Top + " / right : " + this.Right + " / bottom : " +
                       this.Bottom + " }";
            }

            public override bool Equals(object obj)
            {
                if (!(obj is RECT))
                {
                    return false;
                }
                return (this == (RECT)obj);
            }

            public override int GetHashCode()
            {
                return this.Left.GetHashCode() + this.Top.GetHashCode() + this.Right.GetHashCode() +
                       this.Bottom.GetHashCode();
            }

            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.Left == rect2.Left && rect1.Top == rect2.Top && rect1.Right == rect2.Right &&
                        rect1.Bottom == rect2.Bottom);
            }

            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        bool CoverMouseDown = false;
        private void Cover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CoverMouseDown = true;
        }

        private void Cover_MouseMove(object sender, MouseEventArgs e)
        {
            if (CoverMouseDown)
            {
                CoverMouseDown = false;
                DragDrop.DoDragDrop(Songs, playedSongs[playedSongs.Count - 1], DragDropEffects.All);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        Boolean GotDown = false;
        private GroovesharkSongObject CurrentlyPlayingSong;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_HOTKEY)
            {
                if (GotDown == true)
                {
                    GotDown = false;
                    return IntPtr.Zero;
                }
                else
                {
                    GotDown = true;
                }

                var lpInt = (int)lParam;
                System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)((lpInt >> 16) & 0xFFFF);
                //Modifiers modifier = (Modifiers)(lpInt & 0xFFFF);
                if (key == System.Windows.Forms.Keys.MediaPlayPause)
                {
                    try
                    {
                        if (state == "Playing")
                        {
                            Grooveshark.Pause();
                            state = "Paused";
                            PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                            return IntPtr.Zero;
                        }
                        Grooveshark.Play();
                        state = "Playing";
                        PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPaused.png"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (key == System.Windows.Forms.Keys.MediaPlayPause || key == System.Windows.Forms.Keys.MediaStop)
                {
                    if (state == "Playing")
                    {
                        Grooveshark.Pause();
                        state = "Paused";
                        PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPlay_dark.png"));
                        return IntPtr.Zero;
                    }
                }
                else if (key == System.Windows.Forms.Keys.MediaNextTrack)
                {
                    playNext();
                }
                else if (key == System.Windows.Forms.Keys.MediaPreviousTrack)
                {
                    playPrevius();
                }
            }


            if (wParam.ToInt32() == 4561365)
            {
                var song = Grooveshark.readUrlClicked();
                var playlist = new GroovesharkPlaylistObject();
                playlist.addSong(song);

                searchList = currentPlaylist = playlist;
                Songs.Items.Clear();
                Songs.Items.Add(song);
                playSong(song);
                BringToFront();
            }
            return IntPtr.Zero;
        }

        private void ShuffleButton_Copy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int w = MessageHelper.FindWindow(null, "Groovify");
            MessageHelper m = new MessageHelper();
            m.bringAppToFront(w);
            int r = m.sendWindowsStringMessage(w, 4561365, "TEST");
        }

        public void BringToFront()
        {
            new MessageHelper().bringAppToFront(MessageHelper.FindWindow(null, this.Title));
        }

        public void NewVersionAvilable()
        {
            String cv = CurrentVersion();
            bool isAlpha = false;
            if (cv.StartsWith("A"))
            {
                isAlpha = true;
                cv = cv.Substring(1);
            }
            cv = cv.Replace(".", "");

            int newv = Convert.ToInt32(cv);
            var d = new String[] { "", "" };
            var v = new int[] { 0, 0 };
            using (WebClient c = new WebClient())
            {
                String downloaded = c.DownloadString("http://groovify.net46.net/public/Home/LatestGroovify");
                d = downloaded.Split(new String[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                d[0] = d[0].Substring(d[0].LastIndexOf('>') + 1).Trim();
                d[1] = d[1].Remove(d[1].IndexOf("<")).Trim();

                v[0] = Convert.ToInt32(d[0].Replace(".", ""));
                if (d[1].Replace(".", "").Trim() != "") v[1] = Convert.ToInt32(d[1].Replace(".", ""));
            }

            if (isAlpha)
            {
                if (newv < v[0])
                {
                    if (MessageBox.Show("There is a new alpha version out, would you like to download it?", "New verion found!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("Downloader.exe", "A" + d[0] + ".zip");
                        this.Close();
                    }
                }
                return;
            }
            if (newv < v[1])
            {
                if (MessageBox.Show("There is a new stable version out, would you like to download it?", "New verion found!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("Downloader.exe", "S" + d[1] + ".zip");
                    this.Close();
                }
            }
        }

        public String CurrentVersion()
        {
            using (StreamReader sr = new StreamReader("version"))
            {
                return sr.ReadLine();
            }
        }

        private void CurrentlyPlayingLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine(e.ErrorException.Message.ToString());

        }

        private void Songs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            playSelectedItem();
        }

        internal void BuildSongUI()
        {
            PlayButton.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf('\\')) + "/Resources/GroovifyPaused.png"));
            var info = Grooveshark.ChannelTag;
            if (info != null)
            {
                String artist = info.artist.Trim() == String.Empty ? CurrentlyPlayingSong.Artist : info.artist;
                String title = info.title.Trim() == String.Empty ? CurrentlyPlayingSong.Name : info.title;
                String album = info.album.Trim() == String.Empty ? CurrentlyPlayingSong.Album : info.album;
                //TextBlock na = new TextBlock();
                //na.Text = o.Name;
                //na.FontWeight = FontWeights.Normal;
                //CurrentlyPlayingLabel.Content = o.Artist + " - " + o.Name;
                CurrentlyPlayingLabel.Content = new Bold(new Run(title));
                CurrentlyPlayingLabel2.Content = artist + " - " + album;
            }
            String url = CurrentlyPlayingSong.CoverArtFileName;
            Console.WriteLine("Cover URL: " + url);
            var bmpImage = new BitmapImage(new Uri(url));
            bmpImage.DownloadFailed += bmpImage_DownloadFailed;
            ImageBrush b = new ImageBrush(bmpImage);
            Cover.Background = b;
        }

        void bmpImage_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bmpImage = new BitmapImage(new Uri("http://images.gs-cdn.net/static/albums/200_album.jpg"));
            ImageBrush b = new ImageBrush(bmpImage);
            Cover.Background = b;
        }
    }
}
