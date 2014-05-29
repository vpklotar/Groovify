using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static String FILE_TO_DOWNLOAD = "A0.0.7.zip";
        protected override void OnStartup(StartupEventArgs e)
        {
            if(e.Args.Length > 0) FILE_TO_DOWNLOAD = e.Args[0].ToString();
            base.OnStartup(e);
        }

    }
}
