using fNbt;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Adaptable_Studio
{
    /// <summary> MainWindow.xaml 的交互逻辑 </summary>
    public partial class MainWindow : MetroWindow
    {
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径

        const string version = "Version:0.3.9.0 Alpha";//当前版本号
        public static bool _langCN = true;//汉英切换
        public static int PageIndex = -1;//页面读取值
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

        #region ini配置文件s
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

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary> 日志文件输出 </summary>
        public static void Log_Write(string path, string information)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            { file.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + "] " + information); }
        }

        /// <summary> 终止外部进程 </summary>
        private void KillProcess(string processName, bool output)
        {
            try
            {
                Process[] thisproc = Process.GetProcessesByName(processName);
                //thisproc.lendth:名字为进程总数
                if (thisproc.Length > 0)
                {
                    for (int i = 0; i < thisproc.Length; i++)
                    {
                        if (!thisproc[i].CloseMainWindow()) thisproc[i].Kill();//尝试关闭进程 释放资源,否则强制关闭                        
                    }
                }
                else if (output) MessageBox.Show("进程关闭失败!", processName);
            }
            catch { MessageBox.Show("结束进程出错!", processName); }
        }

        #region Main
        /// <summary> 窗体加载 </summary>
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            LogPath = AppPath + @"\log.txt";
            File.Delete(LogPath);//清除日志文件
            Log_Write(LogPath, "[Main]全局初始化");

            //删除更新文件
            KillProcess("MAS Updater", false);
            File.Delete(AppPath + @"\MAS Updater.exe");

            try
            {
                StreamReader test = new StreamReader(WebRequest.Create("http://www.mcbbs.net/thread-580119-1-1.html").GetResponse().GetResponseStream(), Encoding.UTF8);
                WebView.Navigate(new Uri("http://p9fi3mtgy.bkt.clouddn.com/MAS-Stat.html"));
            }
            catch { Log_Write(LogPath, "[Main]测试访问失败"); }//测试访问

            try
            {
                IniRead(ref StrName, "System", "PageIndex", iniPath);
                PageIndex = int.Parse(StrName.ToString());
            }
            catch
            {
                IniWrite("System", "PageIndex", "0", iniPath);
            }//Page Testing

            try
            {
                IniRead(ref StrName, "System", "Lang", iniPath);
                _langCN = bool.Parse(StrName.ToString());
            }
            catch
            {
                IniWrite("System", "Lang", true.ToString(), iniPath);
            }//Language
            //语言文件初始化
            ResourceDictionary dict = new ResourceDictionary();
            if (_langCN)
                dict.Source = new Uri(@"lang\CN.xaml", UriKind.Relative);
            else
                dict.Source = new Uri(@"lang\EN.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = dict;//资源赋值

            try
            {
                IniRead(ref StrName, "System", "Guidance", iniPath);
                Guidance = bool.Parse(StrName.ToString());

                if (PageIndex == -1)
                {
                    Guiding = Guidance;
                }
                else
                {
                    IniRead(ref StrName, "System", "Guiding", iniPath);
                    Guiding = bool.Parse(StrName.ToString());
                }
            }
            catch
            {
                IniWrite("System", "Guidance", Guidance.ToString(), iniPath);
                IniWrite("System", "Guiding", Guiding.ToString(), iniPath);
            }//引导

            try
            {
                IniRead(ref StrName, "NbtStructures", "MaxLength", iniPath);//结构最大长度
                Max_length = int.Parse(StrName.ToString());
                IniRead(ref StrName, "NbtStructures", "Portrait", iniPath);//纵横状态
                Portrait = bool.Parse(StrName.ToString());
                IniRead(ref StrName, "NbtStructures", "Flat", iniPath);//平铺
                Flat = bool.Parse(StrName.ToString());
            }
            catch
            {
                IniWrite("NbtStructures", "MaxLength", Max_length.ToString(), iniPath);//结构
                IniWrite("NbtStructures", "Portrait", Portrait.ToString(), iniPath);//纵横状态
                IniWrite("NbtStructures", "Flat", Flat.ToString(), iniPath);//平铺                                
            } //NBT文件结构

            Log_Write(LogPath, "[Main]配置文件初始化完成");

            if (PageIndex == -1)
            {
                Thread th = new Thread(new ThreadStart(delegate
                {
                    string NewVerStr, NewVer = "", Infor = "";
                    try //读取网页流到末尾，即使用正则表达式从网页流中提取，没有则为空
                    {
                        using (StreamReader sr = new StreamReader(WebRequest.Create(updatelog).GetResponse().GetResponseStream(), Encoding.UTF8))
                        {
                            NewVerStr = sr.ReadLine();
                            int strl = ("Version:").Length;
                            NewVer = NewVerStr.Substring(strl, NewVerStr.Length - strl);//读取主程序内容
                            Infor = sr.ReadToEnd();//读取剩余内容
                            sr.Close(); //关闭流
                            Log_Write(LogPath, "[Main]版本检测完成");
                        }

                        Dispatcher.Invoke(new ThreadStart(delegate
                        {
                            if (version != NewVerStr)
                            {
                                VersionMessager vm = new VersionMessager();
                                vm.VersionText.Text += NewVer;
                                vm.UpdateLog.Text = Infor;
                                vm.Show();
                            }
                        }));//Version Messager
                    }
                    catch { Log_Write(LogPath, "[Main]获取新版本信息失败"); }
                }));
                th.Start();
            }//Version Testing

            _NavigationFrame.Navigate(new menu_Page());//page读取
        }

        /// <summary> 窗体关闭 </summary>
        private void MainClosed(object sender, EventArgs e)
        {
            Log_Write(LogPath, "[Main]正常关闭");
            IniWrite("System", "PageIndex", "-1", iniPath);
            Environment.Exit(0);
        }
        #endregion

        #region TitleBar        
        /// <summary> 功能页面引导 </summary>
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Guiding = !Guiding;
            IniWrite("System", "Guiding", Guiding.ToString(), iniPath);
            Page PageContent = (Page)_NavigationFrame.Content;
            switch (PageIndex)
            {
                default://Menu                    
                    break;
                case 3://MAS.C
                    try
                    {
                        if (Guiding)
                        {
                            PageContent.FindChild<Grid>("Settings_guide").Visibility = Visibility.Visible;
                            PageContent.FindChild<Grid>("Timeaxis_guide").Visibility = Visibility.Visible;
                        }
                        else
                        {
                            PageContent.FindChild<Grid>("Settings_guide").Visibility = Visibility.Hidden;
                            PageContent.FindChild<Grid>("Timeaxis_guide").Visibility = Visibility.Hidden;
                        }
                    }
                    catch { }
                    break;
                case 16://MAS.P

                    break;
            }//page读取
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            _NavigationFrame.Navigate(new Option_Page());
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            _NavigationFrame.Navigate(new About_Page());
        }
        #endregion

        #region ini Read & Write
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
