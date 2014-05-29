using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Diagnostics;
using System.Text;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process[] procs = Process.GetProcessesByName("MainClient");
            int c = 0;
            for (int i = 0; i < procs.Count(); i++)
            {
                if (procs[i].MainWindowTitle.Trim() == "")
                {
                    // Do nothing!
                }
                else
                {
                    c++;
                }
            }
            try
            {

                if (e.Args.Length > 0 && c > 0)
                {
                    int w = MessageHelper.FindWindow(null, "Groovify");
                    Grooveshark.WriteUrlClicked(e.Args[0].ToString());
                    var m = new MessageHelper();
                    m.sendWindowsStringMessage(w, 4561365, "TEST");
                    //new MessageHelper().bringAppToFront(procs[0].MainWindowHandle.ToInt32());
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            if (c != 0)
            {
                //MessageBox.Show("There is already a Groovify instance running, shutting down this instance!");
                Application.Current.Shutdown();
                return;
            }
            else
            {
                base.OnStartup(e);
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((Object o) =>
                {
                    
                    //Grooveshark.login(e.Args[0].ToString(), e.Args[1].ToString());
                }));
            }
        }
    }
}
