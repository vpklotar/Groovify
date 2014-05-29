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

using System.Net;
using System.IO;
using Ionic.Zip;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow instance;
        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            using (WebClient c = new WebClient())
            {
                c.DownloadProgressChanged += c_DownloadProgressChanged;
                c.DownloadFileCompleted += c_DownloadFileCompleted;
                c.DownloadFileAsync(new System.Uri("http://groovify.net46.net/public/Home/Download/" + App.FILE_TO_DOWNLOAD), "tmp.zip");
            }
        }

        void c_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Status.Content = "Status: Unzipping and installing";
            string extractDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            extractDir = extractDir.Remove(extractDir.LastIndexOf("\\"));
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((Object o) =>
            {
                using (ZipFile file = ZipFile.Read("tmp.zip"))
                {
                    foreach (ZipEntry zip in file)
                    {
                        try
                        {
                            File.Delete(zip.FileName);
                            //if (zip.FileName == "Downloader.exe") continue;
                            instance.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Status.Content = "Extracting: " + zip.FileName;
                            }));
                            zip.Extract(extractDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + "::" + zip.FileName);
                        }
                    }
                }
                using (StreamWriter sw = new StreamWriter("version"))
                {
                    sw.Write(App.FILE_TO_DOWNLOAD.Substring(0, App.FILE_TO_DOWNLOAD.Length-4)); // Remove the .zip extension
                    sw.Close();
                }
                instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Status.Content = "Update finished. Launcing Groovify...";
                    System.Diagnostics.Process.Start("Launcher.exe");
                    this.Close();
                }));
            }));
        }

        void c_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.VisualProgress.Value = e.ProgressPercentage;
            DownloadBytes.Content = e.BytesReceived + " / " + e.TotalBytesToReceive + "b";
            Status.Content = "Status: Downloading...";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
