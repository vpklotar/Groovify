using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Threading;
using SciLorsGroovesharkAPI.Groove;
using System.Windows.Media;
using System.ComponentModel;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using System.Windows.Interop;

namespace Launcher
{
    public class Grooveshark
    {
        internal static GroovesharkClient client;
        public static String Username;
        public static int uID;
        private static String COM_TOKEN = "";
        public static String LINK_HOST = "http://groovify.net46.net/public/Song/";
        private static bool keepLoggedIn = true;
        private static int _CurrentChannel = -1;
        public static IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(MainWindow.INSTANCE).Handle;
            }
        }
        public static BASS_CHANNELINFO ChannelInfo
        {
            get
            {
                BASS_CHANNELINFO info = null;
                try
                {
                    info = new BASS_CHANNELINFO();
                    Bass.BASS_ChannelGetInfo(_CurrentChannel, info);
                }
                catch (Exception)
                {

                }
                return info;
            }
        }
        public static TAG_INFO ChannelTag
        {
            get
            {
                var info = ChannelInfo;
                if (info != null)
                {
                    TAG_INFO inf = new TAG_INFO();
                    BassTags.BASS_TAG_GetFromFile(_CurrentChannel, inf);
                    return inf;
                }
                return null;
            }
        }
        public static BASSActive ChannelIsActive
        {
            get
            {
                return Bass.BASS_ChannelIsActive(_CurrentChannel);
            }
        }

        public static void UPDATE()
        {

        }

        public static Boolean updateAvilable()
        {
            return false;
        }

        public static void downloadUpdate()
        {

        }

        public static List<GroovesharkPlaylistObject> Playlists = new List<GroovesharkPlaylistObject>();
        public static bool hasPlaylist(GroovesharkPlaylistObject playlist, List<GroovesharkPlaylistObject> Playlists)
        {
            if (Playlists == null) Playlists = Grooveshark.Playlists;
            for (int i = 0; i < Playlists.Count; i++)
            {
                if (playlist.playlistID == Playlists[i].playlistID) return true;
            }
            return false;
        }
        public static String loginResult = "";

        public static void login()
        {
            /*ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {
                var res = client.Login(username, password);
                uID = res.result.userID;
                Username = res.result.username;
                getAllPlaylists(null);
            }));*/

            /*for (int i = 0; i < Playlists.Count; i++)
            {
                MainWindow.INSTANCE.Playlists.Dispatcher.Invoke(new Action(() => MainWindow.INSTANCE.Playlists.Items.Add(Playlists[i])));
            }*/

            using (StreamReader sr = new StreamReader(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "GroovifyUserData.data")))
            {
                List<String> lines = new List<string>();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                //System.Windows.MessageBox.Show("TEST3");
                sr.Close();
                Username = lines[0];
                String pass = Base64Decode(lines[1]);
                //System.Windows.MessageBox.Show("TEST4 Base64Pass");
                keepLoggedIn = lines[2] == "1" ? true : false;
                var l = client.Login(Username, pass);
                //System.Windows.MessageBox.Show("TEST5 Logged in");
                uID = l.result.userID;
                getAllPlaylists(null);
            }
        }

        public static void GetAllLocalyCachedPlaylists()
        {
            Directory.CreateDirectory("Playlists");
            var parentDir = new DirectoryInfo("Playlists");

            foreach (FileInfo di in parentDir.GetFiles())
            {
                String[] info = di.Name.Split(',');
                var playlist = new GroovesharkPlaylistObject(Convert.ToInt32(info[1]), info[0]);
                if (MainWindow.currentPlaylist == null) MainWindow.currentPlaylist = playlist;
                using (StreamReader sr = new StreamReader(di.FullName))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //write += song.SongID + "::" + song.Name + "::" + song.Album + "::" + song.Artist + "::" + song.ArtistID + "::" + song.CoverArtFileName + System.Environment.NewLine;
                        String[] sinfo = line.Split(new String[] { "::" }, StringSplitOptions.None);
                        var temp = new SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult() { SongID = Convert.ToInt32(sinfo[0]), SongName = sinfo[1], AlbumName = sinfo[2], ArtistName = sinfo[3], ArtistID = Convert.ToInt32(sinfo[4]), CoverArtFilename = sinfo.Length > 5 ? sinfo[5] : "" };
                        var song = new GroovesharkSongObject(temp);
                        playlist.addSong(song);
                    }
                    sr.Close();
                }
                Playlists.Add(playlist);
                MainWindow.INSTANCE.Playlists.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.INSTANCE.Playlists.Items.Add(playlist);
                }));
            }
        }

        public static void getAllPlaylists(Object threadContext)
        {
            //List<GroovesharkPlaylistObject> listsDownloaded = new List<GroovesharkPlaylistObject>();
            List<GroovesharkPlaylistObject> JustDownloadedPlaylists = new List<GroovesharkPlaylistObject>();
            try
            {
                var res = client.GetPlaylists(uID);
                for (int i = 0; i < res.result.Playlists.Count; i++)
                {
                    var p = res.result.Playlists[i];
                    GroovesharkPlaylistObject playlist = new GroovesharkPlaylistObject(p);
                    JustDownloadedPlaylists.Add(playlist);
                    if (!Grooveshark.hasPlaylist(playlist, null))
                    {
                        Grooveshark.Playlists.Add(playlist);
                        //listsDownloaded.Add(playlist);
                        MainWindow.INSTANCE.Playlists.Dispatcher.Invoke(new Action(() => MainWindow.INSTANCE.Playlists.Items.Add(playlist)));
                        if (MainWindow.currentPlaylist == null)
                        {
                            MainWindow.currentPlaylist = playlist;
                            MainWindow.INSTANCE.Dispatcher.Invoke(new Action(() =>
                            {
                                MainWindow.INSTANCE.populateSongs();
                            }));
                        }
                    }
                    else
                    {
                        GroovesharkPlaylistObject ppl = null;
                        for (int j = 0; j < Playlists.Count; j++)
                        {
                            if (Playlists[j].playlistID == playlist.playlistID)
                            {
                                ppl = Playlists[j];
                                break;
                            }
                        }
                        if (ppl != null)
                        {
                            ppl.UpdateSong(playlist);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var di = new DirectoryInfo("Playlists");
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                FileInfo fi = fis[i];
                String[] info = fi.Name.Split(',');
                var playlist = new GroovesharkPlaylistObject(Convert.ToInt32(info[1]), info[0]);
                if (!hasPlaylist(playlist, JustDownloadedPlaylists))
                {
                    File.Delete(fi.FullName);
                    var p = GetPlaylistByID(playlist.playlistID);
                    if (p != null)
                    {
                        Playlists.Remove(p);
                        MainWindow.INSTANCE.Playlists.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MainWindow.INSTANCE.Playlists.Items.Remove(p);
                        }));
                    }
                }
            }
        }

        public static GroovesharkPlaylistObject GetPlaylistByID(int id)
        {
            for (int i = 0; i < Playlists.Count; i++)
            {
                if (Playlists[i].playlistID == id) return Playlists[i];
            }
            return null;
        }

        public static void init()
        {
            //System.Windows.MessageBox.Show("TEST2");
            try
            {
                if (client == null) client = new GroovesharkClient() { UseGZip = true, AutoReconnectWithoutMessageBox = true };
                if (COM_TOKEN == null || COM_TOKEN == String.Empty || COM_TOKEN == "") COM_TOKEN = client.CommunicationToken;
                login();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            /*if (comToken == "")
            {
                //JObject com = JObject.Parse(HttpPost("getCommunicationToken", "{\"header\":{\"client\":\"jsqueue\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"EFE44D99-824E-4616-97D7-C9FB7BBF32E0\",\"session\":\"e901675acbaaba4fb4d1a712cf6ed4a8\"},\"method\":\"getCommunicationToken\",\"parameters\":{\"secretKey\":\"c189ed93fb434a90da0eef46a3db8c0e\"}}"));
                String r = HttpPost("http://grooveshark.com/preload.php?getCommunicationToken=1&hash=&1396481437293", "");
                String[] rr = r.Split(new String[] { "\"getCommunicationToken\":\"" }, StringSplitOptions.RemoveEmptyEntries);
                String token = rr[1].Split(new String[] { "\"," }, StringSplitOptions.RemoveEmptyEntries).First();
                //r = r.Substring(19, r.IndexOf("};") - 19);
                //JObject com = JObject.Parse(r);
                //comToken = com["getCommunicationToken"].ToString();
                comToken = token;
            }*/
        }

        public static GroovesharkPlaylistObject search(String querry)
        {
            GroovesharkPlaylistObject searchList = new GroovesharkPlaylistObject();
            String r = HttpPost("http://tinysong.com/s/" + System.Net.WebUtility.UrlEncode(querry) + "?format=json&limit=64&key=16e37e8e9e47102d2f5fc8086790298f", "");
            //r = r.Split('<')[0].ToString();
            r = r.Replace("<html>", "").Replace("</html>", "").Replace("<body>", "").Replace("</body>", "").Replace("<head></head>", "");
            JArray result = JArray.Parse(r);
            //var res = Grooveshark.client.
            for (int i = 0; i < result.Count; i++)
            {
                var res = result[i];
                GroovesharkSongObject obj = new GroovesharkSongObject() { SongID = Convert.ToInt32(res["SongID"].ToString()), Name = res["SongName"].ToString() };
                searchList.addSong(obj);
            }

            /*if (querry.ToLower().StartsWith("gurl::"))
            {
                //searchList.addSong(GroovesharkSongObject.GetSongFromGroovifyURL(querry));
            }
            else
            {
                var res = Grooveshark.client.SearchArtist(querry).result.result;
                for (int i = 0; i < res.Count; i++)
                {
                    var item = res[i];
                    searchList.addSong(new GroovesharkSongObject(item));
                }
            }*/
            return searchList;
        }

        public static List<String> GetAutoComplete(String s)
        {
            List<String> re = new List<string>();
            try
            {
                String r = HttpPost("http://grooveshark.com/more.php?getAutocompleteEx", "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"D2D647A7-2D4F-4ED0-AC58-23465D09D7FB\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("getAutocompleteEx") + "\"},\"method\":\"getAutocompleteEx\",\"parameters\":{\"query\":\"" + s + "\",\"type\":\"combined\"}}");
                var g = Newtonsoft.Json.Linq.JObject.Parse(r);
                var ar = g["result"]["artist"];
                var so = g["result"]["song"];
                for (int i = 0; i < ar.Count(); i++)
                {
                    re.Add(ar[i]["Name"].ToString());
                    Console.WriteLine("Added artist: " + ar[i]["Name"].ToString());
                }
                for (int i = 0; i < so.Count(); i++)
                {
                    re.Add(so[i]["Name"].ToString());
                    Console.WriteLine("Added song: " + so[i]["Name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return re;
        }

        public static string HttpPost(string URI, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static void addSongToPlaylist(GroovesharkPlaylistObject playlist, GroovesharkSongObject song)
        {
            playlist.addSong(song);
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {
                HttpPost("http://grooveshark.com/more.php?playlistAddSongToExistingEx", "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"FB4751A2-A242-4FFB-95B4-BF7567A067A3\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("playlistAddSongToExistingEx") + "\"},\"method\":\"playlistAddSongToExistingEx\",\"parameters\":{\"playlistID\":" + playlist.playlistID + ",\"songID\":" + song.SongID + "}}");
            }));
        }

        public static void removeSongFromPlaylist(GroovesharkPlaylistObject playlist, GroovesharkSongObject song)
        {
            playlist.removeSong(song);
            MainWindow.INSTANCE.Songs.Items.Remove(song);
            updatePlaylistIDS(playlist);
        }

        public static void updatePlaylistIDS(GroovesharkPlaylistObject playlist)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {
                String json = "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"B70429C4-753F-4DE5-BB14-C29403C2C938\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("overwritePlaylistEx") + "\"},\"method\":\"overwritePlaylistEx\",\"parameters\":{\"playlistID\":" + playlist.playlistID + ",\"playlistName\":\"" + playlist.Name + "\",\"songIDs\":[";
                for (int i = 0; i < playlist.getSongs().Count; i++)
                {
                    GroovesharkSongObject s = playlist.getSongs()[i];
                    json += s.SongID + ",";
                }
                json = json.Substring(0, json.Length - 1);
                json += "]}}";

                string res = HttpPost("http://grooveshark.com/more.php?overwritePlaylistEx", json);
            }));
        }

        public static void createPlaylist(String name)
        {
            String json = "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"1A34BD6F-1E60-48DF-AA89-759EB5C70C0B\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("createPlaylistEx") + "\"},\"method\":\"createPlaylistEx\",\"parameters\":{\"playlistName\":\"" + name + "\",\"songIDs\":[],\"playlistAbout\":\"\"}}";
            var res = HttpPost("http://grooveshark.com/more.php?createPlaylistEx", json);
            var j = Newtonsoft.Json.Linq.JObject.Parse(res);
            MainWindow.INSTANCE.Playlists.Items.Add(new GroovesharkPlaylistObject(Convert.ToInt32(j["result"].ToString()), name));
        }

        public static void removePlaylist(GroovesharkPlaylistObject playlist)
        {
            MainWindow.INSTANCE.Playlists.Items.Remove(playlist);
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {
                String json = "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"F5AAD634-87D8-4BFF-9A47-685EA0B85CC2\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("deletePlaylist") + "\"},\"method\":\"deletePlaylist\",\"parameters\":{\"playlistID\":" + playlist.playlistID + ",\"name\":\"" + playlist.Name + "\"}}";
                HttpPost("http://grooveshark.com/more.php?deletePlaylist", json);
            }));
        }

        internal static string GetRequestToken(string method)
        {
            string randomHexStr = GetRandomHexStr();
            //string str = method + ":" + COM_TOKEN + ":nuggetsOfBaller:" + randomHexStr;
            string str = method + ":" + COM_TOKEN + ":nuggetsOfBaller:" + randomHexStr;
            string shA1Hash = HashHelper.GetSHA1Hash(str);
            string text = randomHexStr + shA1Hash;
            return text;
        }

        private static string GetRandomHexStr()
        {
            char[] chArray = new char[6];
            Random random = new Random();
            int index = 0;
            do
            {
                chArray[index] = "0123456789abcdef"[random.Next(16)];
                checked { ++index; }
            }
            while (index <= 5);
            return new string(chArray);
        }

        public static String GOOGLE_IMAGE_SEARCH_GET_FIRST_URL(String search)
        {
            search = System.Net.WebUtility.UrlEncode(search);
            using (WebClient c = new WebClient())
            {
                try
                {
                    String r = c.DownloadString("https://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + search);
                    var j = Newtonsoft.Json.Linq.JObject.Parse(r);
                    var jj = j["responseData"];
                    jj = jj["results"];
                    jj = jj[0];
                    return jj["url"].ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }
        }
        public static String GetStreamURL(int SongID, bool isRetry = false)
        {
            if (isRetry)
            {
                COM_TOKEN = GetComToken();
            }
            //String json = "{\"header\":{\"client\":\"htmlshark\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"ID\":190,\"CC1\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"CC4\":0,\"DMA\":0,\"IPR\":0},\"uuid\":\"1A34BD6F-1E60-48DF-AA89-759EB5C70C0B\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"token\":\"" + GetRequestToken("createPlaylistEx") + "\"},\"method\":\"createPlaylistEx\",\"parameters\":{\"playlistName\":\"" + name + "\",\"songIDs\":[],\"playlistAbout\":\"\"}}";
            try
            {
                /*COM_TOKEN = GetComToken();
                String sessID = GetSessionID();*/
                //String json = "{\"parameters\":{\"songID\":" + SongID + ",\"type\":16,\"country\":{\"CC4\":0,\"DMA\":0,\"CC1\":0,\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190},\"mobile\":false,\"prefetch\":false},\"method\":\"getStreamKeyFromSongIDEx\",\"header\":{\"token\":\"" + GetRequestToken("getStreamKeyFromSongIDEx") + "\",\"privacy\":0,\"country\":{\"CC4\":0,\"DMA\":0,\"CC1\":0,\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190},\"uuid\":\"01AB1DB6-C4DF-4F7B-AA95-495DC0B484F3\",\"client\":\"htmlshark\",\"session\":\"" + client.GetSessionIdFromApi() + "\",\"clientRevision\":\"20130520.14\"}}";
                //String json = "{\"parameters\":{\"songID\":" + SongID + ",\"type\":16,\"country\":{\"CC4\":0,\"DMA\":0,\"CC1\":0,\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190},\"mobile\":false,\"prefetch\":false},\"method\":\"getStreamKeyFromSongIDEx\",\"header\":{\"token\":\"" + GetRequestToken("getStreamKeyFromSongIDEx") + "\",\"privacy\":0,\"country\":{\"CC4\":0,\"DMA\":0,\"CC1\":0,\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190},\"uuid\":\"01AB1DB6-C4DF-4F7B-AA95-495DC0B484F3\",\"client\":\"htmlshark\",\"session\":\"" + sessID + "\",\"clientRevision\":\"20130520.14\"}}";
                String json = "{\"header\":{\"session\":\"12c75665779fc5862d87ee8200b91952\",\"client\":\"jsqueue\",\"uuid\":\"6D9B4599-EE13-4455-AF27-D1F6139CF60D\",\"clientRevision\":\"20130520\",\"privacy\":0,\"country\":{\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190,\"CC4\":0,\"DMA\":0,\"CC1\":0},\"token\":\"179318e0b91debf15fbf5e8939b1220946ef9d4a7c6162\"},\"method\":\"getStreamKeyFromSongIDEx\",\"parameters\":{\"mobile\":false,\"songID\":"+SongID+",\"country\":{\"IPR\":0,\"CC2\":0,\"CC3\":2305843009213694000,\"ID\":190,\"CC4\":0,\"DMA\":0,\"CC1\":0},\"type\":16,\"prefetch\":false}}";
                var res = HttpPost("http://grooveshark.com/more.php?getStreamKeyFromSongIDEx", json);
                var j = Newtonsoft.Json.Linq.JObject.Parse(res);
                String ip = j["result"]["ip"].ToString();
                String streamKey = j["result"]["streamKey"].ToString();
                String url = "http://" + ip + "/stream.php?streamKey=" + streamKey;
                return url;
            }
            catch (Exception ex)
            {
                if (!isRetry) return GetStreamURL(SongID, true);
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static String GetComToken()
        {
            String r = HttpPost("http://grooveshark.com/preload.php?getCommunicationToken=1", "");

            r = r.Substring(46);
            r = r.Remove(r.IndexOf('"'));

            return r;
        }

        public static String GetSessionID()
        {
            String r = HttpPost("http://grooveshark.com/preload.php?getCommunicationToken=1", "");

            r = r.Substring(37254);
            r = r.Remove(r.IndexOf('"'));

            return r;
        }

        public static GroovesharkSongObject readUrlClicked()
        {
            List<String> parts = new List<string>();
            using (StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\songclicked.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    parts.Add(line);
                }
                sr.Close();
            }
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\songclicked.txt");
            String cover = "";
            if (parts.Count >= 4) cover = parts[3];
            return GroovesharkSongObject.GetSongFromGroovifyURL(parts[2], parts[1], parts[0], cover);
        }

        public static void WriteUrlClicked(String url)
        {
            url = url.Replace("grv://", "");
            url = url.Substring(0, url.Length - 1);
            using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\songclicked.txt"))
            {
                String[] parts = Base64Decode(url).Split(new String[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                String write = "";
                foreach (String s in parts)
                {
                    write += s + System.Environment.NewLine;
                }
                sw.Write(write.Trim());
                sw.Close();
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static void SavePlaylists()
        {
            Directory.CreateDirectory("Playlists");
            for (int i = 0; i < Playlists.Count; i++)
            {
                GroovesharkPlaylistObject playlist = Playlists[i];
                using (StreamWriter sw = new StreamWriter("Playlists\\" + playlist.Name + "," + playlist.playlistID))
                {
                    String write = "";
                    for (int j = 0; j < playlist.getSongs().Count; j++)
                    {
                        var song = playlist.getSongs()[j];
                        write += song.SongID + "::" + song.Name + "::" + song.Album + "::" + song.Artist + "::" + song.ArtistID + "::" + song.CoverArtFileName + System.Environment.NewLine;
                    }
                    write = write.Trim();
                    sw.Write(write);
                    sw.Close();
                }
            }
        }

        private static Stream readStream = null;
        public static long contentLength = 0;
        public static void Play(string url)
        {
            Grooveshark.Stop();

            if (url.ToLower().StartsWith("http"))
            {
                _CurrentChannel = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_ASYNCFILE, null, Grooveshark.Handle);
            }
            else
            {
                _CurrentChannel = Bass.BASS_StreamCreateFile(url, 0, 0, BASSFlag.BASS_ASYNCFILE);
            }
            Bass.BASS_ChannelPlay(_CurrentChannel, false);

            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {
                int t = 0;
                do
                {
                    if (ChannelTag != null)
                    {
                        Console.WriteLine("Building UI Data");
                        MainWindow.INSTANCE.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MainWindow.INSTANCE.BuildSongUI();
                        }));
                        break;
                    }
                    else
                    {
                        Console.WriteLine("ChannelTag is null, sleeping for 2 sec.");
                        t++;
                        System.Threading.Thread.Sleep(2000);
                    }
                } while (ChannelTag == null && t < 100);
                if (t >= 100)
                {
                    Console.WriteLine("Failed to get ChannelTag, tag return: " + ChannelTag);
                }
            }));
        }

        public static long Position
        {
            get
            {
                return Bass.BASS_ChannelGetPosition(_CurrentChannel);
            }
        }
        public static float PositionSecounds
        {
            get
            {
                return (float)Bass.BASS_ChannelBytes2Seconds(_CurrentChannel, Position);
            }
        }

        public static void Seek(int sec)
        {
            var pos = Bass.BASS_ChannelSeconds2Bytes(_CurrentChannel, sec);
            Console.WriteLine(Bass.BASS_ChannelSetPosition(_CurrentChannel, pos));
        }

        public static long Length
        {
            get
            {
                return Bass.BASS_ChannelGetLength(_CurrentChannel);
            }
        }

        public static float LengthSecounds
        {
            get
            {
                return (float)Bass.BASS_ChannelBytes2Seconds(_CurrentChannel, Length);
            }
        }

        public static void Play()
        {
            Bass.BASS_ChannelPlay(_CurrentChannel, false);
        }

        public static void Pause()
        {
            Bass.BASS_ChannelPause(_CurrentChannel);
        }

        public static void Stop()
        {
            Bass.BASS_ChannelStop(_CurrentChannel);
        }
    }

    public class GroovesharkPlaylistObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private List<GroovesharkSongObject> songs = new List<GroovesharkSongObject>();
        public int playlistID
        {
            get
            {
                return _p.playlistID;
            }
        }
        private String BaseName = "";

        public String GetBaseName()
        {
            return BaseName;
        }
        public String Name
        {
            get
            {
                return _p.name;
            }
            set
            {
                _p.name = value;
                OnPropertyChanged("Name");
            }
        }
        protected JObject o;
        protected String rawJson = "";
        protected SciLorsGroovesharkAPI.Groove.Functions.UserGetPlaylists.MyResult _p;

        public GroovesharkPlaylistObject(SciLorsGroovesharkAPI.Groove.Functions.UserGetPlaylists.MyResult p)
        {
            _p = p;
            BaseName = _p.name;
            /*System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback((Object o) =>
            {*/
            try
            {
                SciLorsGroovesharkAPI.Groove.Functions.PlaylistGetSongs.playlistGetSongsResponse songs = Grooveshark.client.GetPlaylistSongs(playlistID);
                for (int i = 0; i < songs.result.Songs.Count; i++)
                {
                    SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult song = songs.result.Songs[i];
                    var s = new GroovesharkSongObject(song);
                    addSong(s);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //}));
        }

        public GroovesharkPlaylistObject(int ID, String name)
        {
            _p = new SciLorsGroovesharkAPI.Groove.Functions.UserGetPlaylists.MyResult() { playlistID = ID, name = name };
        }

        public GroovesharkPlaylistObject()
        {

        }

        private static void refreshSongs()
        {

        }

        public void addSong(GroovesharkSongObject song)
        {
            songs.Add(song);
            if (MainWindow.currentPlaylist == this)
            {
                MainWindow.INSTANCE.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow.INSTANCE.Songs.Items.Add(song);
                }));
            }
        }

        public void removeSong(GroovesharkSongObject song)
        {
            songs.Remove(song);
            if (MainWindow.currentPlaylist == this)
            {
                MainWindow.INSTANCE.Songs.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.INSTANCE.Songs.Items.Remove(song);
                }));
            }
        }

        public List<GroovesharkSongObject> getSongs()
        {
            return songs;
        }

        public bool DownloadRunning = false;
        public void Download()
        {
            DownloadRunning = true;
            DownloadPoolObject pool = new DownloadPoolObject();
            pool.DownloadComplete += pool_DownloadComplete;
            pool.ProgressChanged += pool_ProgressChanged;
            for (int i = 0; i < songs.Count; i++)
            {
                pool.AddSongToDownloadQueue(songs[i]);
            }
            pool.Download();
        }

        void pool_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadPoolObject pool = (DownloadPoolObject)sender;
            double per = (pool.DownloadIndex - 1) * 100;
            if (per < 0) per = 0;
            per += e.ProgressPercentage;
            per = (per / (pool.TotalDownloadsInQueue * 100D)) * 100;
            per = Math.Round(per, 2, MidpointRounding.AwayFromZero);
            this.Name = BaseName + "[" + per + "%]";
        }

        void pool_DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            DownloadPoolObject pool = (DownloadPoolObject)sender;
            if (pool.DownloadIndex >= pool.TotalDownloadsInQueue)
            {
                this.Name = BaseName;
                DownloadRunning = false; ;
            }
        }

        public bool hasSong(GroovesharkSongObject song)
        {
            for (int i = 0; i < songs.Count; i++)
            {
                if (songs[i].SongID == song.SongID) return true;
            }
            return false;
        }

        internal void UpdateSong(GroovesharkPlaylistObject playlist)
        {
            for (int i = 0; i < playlist.songs.Count; i++)
            {
                if (!hasSong(playlist.songs[i]))
                {
                    this.addSong(playlist.songs[i]);
                }
            }
            for (int i = 0; i < songs.Count; i++)
            {
                if (!playlist.hasSong(songs[i]))
                {
                    this.removeSong(songs[i]);
                }
            }
        }
    }

    public class GroovesharkSongObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public static GroovesharkSongObject GetSongFromGroovifyURL(String artist, String Name, String ID, String Cover = "")
        {
            var s = new SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult();
            s.SongID = Convert.ToInt32(ID);
            s.SongName = Name;
            s.ArtistName = artist;
            s.CoverArtFilename = Cover;
            return new GroovesharkSongObject(s);
        }

        private String BaseName = "";
        public String GetBaseName()
        {
            return BaseName;
        }
        public string Name
        {
            get
            {
                return _s.SongName;
            }
            set
            {
                _s.SongName = value;
                OnPropertyChanged("Name");
            }
        }
        public int SongID = 0;
        private String _streamURL = "";
        //private 
        public string Artist
        {
            get
            {
                return _s.ArtistName;
            }
        }
        public int ArtistID
        {
            get
            {
                return _s.ArtistID;
            }
        }

        public long EstimatedDuration
        {
            get
            {
                if (!isDownloading && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + Artist + " - " + Album + " - " + Name + ".mp3"))
                {
                    _streamURL = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + Artist + " - " + Album + " - " + Name + ".mp3";
                }
                return (long)_s.EstimateDuration;
            }
        }
        public String CoverArtFileName
        {
            get
            {
                String url = _s.CoverArtFilename;
                if (url == String.Empty || url == null)
                {
                    url = Grooveshark.GOOGLE_IMAGE_SEARCH_GET_FIRST_URL(Artist + " " + Album);
                    if (url == String.Empty || url == null) url = "http://images.gs-cdn.net/static/albums/70_album.jpg";
                }
                if (url != String.Empty && url != null && !url.Contains('/')) url = "http://images.gs-cdn.net/static/albums/200_" + url;
                return url;
            }
        }
        private SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult _s;

        public GroovesharkSongObject()
        {
            _s = new SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult();
        }

        public GroovesharkSongObject(SciLorsGroovesharkAPI.Groove.Functions.SearchArtist.SearchArtistResult song)
        {
            _s = song;
            BaseName = _s.SongName;
            SongID = song.SongID;
        }

        public String Album
        {
            get
            {
                return _s.AlbumName;
            }
        }

        public String getStreamURL()
        {
            //return Grooveshark.GetStreamURL(SongID);
            if (!isDownloading && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + Artist + " - " + Album + " - " + Name + ".mp3"))
            {
                _streamURL = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + Artist + " - " + Album + " - " + Name + ".mp3";
                return _streamURL;
            }

            try
            {
                //var s = Grooveshark.GetStreamURL(SongID);
                var res = Grooveshark.client.GetStreamKey(SongID);
                _streamURL = "http://" + res.ip + "/stream.php?streamKey=" + res.streamKey;
                return _streamURL;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                try
                {
                    return GetStreamURLUnofical();
                }
                catch (Exception eex)
                {
                    Console.WriteLine("Try 2: " + eex.Message);
                    return "-1";
                }
            }
            //return Grooveshark.GetStreamURL(SongID);
        }

        public String GetStreamURLUnofical()
        {
            return Grooveshark.GetStreamURL(SongID);
        }

        private WebClient client = new WebClient();
        private bool isDownloading = false;
        public void Download(String SavePath)
        {
            isDownloading = true;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(this.getStreamURL()), SavePath);
        }

        public void CancelDownload()
        {
            client.CancelAsync();
            isDownloading = false;
        }

        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownloadComplete != null) DownloadComplete(this, e);
            this.Name = BaseName;
            isDownloading = false;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(this, e);
            this.Name = BaseName + " [" + e.ProgressPercentage + "%]";
        }
        public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged = null;
        public event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> DownloadComplete = null;
    }

    public class DownloadPool
    {
        private static List<GroovesharkSongObject> songsToDownload = new List<GroovesharkSongObject>();
        public static int DownloadIndex = 0;
        public static int TotalDownloadsInQueue = 0;
        private static GroovesharkSongObject CurrentlyDownloadingSong = null;
        public static Boolean DownloadRunning = false;

        public static void AddSongToDownloadQueue(GroovesharkSongObject song)
        {
            songsToDownload.Add(song);
            TotalDownloadsInQueue = songsToDownload.Count;
        }

        public static void RemoveSongFromDownloadQueue(GroovesharkSongObject song)
        {
            songsToDownload.Remove(song);
            TotalDownloadsInQueue = songsToDownload.Count;
        }

        public static void Download()
        {
            if (DownloadRunning) return;
            DownloadRunning = true;
            if (DownloadIndex >= songsToDownload.Count)
            {
                Reset();
                return;
            }
            GroovesharkSongObject song = songsToDownload[DownloadIndex];
            song.DownloadComplete += DownloadPool_DownloadComplete;
            song.ProgressChanged += DownloadPool_ProgressChanged;
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\");
            song.Download(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + song.Artist + " - " + song.Album + " - " + song.Name + ".mp3");
        }

        static void DownloadPool_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(null, e);
        }

        static void DownloadPool_DownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownloadComplete != null) DownloadComplete(null, e);
            GoToNextDownload();
        }

        private static void GoToNextDownload()
        {
            DownloadIndex++;
            DownloadRunning = false;
            Download();
        }

        public static void StopDownload()
        {
            Reset();
        }

        public static void Reset()
        {
            if (CurrentlyDownloadingSong != null) CurrentlyDownloadingSong.CancelDownload();
            CurrentlyDownloadingSong = null;
            DownloadIndex = 0;
            TotalDownloadsInQueue = 0;
            songsToDownload.Clear();
            DownloadRunning = false;
        }

        public static event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged = null;
        public static event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> DownloadComplete = null;
    }

    public class DownloadPoolObject
    {
        private List<GroovesharkSongObject> songsToDownload = new List<GroovesharkSongObject>();
        public int DownloadIndex = 0;
        public int TotalDownloadsInQueue = 0;
        private GroovesharkSongObject CurrentlyDownloadingSong = null;
        public Boolean DownloadRunning = false;

        public void AddSongToDownloadQueue(GroovesharkSongObject song)
        {
            songsToDownload.Add(song);
            TotalDownloadsInQueue = songsToDownload.Count;
        }

        public void RemoveSongFromDownloadQueue(GroovesharkSongObject song)
        {
            songsToDownload.Remove(song);
            TotalDownloadsInQueue = songsToDownload.Count;
        }

        public void Download()
        {
            if (DownloadRunning) return;
            DownloadRunning = true;
            if (DownloadIndex >= songsToDownload.Count)
            {
                Reset();
                return;
            }
            GroovesharkSongObject song = songsToDownload[DownloadIndex];
            song.DownloadComplete += DownloadPool_DownloadComplete;
            song.ProgressChanged += DownloadPool_ProgressChanged;
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\");
            song.Download(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Groovify\\" + song.Artist + " - " + song.Album + " - " + song.Name + ".mp3");
        }

        void DownloadPool_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(this, e);
        }

        void DownloadPool_DownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownloadComplete != null) DownloadComplete(this, e);
            GoToNextDownload();
        }

        private void GoToNextDownload()
        {
            DownloadIndex++;
            DownloadRunning = false;
            Download();
        }

        public void StopDownload()
        {
            Reset();
        }

        public void Reset()
        {
            if (CurrentlyDownloadingSong != null) CurrentlyDownloadingSong.CancelDownload();
            CurrentlyDownloadingSong = null;
            DownloadIndex = 0;
            TotalDownloadsInQueue = 0;
            songsToDownload.Clear();
            DownloadRunning = false;
        }

        public event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged = null;
        public event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> DownloadComplete = null;
    }
}
