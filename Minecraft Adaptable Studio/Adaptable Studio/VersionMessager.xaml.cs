using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;

namespace Adaptable_Studio
{
    /// <summary> VersionMessager.xaml 的交互逻辑 </summary>
    public partial class VersionMessager : MetroWindow
    {
        bool updated;

        //更新器下载链接
        const string updaterlink = "http://minecraft-adaptable-studio-1254149191.coscd.myqcloud.com/MAS%20Updater.exe";

        #region ini配置文件
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //定义写入函数
        //用途：若存在给定路径下的ini文件，就在其中写入给定节和键的值（若已存在此键就覆盖之前的值），若不存在ini文件，就创建该ini文件并写入。

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        //定义读入函数

        string iniPath = Environment.CurrentDirectory + @"\config.ini";//ini文件路径
        StringBuilder StrName = new StringBuilder(255);//定义字符串  
        #endregion

        public VersionMessager()
        {
            InitializeComponent();
            //控件事件
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
            ExeButton.Click += Update_Click;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (updated)
            {
                Process.Start(@".\MAS Updater.exe");
                Log_Write(LogPath, "[Main]开始升级程序");
                IniWrite("System", "PageIndex", "-1", iniPath);
                Environment.Exit(0);
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExeButton.Content = FindResource("UpdaterDownloading");
                ExeButton.IsEnabled = false;
                Thread th_updaterdownload = new Thread(new ThreadStart(delegate
                {
                    //向目标网页发送Post请求
                    Stream responseStream = WebRequest.Create(updaterlink).GetResponse().GetResponseStream();
                    //创建本地文件写入流
                    using (Stream stream = new FileStream(@".\MAS Updater.exe", FileMode.Create))
                    {
                        byte[] bArr = new byte[1024];
                        int size = responseStream.Read(bArr, 0, bArr.Length);
                        while (size > 0)
                        {
                            stream.Write(bArr, 0, size);
                            size = responseStream.Read(bArr, 0, bArr.Length);
                        }
                        stream.Close();
                        responseStream.Close();

                        updated = true;
                        Dispatcher.Invoke(new ThreadStart(delegate
                        {
                            Close();
                        }));
                    }
                }));

                th_updaterdownload.Start();
            }
            catch
            {
                ExeButton.Content = FindResource("Retry");
                Log_Write(LogPath, "[VersionMessager]更新器下载失败");
                MessageBox.Show(FindResource("UpdaterDownloadFail") as string, "Error");
            }
            finally
            {
                ExeButton.IsEnabled = true;
            }
        }
    }
}
