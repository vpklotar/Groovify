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

using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _start_point = new Point(0, 0);
        private SciLorsGroovesharkAPI.Groove.GroovesharkClient client;
        private static MainWindow instance;

        public MainWindow()
        {
            instance = this;
            InitializeComponent();
            if (File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "GroovifyUserData.data")))
            {
                System.Diagnostics.Process.Start("MainClient.exe");
                this.Close();
                Application.Current.Shutdown();
            }

            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

            Close.MouseEnter +=Close_MouseEnter;
            Close.MouseLeave +=Close_MouseLeave;
            Min.MouseEnter +=Min_MouseEnter;
            Min.MouseLeave +=Min_MouseLeave;

            client = new SciLorsGroovesharkAPI.Groove.GroovesharkClient() { UseGZip = true };
        }

        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
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

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private int UID = 0;
        public void login()
        {
            
            /*List<String> creds = new List<string>();
            creds.Add(Username.Text);
            creds.Add(Password.Password);

            System.Threading.ThreadPool.QueueUserWorkItem(Grooveshark.login, creds);*/
            var res = client.Login(Username.Text, Password.Password);
            if (res.result.userID == 0)
            {
                updateLogin("Username or password didn't match", false, "Username or password didn't match");
            }
            else
            {
                UID = res.result.userID;
                File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "GroovifyUserData.data"));
                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "GroovifyUserData.data")))
                {
                    sw.WriteLine(this.Username.Text);
                    sw.WriteLine(Base64Encode(Password.Password));
                    sw.WriteLine(RememberMe.IsChecked == true ? 1 : 0);
                    sw.Close();
                }
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((Object o) =>
                {
                    while (!File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "GroovifyUserData.data")))
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                    instance.Dispatcher.BeginInvoke(new Action(() => {
                        updateLogin("SUCCESS", false, "", Username.Text, Password.Password);
                    }));
                }));
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

        private void writeUserData(String data, String f)
        {
            System.IO.Directory.CreateDirectory("users");
            System.IO.Directory.CreateDirectory("users/" + f);
            System.IO.StreamWriter sw = new System.IO.StreamWriter("users/" + f + "/User.data");
            sw.Write(data);
            sw.Close();
        }

        internal void updateLogin(string status, bool removeLoader, string data, String username = "", String password = "")
        {
            if (status == "SUCCESS")
            {
                System.Diagnostics.Process.Start("MainClient.exe");
                this.Close();
            }

            try
            {
                Current.Visibility = System.Windows.Visibility.Visible;
                Current.Content = status;
                //if (removeLoader) this.Dispatcher.Invoke(new Action(() => Current.Visibility = System.Windows.Visibility.Hidden));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Label_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Border_MouseUp(sender, e);
        }

        private void Label_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            login();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            login();
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            login();
        }

        private void Username_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Password_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Username.Text == "Username")
            {
                Username.Text = "";
            }
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Password.Password == "Password")
            {
                Password.Password = "";
            }
        }

        private void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Username.Text == "")
            {
                Username.Text = "Username";
            }
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Password.Password == "")
            {
                Password.Password = "Password";
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwww.grooveshark.com/");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            login();
        }

        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                login();
            }
        }
    }
}
