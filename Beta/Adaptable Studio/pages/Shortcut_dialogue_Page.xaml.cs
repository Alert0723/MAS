using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Adaptable_Studio
{
    /// <summary> shortcut_dialogue_Page.xaml 的交互逻辑 </summary>
    public partial class Shortcut_dialogue_Page : Page
    {
        string LogPath;//日志文件路径

        bool[] Marked = new bool[32767];//行标记存储

        int lines, list_lines;//文本行数

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

        public Shortcut_dialogue_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogPath = @".\log.txt";
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Plot = new OpenFileDialog
            {
                Title = "Open TXT",
                Filter = "TXT File(*.txt)|*.txt"
            };

            //载入
            if ((bool)Plot.ShowDialog())
            {
                //文件名检测
                string filename = Plot.FileName;

                if (filename != null)
                {
                    //读取剧情文件
                    MainWindow.Log_Write(LogPath, "[masd]正在读取txt文件");
                    try
                    {
                        //逐行存储
                        using (var sr = new StreamReader(filename))
                        {
                            string mark;
                            while ((mark = sr.ReadLine()) != null)
                            {
                                //添加行数据
                                TextBlock dialogue = new TextBlock { Text = mark };

                                dialogue.KeyDown += Dialugue_delete;
                                Dialogue_List.Items.Add(dialogue);

                                //创建对应行参数
                                TextBlock extra = new TextBlock { Text = "{null}" };

                                extra.KeyDown += Dialugue_delete;
                                Extra_List.Items.Add(extra);
                                lines++;
                            }
                        }
                        list_lines = lines;//列表文本行数保存
                        MainWindow.Log_Write(LogPath, "[masd]txt读取成功");
                    }
                    catch { MainWindow.Log_Write(LogPath, "[masd]txt读取失败"); }
                }
                else return;
            }

        }

        /// <summary> 移除行数据 </summary>
        private void Dialugue_delete(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Dialogue_List.Items.Remove(Dialogue_List.Items[Dialogue_List.SelectedIndex]);
                Extra_List.Items.Remove(Extra_List.Items[Extra_List.SelectedIndex]);
            }
        }

        #region 基础格式编辑
        private void MarkOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Marked[Dialogue_List.SelectedIndex] = true;
                ((TextBlock)Dialogue_List.SelectedItem).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                ((TextBlock)Dialogue_List.SelectedItem).FontWeight = FontWeights.Bold;
            }
            catch { }
        }

        private void DlelteOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = Dialogue_List.SelectedIndex; i < list_lines; i++)
                {
                    Marked[i] = Marked[i + 1];
                    Marked[i + 1] = false;
                }
                int a = Dialogue_List.SelectedIndex;
                Dialogue_List.Items.Remove(Dialogue_List.Items[a]);
                Extra_List.Items.Remove(Extra_List.Items[a]);
                list_lines--;
            }
            catch { }
        }

        private void DeleteMark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < list_lines; i++) { Marked[i] = false; }
                for (int i = 0; i < list_lines; i++)
                {
                    while (((TextBlock)Dialogue_List.Items[i]).FontWeight == FontWeights.Bold)
                    {
                        Dialogue_List.Items.Remove(Dialogue_List.Items[i]);
                        Extra_List.Items.Remove(Extra_List.Items[i]);
                        list_lines--;
                    }
                }
            }
            catch { }
        }

        private void OutPut_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog Plot = new SaveFileDialog { Title = "Save", Filter = "TXT File(*.txt)|*.txt", FileName = "PlotFile" };

            //保存输出
            if ((bool)Plot.ShowDialog())
            {
                //文件名检测
                string PlotName = Plot.FileName;
                if (PlotName == null) PlotName = "PlotFile";

                using (var sw = new StreamWriter(PlotName))
                {
                    string str;
                    int i = 0;
                    foreach (var item in Dialogue_List.Items)
                    {
                        string extrMark = ((TextBlock)Extra_List.Items[i]).Text;
                        str = "tellraw @a {\"text\":\"" + ((TextBlock)item).Text + "\",\"color\":\"null\"}";
                        str = str.Replace("null", extrMark.Substring(1, extrMark.IndexOf("}") - 1));
                        i++;
                        sw.WriteLine(str);
                    }
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Dialogue_List.Items.Clear();
            Extra_List.Items.Clear();
            lines = 0;
            list_lines = 0;
            for (int i = 0; i < 32767; i++) { Marked[i] = false; }
        }

        private void Clear_mark_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < list_lines; i++)
            {
                ((TextBlock)Dialogue_List.Items[i]).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                ((TextBlock)Dialogue_List.Items[i]).FontWeight = FontWeights.Regular;
            }
            for (int i = 0; i < 32767; i++) { Marked[i] = false; }
        }
        #endregion

        #region 高级格式编辑
        /// <summary> 标记操作func(索引初位,索引末位,初位长度,截取初位,截取末位) </summary>
        private void Format(int index0, int index1, int length, string start, string end, ref string str)
        {
            index0 = format_input.Text.IndexOf(start);
            index1 = format_input.Text.IndexOf(end);
            str = format_input.Text.Substring(index0 + length, index1 - length);
        }

        /// <summary> 格式编辑事件 </summary>
        private void Format_execute_Click(object sender, RoutedEventArgs e)
        {
            int index0 = 0, index1 = 0;//字符索引
            string str = "";//索引定位字段

            if (format_input.Text.IndexOf(">") != -1)//替换字符
            {
                string str0, str1;
                index0 = format_input.Text.IndexOf(">");
                str0 = format_input.Text.Substring(0, index0);
                str1 = format_input.Text.Substring(index0 + 1, format_input.Text.Length - index0 - 1);
                Format_replace(ref Dialogue_List, str0, str1);
            }
            else if (format_input.Text.IndexOf("^[") != -1)//标记行
            {
                Format(index0, index1, 2, "^[", "]", ref str);
                Format_mark(ref Dialogue_List, str);
            }
            else if (format_input.Text.IndexOf("^c[") != -1)//设置标记行颜色
            {
                Format(index0, index1, 3, "^c[", "]", ref str);
                Format_color(ref Dialogue_List, ref Extra_List, str);
            }
        }

        /// <summary> 字段替换 </summary>
        private void Format_replace(ref ListBox list, string str0, string str1)
        {
            for (int i = 0; i < list_lines; i++)
            {
                string mark = ((TextBlock)list.Items[i]).Text;
                ((TextBlock)list.Items[i]).Text = mark.Replace(str0, str1);
            }
        }

        /// <summary> 标记行 </summary>
        private void Format_mark(ref ListBox list, string str)
        {
            for (int i = 0; i < list_lines; i++)
            {
                string mark = ((TextBlock)list.Items[i]).Text;
                if (mark.IndexOf(str) != -1)
                {
                    Marked[i] = true;
                    ((TextBlock)list.Items[i]).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    ((TextBlock)list.Items[i]).FontWeight = FontWeights.Bold;
                }
            }
        }

        /// <summary> 标记行颜色 </summary>
        private void Format_color(ref ListBox list0, ref ListBox list1, string str)
        {
            for (int i = 0; i < list_lines; i++)
            {
                string mark0 = ((TextBlock)list0.Items[i]).Text;//文本列表
                string mark1 = ((TextBlock)list1.Items[i]).Text;//文本对应参数                
                if (Marked[i])
                {
                    mark1 = mark1.Replace(mark1.Substring(1, mark1.IndexOf("}") - 1), str);
                    ((TextBlock)list1.Items[i]).Text = mark1;
                }

            }
        }
        #endregion

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