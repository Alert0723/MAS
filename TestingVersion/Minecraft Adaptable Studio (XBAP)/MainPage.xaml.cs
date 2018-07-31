using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minecraft_Adaptable_Studio__XBAP_
{
    /// <summary> MainPage.xaml 的交互逻辑 </summary>
    public partial class MainPage : Page
    {
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径

        const string version = "Version:0.3.9.0 Alpha";//当前版本号
        public static bool _langCN = true;//汉英切换
        public static int PageIndex = -1;//页面读取值
        public static bool Restart = false;//重启判定
        public static bool Guidance = true;//启动引导
        public static bool Guiding = true;

        public static string result = "";//指令结果

        //在线更新日志链接
        const string updatelog = "http://minecraft-adaptable-studio-1254149191.coscd.myqcloud.com/update.log";

        #region fNBT
        public static NbtCompound StructureNbt = new NbtCompound("");//文件主框架        
        public static long k;//命令序列
        public static string[] commands = new string[10000];//命令数组
        #endregion

        #region 设置
        //结构
        public static int Max_length = 16;//最大长度;
        public static bool Portrait = true/*是否纵向*/,
                           Flat/*是否平铺结构*/;

        #endregion

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

        public static void Log_Write(string path, string information)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            { file.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + "] " + information); }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void MASC(object sender, RoutedEventArgs e)
        {
            //IniWrite("System", "PageIndex", "3", iniPath);
            //MainWindow.PageIndex = 3;
            NavigationService.Navigate(new Uri("pages/armor_stand_Page.xaml", UriKind.Relative));
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
