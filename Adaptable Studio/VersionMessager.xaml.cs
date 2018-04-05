using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Adaptable_Studio
{
    /// <summary> VersionMessager.xaml 的交互逻辑 </summary>
    public partial class VersionMessager : MetroWindow
    {
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径

        bool updated;

        //更新器下载链接
        const string updaterlink = "http://minecraft-1254149191.coscd.myqcloud.com/adaptable%20studio/MAS%20Updater.exe";

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogPath = AppPath + @"\log.txt";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (updated)
            {
                Process.Start(AppPath + @"\MAS Updater.exe");
                Log_Write(LogPath, "[Main]开始升级程序");
                IniWrite("System", "PageIndex", "-1", iniPath);
                Environment.Exit(0);
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            //设置参数
            HttpWebRequest request = WebRequest.Create(updaterlink) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            Stream stream = new FileStream(AppPath + @"\MAS Updater.exe", FileMode.Create);
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
            Close();
        }

        /// <summary> 日志文件输出 </summary>
        public static void Log_Write(string path, string information)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            { file.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + "] " + information); }
        }

        #region ini文件读写
        /// <summary> 读取配置文件(字符串, "节名", "键名", 文件路径) </summary>
        public static void IniRead(ref StringBuilder StrName, string configureNode, string key, string path)
        {
            //获取节中 键的值，存在字符串中
            //格式：GetPrivateProfileString("节名", "键名", "", 字符串, 255, 文件路径)
            GetPrivateProfileString(configureNode, key, "", StrName, 255, path);
        }

        /// <summary> 写入配置文件("节名", "键名", 键值, 文件路径) </summary>
        public static void IniWrite(string configureNode, string key, string keyValue, string path)
        {
            WritePrivateProfileString(configureNode, key, keyValue, path);
        }
        #endregion
    }
}
