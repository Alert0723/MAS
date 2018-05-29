using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adaptable_Studio
{
    /// <summary> Option_Page.xaml 的交互逻辑 </summary>
    public partial class Option_Page : Page
    {
        string AppPath = Environment.CurrentDirectory;//应用程序根目录

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

        public Option_Page()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IniRead(ref StrName, "System", "PageIndex", iniPath);
                MainWindow.PageIndex = int.Parse(StrName.ToString());
            }
            catch
            {
                IniWrite("System", "PageIndex", "0", iniPath);
            }//Page检测

            if (MainWindow.PageIndex > 0)
            {
                MainWindow.Restart = true;
                Process.Start(Assembly.GetExecutingAssembly().Location); //重新启动当前程序
                Application.Current.Shutdown();//关闭当前程序
            }
            else NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Option_load();
        }

        private void Lang_Save(object sender, RoutedEventArgs e)
        {
            MainWindow._langCN = (bool)Lang_CN.IsChecked;//语言
            //语言文件初始化
            ResourceDictionary dict = new ResourceDictionary();
            if (MainWindow._langCN)
                dict.Source = new Uri(@"lang\CN.xaml", UriKind.Relative);
            else
                dict.Source = new Uri(@"lang\EN.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = dict;//资源赋值

            MainWindow.IniWrite("System", "Lang", MainWindow._langCN.ToString(), iniPath);
        }

        private void Option_Save(object sender, MouseEventArgs e)
        {
            Option_save();
        }

        private void Option_Save(object sender, MouseButtonEventArgs e)
        {
            Option_save();
        }

        private void Option_load()
        {
            //语言
            if (MainWindow._langCN) Lang_CN.IsChecked = true;
            else Lang_EN.IsChecked = true;

            //引导
            if (MainWindow.Guidance) Guidance_on.IsChecked = true;
            else Guidance_off.IsChecked = true;

            Max_length.Value = MainWindow.Max_length;//结构最大长度
            Horizontal.IsChecked = !MainWindow.Portrait;//纵横
            Flat_structure.IsChecked = MainWindow.Flat;//是否平铺
        }

        private void Option_save()
        {
            MainWindow.Guidance = (bool)Guidance_on.IsChecked;//引导开关

            MainWindow.Max_length = (int)Max_length.Value;//结构最大长度
            MainWindow.Portrait = (bool)Vertical.IsChecked;//纵横
            MainWindow.Flat = (bool)Flat_structure.IsChecked;//是否平铺
            #region"ini写入"
            MainWindow.IniWrite("System", "Guidance", MainWindow.Guidance.ToString(), iniPath);//引导

            MainWindow.IniWrite("NbtStructures", "MaxLength", MainWindow.Max_length.ToString(), iniPath);//结构
            MainWindow.IniWrite("NbtStructures", "Portrait", MainWindow.Portrait.ToString(), iniPath);//纵横状态
            MainWindow.IniWrite("NbtStructures", "Flat", MainWindow.Flat.ToString(), iniPath);//平铺
            #endregion
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