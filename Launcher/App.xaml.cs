using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Win32;
using System.Security.Principal;
using System.Security.Permissions;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (hasAdministrativeRight)
            {
                try
                {
                    RegistryKey key = Registry.ClassesRoot.CreateSubKey("grv");
                    key.SetValue("", "URL:Groovify Song Protocol");
                    key.SetValue("URL Protocol", "");
                    key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", System.Reflection.Assembly.GetExecutingAssembly().Location + "\\MainClient.exe %1");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    MessageBox.Show("Install finished, Launcher will now open!");
                }
            }
            else
            {
                try
                {
                    if (!Registry.ClassesRoot.GetSubKeyNames().Contains("grv"))
                    {
                        MessageBox.Show("Run as admin in order to install grooveshark protocol the first run!");
                        Application.Current.Shutdown();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            base.OnStartup(e);
        }

    }
}
