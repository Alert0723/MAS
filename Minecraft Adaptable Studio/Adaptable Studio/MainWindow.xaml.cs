using fNbt;
using MahApps.Metro.Controls;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.PublicControl;

namespace Adaptable_Studio
{
    /// <summary> xaml 的交互逻辑 </summary>
    public partial class MainWindow : MetroWindow
    {
        public static string LogPath = "log.txt";//程序根目录 日志文件路径

        string version = "Version:" + ConfigurationManager.AppSettings["MainVersion"];//当前版本号
        string updatelog = ConfigurationManager.AppSettings["UpdateLog"]; //更新日志链接

        public static int PageIndex = -1;//页面读取值
        public static bool Guiding = true;//引导状态
        public static string result = "";//指令结果

        #region 设置
        /// <summary> 程序英汉显示切换 </summary>
        public static bool _langCN = true;
        /// <summary> 启动时是否开启引导 </summary>
        public static bool Guidance = true;
        #endregion

        public MainWindow()
        {
            //静态资源初始化
            Main = this;
            vmsg = new VersionMessager();
            Page_menu = new menu_Page();
            Page_masp = new Special_particle_Page();
            Page_masc = new Armor_stand_Page();

            InitializeComponent();
        }

        /// <summary> 日志文件输出 </summary>
        public static void Log_Write(string path, string information)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.WriteLine("["
                    + DateTime.Now.Hour
                    + ":" + DateTime.Now.Minute
                    + ":" + DateTime.Now.Second
                    + "." + DateTime.Now.Millisecond
                    + "] " + information);
            }
        }

        /// <summary> 终止外部进程 </summary>
        void KillProcess(string processName, bool output)
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
        void Main_Loaded(object sender, RoutedEventArgs e)
        {
            File.Delete(LogPath);//清除日志文件
            Log_Write(LogPath, "[Main]全局初始化");

            //删除更新文件
            KillProcess("MAS Updater", false);
            File.Delete("MAS Updater.exe");

            try
            {
                StreamReader test = new StreamReader(WebRequest.Create("http://www.mcbbs.net/thread-580119-1-1.html").GetResponse().GetResponseStream(), Encoding.UTF8);
                WebView.Navigate(new Uri("http://p9fi3mtgy.bkt.clouddn.com/MAS-Stat.html"));
                Log_Write(LogPath, "[Main]测试访问成功");
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

            Log_Write(LogPath, "[Main]配置文件初始化完成");

            NewVerTest();//Version Testing

            _NavigationFrame.Navigate(Page_menu);//page读取
        }

        /// <summary> 窗体关闭 </summary>
        void MainClosed(object sender, EventArgs e)
        {
            Log_Write(LogPath, "[Main]正常关闭");
            IniWrite("System", "PageIndex", "-1", iniPath);
            Environment.Exit(0);
        }

        /// <summary> 最新版本信息检测 & 新版本信息弹出 </summary>
        void NewVerTest()
        {
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
                            Log_Write(LogPath, "[Main]版本检测完成");
                        }

                        _ = Dispatcher.Invoke(new ThreadStart(delegate
                          {
                              //校验版本号
                              if (version != NewVerStr)
                              {
                                  //主页检测提示
                                  VersionText.Content = FindResource("FindNewVer");
                                  VerPath.Data = FindResource("Icon.Warning") as Geometry;
                                  VerPath.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                                  //弹窗提示
                                  vmsg.Version.Content = NewVer;
                                  vmsg.UpdateLog.Text = Infor;
                                  vmsg.Show();
                              }
                              else
                              {
                                  VersionText.Content = FindResource("LatestVer");
                                  VerPath.Data = FindResource("Icon.Check.Round") as Geometry;
                                  VerPath.Fill = new SolidColorBrush(Color.FromRgb(0, 200, 0));
                              }
                          }));//Version Messager
                    }
                    catch
                    {
                        Dispatcher.Invoke(new ThreadStart(delegate
                        {
                            VersionText.Content = FindResource("SearchNewVerErr");
                        }));
                        Log_Write(LogPath, "[Main]获取版本信息失败");
                    }
                }));
                th.Start();
            }

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

    }
}
