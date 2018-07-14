using ArmorStand.CustomControl;
using DotNetZip;
using Ionic.Zip;
using Newtonsoft.Json;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Assets;
using SharpGL.WPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Adaptable_Studio
{
    /// <summary> special_particle_Page.xaml 的交互逻辑 </summary>
    public partial class Special_particle_Page : Page
    {
        #region Define     
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径

        Json particleName;
        public static int pre_count/*预览粒子数量统计*/;

        public struct Particle
        {
            public int Id;
            public string Style;
            public string Coded;
        }
        public struct Preview
        {
            public string StyleName;
            public string[] Controls;
            public string[] ControlType;
            public string Code;
        }

        #region 控件数据声明        
        public static double[] radius = new double[32767];//半径
        public static double[] angle = new double[32767];//角度
        #endregion

        public static string Extra_style = "单粒子环绕"/*特殊样式*/, Scorename/*计分板名*/;
        public static bool[] axis = { true, false, false }/*轴心转换*/;
        public static bool ScoreSwitch = false/*计分板开关*/, SelectorSwitch = false/*选择器开关*/;
        public static bool exeTest;//OpenGL探测是否需要检测坐标偏移
        public static double[] par_distance = new double[3],
                              exe_shift = new double[3],
                              Start = new double[3],
                /*直线参数*/  del = new double[3];

        /// <summary> 特效总列表存储-存储于ComboBox </summary>
        string[] StyleName = new string[50];
        /// <summary> 队列特效信息-队列中的特效信息 </summary>
        Particle[] par = new Particle[50];
        /// <summary> 预览算法预加载-预先计算所有特效算法输出结果 </summary>
        Preview[] pre = new Preview[50];
        #region OpenGL
        public static bool preview_rot;
        public static double GL_Scale = 1.75/*缩放倍数*/;
        double[] mouse_location = new double[2];//鼠标位置
        public static double[] rot = new double[2] { 15, -60 };//X旋转角,Y旋转角        
        #region 纹理映射
        static Texture GLpar = new Texture();
        #endregion

        #region 预览视角自动环绕
        float GLmax = 2, GLnormal = 1, GLround_speed = 1.5f;
        int GLround_status;//环绕视角状态:0-停止,1-匀速,2-加速
        static DispatcherTimer Pre_Timer = new DispatcherTimer();
        #endregion
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
        #endregion        

        DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 1, 500) };//动态编译

        public Special_particle_Page()
        {
            InitializeComponent();
            timer.Tick += Coding;
            timer.Start();
        }

        /// <summary> 动态编译Timer </summary>
        private void Coding(object sender, EventArgs e)
        {
            //临时变量
            Particle[] par_mark = par;
            Preview[] pre_mark = pre;

            Thread th = new Thread(new ThreadStart(delegate
            {
                int i = 0;
                string[] mark = new string[50];//编译结果缓存
                for (int j = 0; j < 50; j++) mark[j] = String.Empty;

                foreach (var particle in par_mark)
                {
                    if (particle.Style == String.Empty) break;
                    else
                    {
                        foreach (var preview in pre_mark)
                        {
                            if (particle.Style == preview.StyleName)
                            {
                                string str1 = preview.Code;
                                //控件读取线程
                                Dispatcher.Invoke(new ThreadStart(delegate
                                {
                                    for (int param = 0; param < 50; param++)//参数遍历
                                    {
                                        if (preview.Controls[param] == null) break;

                                        //搜索UI制定控件值
                                        switch (preview.ControlType[param])
                                        {
                                            case "Value":
                                                CustomNumberBox cnb = style_edit.FindName(preview.Controls[param]) as CustomNumberBox;
                                                double value = cnb.Value;
                                                str1 = str1.Replace(preview.Controls[param] + " = <value>", preview.Controls[param] + " = " + value);
                                                break;
                                        }
                                    }
                                }));
                                //动态编译输出
                                mark[i] = Dynamic_code.Compile(str1);

                            }
                        }
                    }
                    i++;
                }
                for (int x = 0; x < 50; x++) par[x].Coded = mark[x];
            }));
            th.Start();

            //变量值归还
            par = par_mark;
            pre = pre_mark;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogPath = AppPath + @"\log.txt";
            MainWindow.Log_Write(LogPath, "[masp]粒子生成器初始化");
            #region OpenGL初始化
            OpenGL gl = OpenGLControl.OpenGL;
            gl.ClearDepth(1.0f);//设置深度缓存
            gl.Enable(OpenGL.GL_DEPTH_TEST);//启用深度测试
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);//透视修正
            gl.ShadeModel(OpenGL.GL_SMOOTH);//启用阴影平滑
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);//基于源像素alpha通道值的半透明混合函数
            try//粒子纹理绑定
            {
                GLpar.Create(gl, AppPath + @"\textures\masp\particle.bmp");
                MainWindow.Log_Write(LogPath, "[OpenGL]纹理绑定完成");
            }
            catch { MainWindow.Log_Write(LogPath, "[Error]纹理绑定失败"); }
            MainWindow.Log_Write(LogPath, "[masp]OpenGL初始化完成");
            #endregion

            #region 队列数组初始化            
            for (int i = 0; i < 50; i++)
            {
                par[i].Id = 0;
                par[i].Style = String.Empty;
                par[i].Coded = String.Empty;
                pre[i].Controls = new string[5];
                pre[i].ControlType = new string[5];
                pre[i].Code = String.Empty;
            }

            for (int i = 0; i < 32767; i++) { radius[i] = 1; }
            for (int i = 0; i < 32767; i++) { angle[i] = 20; }
            #endregion

            #region Json预读取
            StreamReader line = null;
            try
            {
                line = new StreamReader(AppPath + @"\json\masp\particle.json");
                string JSONcontent = line.ReadToEnd();
                particleName = JsonConvert.DeserializeObject<Json>(JSONcontent);
                MainWindow.Log_Write(LogPath, "[masp]json读取完成");
            }
            catch
            {
                MainWindow.Log_Write(LogPath, "[Error]json文件读取异常，文件丢失或数据已损坏");
            }//判断版本读取对应json
            line.Close();
            #endregion

            StyleFiles_Load();

            #region 预览视角旋转Timer
            Pre_Timer.Tick += Round_Tick;
            Pre_Timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            #endregion
        }

        /// <summary> 样式模板列表构建 </summary>
        private void StyleFiles_Load()
        {
            DirectoryInfo DllFolder = new DirectoryInfo(AppPath + @"\appfile\temp\masp");
            int i = 0;

            //获取模板列表
            foreach (FileInfo file in DllFolder.GetFiles("*.dll"))
            {
                try
                {
                    string DllPath = file.FullName;
                    Assembly assembly = Assembly.LoadFrom(DllPath);
                    Type type = assembly.GetType(file.Name.Replace(".dll", ""));//用类型的命名空间和名称获得类型
                    //StyleName[i] = assembly.
                    i++;
                }
                catch { }

            }




            //DirectoryInfo txtFolder = new DirectoryInfo(AppPath + @"\appfile\temp\masp");
            //int i = 0;

            ////获取模板压缩包列表
            //foreach (FileInfo file in txtFolder.GetFiles("*.zip"))
            //{
            //    StyleName[i] = file.Name.Replace(".zip", "");
            //    i++;
            //}

            try
            {
                DirectoryInfo jsonFolder = new DirectoryInfo(AppPath + @"\appfile\temp\masp");
                i = 0;
                int j = 0;
                foreach (FileInfo file in jsonFolder.GetFiles("*.zip"))
                {
                    pre[j].StyleName = file.Name.Replace(".zip", "");
                    //提取压缩包内预览算法文件路径，存入缓存
                    string str = Compress.ExtractSingleFile(file.FullName, file.Name.Replace(".zip", ".json"));
                    //从流读取缓存文本
                    using (StreamReader sr = new StreamReader(WebRequest.Create(str).GetResponse().GetResponseStream(), Encoding.UTF8))
                    {
                        string str0 = sr.ReadLine();//行读取
                        do
                        {
                            if (str0.IndexOf("#") != -1)
                            {
                                pre[j].Controls[i] = str0.Substring(str0.IndexOf("#") + 1, str0.IndexOf("$") - 1);
                                pre[j].ControlType[i] = str0.Substring(str0.IndexOf("$") + 1, str0.Length - str0.IndexOf("$") - 1);
                                i++;
                            }
                            str0 = sr.ReadLine();
                        } while (str0 != "");//所需控件读取

                        //动态代码
                        pre[j].Code = sr.ReadToEnd();
                    }
                    j++;
                }
            }
            catch { }
        }

        /// <summary> 输出控件 </summary>
        private void OutPut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (OutPutBox.OutPut)
            {
                Generate_Click(sender, e);
                OutPutBox.OutPut = false;
                OutPut output = new OutPut();
                output.Print.Text = MainWindow.result;
                output.Show();
            }
            else if (OutPutBox.Retrieval)
            {
                OutPutBox.Retrieval = false;
                OutPut output = new OutPut();
                output.Print.Text = MainWindow.result;
                output.Show();
            }
        }

        #region 列队事件
        /// <summary> 添加样式 </summary>
        private void Item_add(object sender, RoutedEventArgs e)
        {
            CheckBox NewItem = new CheckBox()
            {
                Style = (Style)(FindResource("CustomCheckBox")),
                IsChecked = true,
                Content = "<显示>  " + "样式",
                Height = 20
            };
            NewItem.Click += Item_SH;//单击事件
            NewItem.KeyDown += Item_KeyDown;
            Style_list.Items.Add(NewItem);
        }

        /// <summary> 移除样式 </summary>
        private void Item_rename(object sender, RoutedEventArgs e)
        {
            Thickness margin = new Thickness();
            try
            {
                //动态新建控件+设置属性
                TextBox t = new TextBox();
                string text = ((CheckBox)Style_list.SelectedItem).Content.ToString();
                t.Text = text.Remove(0, 6);
                Canvas.SetTop(t, 0);
                //控件显示
                style_board.Children.Add(t);
                t.Width = Style_list.Width;
                margin.Top = Style_list.ActualHeight + 10;
                margin.Left = 10;
                t.Margin = margin;
                //获取焦点
                t.Focus();
                t.SelectAll();
                //动态事件
                t.LostFocus += new RoutedEventHandler(ItemName_Changed);
                t.PreviewKeyDown += new KeyEventHandler(ItemName_Changed);
            }
            catch { }
        }

        #region 动态事件
        /// <summary> 编辑样式事件 </summary>
        private void Style_edit(object sender, SelectionChangedEventArgs e)
        {
            style_edit.Children.Clear();//清除所有控件
            if (Style_list.SelectedIndex != -1)
            {
                #region 特效列表
                ComboBox par_style = new ComboBox()
                {
                    Height = 25,
                    Width = style_edit.ActualWidth - 10,
                    Margin = new Thickness(2, 30, Margin.Right, Margin.Bottom)
                };
                //par_style.Items.Add(new TextBlock() { Text = "直线" });
                //par_style.Items.Add(new TextBlock() { Text = "圆圈" });
                //par_style.Items.Add(new TextBlock() { Text = "球体" });

                foreach (var item in StyleName)
                {
                    if (item == null) break;
                    else
                    {
                        //压缩文件提取缓存路径
                        string str = Compress.ExtractSingleFile(AppPath + @"\appfile\temp\masp\" + item + ".zip", item + ".txt");
                        string str0;
                        using (StreamReader sr = new StreamReader(WebRequest.Create(str).GetResponse().GetResponseStream(), Encoding.UTF8))
                        {
                            str0 = sr.ReadLine();//行读取
                            do
                            {
                                if (str0.IndexOf("*") != -1)
                                {
                                    str0 = str0.Substring(str0.IndexOf("*") + 1, str0.Length - 1);
                                    break;
                                }
                                str0 = sr.ReadLine();
                            } while (str0 != "");//注释读取
                        }

                        par_style.Items.Add(new TextBlock()
                        {
                            Text = item,
                            ToolTip = str0,
                            Width = par_style.Width
                        });//添加特效选项
                    }
                }//特效模板列表导入

                par_style.Text = par[Style_list.SelectedIndex].Style == String.Empty ? ((TextBlock)par_style.Items[0]).Text : par[Style_list.SelectedIndex].Style;

                par_style.Loaded += Par_style_Loaded;//重加载事件
                par_style.SelectionChanged += Par_style_Changed;//特效更改事件
                #endregion

                #region 粒子id列表
                ComboBox par_id = new ComboBox()
                {
                    Height = 25,
                    Width = style_edit.ActualWidth - 10,
                    Margin = new Thickness(2, 60, Margin.Right, Margin.Bottom)
                };
                par_id.SelectionChanged += Par_id_Changed;//id更改事件                                                     
                foreach (var item in particleName.CN) par_id.Items.Add(new TextBlock() { Text = item }); //导入json列表

                try { par_id.SelectedIndex = par[Style_list.SelectedIndex].Id; } catch { }
                #endregion

                //控件放置
                style_edit.Children.Add(par_style); Canvas.SetTop(par_style, 0);
                style_edit.Children.Add(par_id); Canvas.SetTop(par_id, 0);
            }
        }

        //样式键盘编辑
        private void Item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) Item_delete(sender, e); //删除样式
        }

        #region 数组响应事件
        #region"动态Grid行列数"
        /// <summary> 在grid中创建rowCount个height高度的分行 </summary>
        private void InitRows(int rowCount, Grid g, double height)
        {
            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(height);
                g.RowDefinitions.Add(rd);
            }
        }
        /// <summary> 在grid中创建colCount个width宽度的分列 </summary>
        private void InitColumns(int colCount, Grid g, double width)
        {
            for (int i = 0; i < colCount; i++)
            {
                ColumnDefinition rd = new ColumnDefinition();
                rd.Width = new GridLength(width);
                g.ColumnDefinitions.Add(rd);
            }
        }
        #endregion

        private void Par_style_Changed(object sender, SelectionChangedEventArgs e)
        {
            Par_style_Return(sender);
        }
        private void Par_style_Loaded(object sender, RoutedEventArgs e)
        {
            Par_style_Return(sender);
        }

        /// <summary> 特效更改事件 </summary>
        private void Par_style_Return(object sender)
        {
            ComboBox c = (ComboBox)sender;
            try { par[Style_list.SelectedIndex].Style = ((TextBlock)c.SelectedItem).Text; } catch { }

            #region 主框架生成
            int index = -1;//控件数量
            foreach (var item in style_edit.Children) index++;
            for (int i = index; i >= 0; i--)
            {
                if (style_edit.Children[i] is ComboBox) continue;
                else style_edit.Children.Remove(style_edit.Children[i]);
            }//删除旧控件

            try
            {
                string str = Compress.ExtractSingleFile(AppPath + @"\appfile\temp\masp\" + par[Style_list.SelectedIndex].Style + ".zip", par[Style_list.SelectedIndex].Style + ".txt");
                using (StreamReader sr = new StreamReader(WebRequest.Create(str).GetResponse().GetResponseStream(), Encoding.UTF8))
                {
                    string[] ControlName = new string[10];//预设控件名
                    string[] ControlType = new string[10];//预设控件类型
                    int i = 0;//控件索引
                    string str0 = sr.ReadLine();//行读取
                    do
                    {
                        if (str0.IndexOf("#") != -1)
                        {
                            ControlName[i] = str0.Substring(str0.IndexOf("#") + 1, str0.IndexOf("$") - 1);
                            ControlType[i] = str0.Substring(str0.IndexOf("$") + 1, str0.Length - str0.IndexOf("$") - 1);
                            i++;
                        }
                        str0 = sr.ReadLine();
                    } while (str0 != "");//所需控件读取

                    for (int j = 0; j <= i; j++)
                    {
                        if (ControlName[j] == null) break;
                        else
                        {
                            TextBlock tb = new TextBlock()
                            {
                                Text = ControlName[j],
                                Margin = new Thickness(2, 100 + 30 * j, Margin.Right, Margin.Bottom)
                            };//文本
                            style_edit.Children.Add(tb);

                            switch (ControlType[j])
                            {
                                case "Value":
                                    CustomNumberBox nb = new CustomNumberBox()
                                    {
                                        Value = 1,
                                        Interval = 0.1,
                                        Maximum = 32767,
                                        Minimum = 0.1,
                                        Margin = new Thickness(80, 100 + 30 * j, Margin.Right, Margin.Bottom)
                                    };
                                    style_edit.Children.Add(nb);
                                    style_edit.RegisterName(ControlName[j], nb);//注册控件名
                                    break;
                            }//参数类型                        
                        }
                    }//控件实例化
                }

            }
            catch { }
            #endregion
        }

        //半径存值
        public void radis_Changed(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            GroupBox old_g = style_edit.FindName("Style_Group") as GroupBox;
            if (old_g != null)
            {
                Grid grid = old_g.FindName("Style_Grid") as Grid;
                if (grid != null)
                {
                    CustomNumberBox r = grid.FindName("radius") as CustomNumberBox;
                    if (r != null)
                    {
                        radius[Style_list.SelectedIndex] = r.Value;
                    }
                }
            }
        }

        private void Par_id_Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            par[Style_list.SelectedIndex].Id = c.SelectedIndex;
        }
        #endregion

        //重命名事件
        private void ItemName_Changed(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            string tag = null;
            if ((bool)((CheckBox)Style_list.SelectedItem).IsChecked) { tag = "<显示>  "; }
            else { tag = "<隐藏>  "; }
            ((CheckBox)Style_list.SelectedItem).Content = tag + t.Text;
            style_board.Children.Remove(t);
        }
        private void ItemName_Changed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox t = (TextBox)sender;
                string tag = null;
                if ((bool)((CheckBox)Style_list.SelectedItem).IsChecked) { tag = "<显示>  "; }
                else { tag = "<隐藏>  "; }
                ((CheckBox)Style_list.SelectedItem).Content = tag + t.Text;
                style_board.Children.Remove(t);
            }
        }
        #endregion

        /// <summary> 样式 显示/隐藏 </summary>
        private void Item_SH(object sender, RoutedEventArgs e)
        {
            CheckBox c1 = (CheckBox)sender;
            string h = "隐藏", s = "显示", content = c1.Content.ToString();
            if ((bool)c1.IsChecked) content = content.Replace(h, s);
            else content = content.Replace(s, h);
            c1.Content = content;
        }

        /// <summary> 样式删除 </summary>
        private void Item_delete(object sender, RoutedEventArgs e)
        {
            if (Style_list.SelectedIndex != -1)
            {
                style_edit.Children.Clear();
                par[Style_list.SelectedIndex].Id = 0;
                #region"队列数组重组"            
                for (int i = Style_list.SelectedIndex; i < 49; i++)
                {
                    if (par[i].Style != String.Empty && par[i + 1].Style != String.Empty)
                        par[i].Style = par[i + 1].Style;
                    else break;
                }//特效名称
                for (int i = Style_list.SelectedIndex; i < 49; i++)
                {
                    if (par[i].Id != -1 && par[i + 1].Id != -1)
                        par[i].Id = par[i + 1].Id;
                    else break;
                } //粒子名称

                for (int i = Style_list.SelectedIndex; i < 49; i++) { radius[i] = radius[i + 1]; } //半径
                #endregion
                Style_list.SelectedIndex = 0;
                Style_list.Items.Remove(Style_list.SelectedItem);
            }
        }

        /// <summary> 样式清空 </summary>
        private void Item_clear(object sender, RoutedEventArgs e)
        {
            //队列数组归零            
            par = new Particle[32767];

            radius = new double[32767];//半径            

            style_edit.Children.Clear();
            Style_list.Items.Clear();
        }
        #endregion

        /// <summary> 生成指令并弹出检索窗体 </summary>
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            #region  参数初始化
            int score_ = 0/*计分板参数*/, count = 0/*指令生成次数*/;
            MainWindow.result = String.Empty;//指令输出
            #endregion

            #region fNBT
            MainWindow.k = 0;
            for (int i = 0; i < 10000; i++) { MainWindow.commands[i] = ""; }//NBT框架搭建数据              
            MainWindow.StructureNbt.Clear();
            #endregion

            string Selector = "", select_mark = "";
            int score_tick = 0;

            #region 算法部分
            for (int i = 0; i < 50; i++)
            {
                string particle = particleName.EN[par[i].Id];

                //switch (particle_style[i])
                //{
                //    case 1://直线
                //        if ((bool)((CheckBox)Style_list.Items[i]).IsChecked)
                //        { }
                //        break;

                //    case 2://圆圈
                //        double par_height = 0f;
                //        if ((bool)((CheckBox)Style_list.Items[i]).IsChecked)
                //            Roll(/*ref R, ref G, ref B,*/ ref MainWindow.result, ref particle, ref Selector, ref select_mark, ref radius[i], ref angle[i], ref score_tick, ref score_, ref par_height);
                //        //Draw.Particle_roll(gl, ref pre_count, /*ref R, ref G, ref B,*/  ref radius[i], ref angle[i], ref par_height);
                //        break;//环绕

                //    case 3://球体
                //        if ((bool)((CheckBox)Style_list.Items[i]).IsChecked) { }
                //        //Draw.Particle_ball(gl, ref pre_count, /*ref R, ref G, ref B,*/ ref radius[i], ref angle[i]);
                //        break;//球体                        
                //}
            }
            #endregion

            //test
            foreach (var sn in StyleName)
            {
                if (sn != null)
                {
                    //压缩文件提取缓存路径
                    string str = Compress.ExtractSingleFile(AppPath + @"\appfile\temp\masp\" + sn + ".zip", sn + ".txt");
                    using (StreamReader sr = new StreamReader(WebRequest.Create(str).GetResponse().GetResponseStream(), Encoding.UTF8))
                    {
                        string[] control = new string[10];//预设控件名
                        int i = 0;//控件索引
                        string str0 = sr.ReadLine();//行读取
                        do
                        {
                            if (str0.IndexOf("#") != -1)
                            {
                                control[i] = str0.Substring(str0.IndexOf("#") + 1, str0.IndexOf("$") - 1);
                                i++;
                            }
                            str0 = sr.ReadLine();
                        } while (str0 != "");//所需控件读取

                        //动态代码
                        string str1 = sr.ReadToEnd();
                        str1 = str1.Replace("Radius = <value>", "Radius = 1");
                        str1 = str1.Replace("Angle = <value>", "Angle = 10");
                        MainWindow.result = Dynamic_code.Compile(str1);//动态编译输出
                    }
                }
                else break;
            }

            //MainWindow.result = String.Empty;
            //string par = "<id>";
            //float pitch/*圆相关循环参数*/;
            //for (pitch = 0; pitch <= 360; pitch += 15)
            //{
            //    par_distance[0] = (float)(Math.Sin(pitch * Math.PI / 180) * 0.35);
            //    par_distance[1] = (float)(Math.Cos(pitch * Math.PI / 180) * 0.35);
            //    Add_ones(/*ref R, ref G, ref B,*/ref par, ref MainWindow.result, ref Selector);
            //}
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Restart = true;
            IniWrite("System", "PageIndex", "0", iniPath);
            Process.Start(Assembly.GetExecutingAssembly().Location);//重新开启当前程序
            Application.Current.Shutdown();//关闭当前程序
        }

        #region OpenGL绘制        
        /// <summary> OpenGL绘制主体 </summary>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = OpenGLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);//深度清除缓存
            gl.LoadIdentity();//重置当前模型观察矩阵
            gl.Translate(0, -1, -10);//设置坐标原点
            gl.Scale(GL_Scale, GL_Scale, GL_Scale);//缩放
            gl.Rotate((float)rot[0], (float)rot[1], 0f);//观察角度
            gl.ClearColor(0.5f, 0.5f, 0.5f, 0.5f);//设置清晰颜色
            OpenGL_Draw Draw = new OpenGL_Draw();
            exeTest = false;
            if ((bool)Network_bool.IsChecked) Draw.Network(gl); //网格绘制

            if ((bool)Axis_bool.IsChecked)
            {
                gl.LineWidth(1.5f);

                gl.Rotate(0F, 0F, 0F); //X坐标轴  
                gl.Color(1.0F, 0.1F, 0.1F);
                OpenGL_Draw.Draw_Axis(gl); OpenGL_reset(gl);
                gl.Rotate(0F, 0F, 90.0F); //Y坐标轴
                gl.Color(0.1F, 1.0F, 0.1F);
                OpenGL_Draw.Draw_Axis(gl); OpenGL_reset(gl);
                gl.Rotate(0F, -90.0F, 0F);//Z坐标轴                 
                gl.Color(0.1F, 0.1F, 1.0F);
                OpenGL_Draw.Draw_Axis(gl); OpenGL_reset(gl);

                gl.LineWidth(1f);
            }//坐标轴绘制

            if (SelectorSwitch) Draw.Steve_framework(gl); //Steve模型框架

            exeTest = true;
            OpenGL_reset(gl);
            gl.Color(1f, 1f, 1f);

            #region 参数重置
            pre_count = 0;//粒子数统计清空
            for (int i = 0; i < 3; i++) { par_distance[i] = 0; }
            #endregion

            //粒子绘制
            GLpar.Bind(gl);
            gl.Enable(OpenGL.GL_TEXTURE_2D);//启用2D纹理映射
            gl.Enable(OpenGL.GL_BLEND);//启用混色
            foreach (var particle in par)
            {
                if (particle.Style == String.Empty) break;
                else
                {
                    foreach (var preview in pre)
                    {
                        if (preview.StyleName == particle.Style)
                        {
                            int mark = 0;
                            string OneData;
                            for (int k = 0; k < particle.Coded.Length; k++)
                            {
                                OpenGL_reset(gl);
                                if (particle.Coded[k].ToString() == "|")
                                {
                                    OneData = particle.Coded.Substring(mark, k - mark).Replace("|", "");
                                    mark = k;
                                    par_distance[0] = double.Parse(OneData.Substring(0, OneData.IndexOf("/")));
                                    par_distance[1] = double.Parse(OneData.Substring(OneData.IndexOf("/") + 1, OneData.LastIndexOf("/") - OneData.IndexOf("/") - 1));
                                    par_distance[2] = double.Parse(OneData.Substring(OneData.LastIndexOf("/") + 1, OneData.Length - OneData.LastIndexOf("/") - 1));
                                    Draw.Par_Draw(gl, ref par_distance);
                                    pre_count++;
                                }
                            }//解析编译数据                            
                        }

                    }
                }
            }//模板坐标计算

            gl.Disable(OpenGL.GL_TEXTURE_2D);//禁用2D纹理映射
            gl.Disable(OpenGL.GL_BLEND);//禁用混色
            gl.Flush();

            paeticle_stats.Content = "粒子数:" + pre_count;
        }

        /// <summary> 坐标重置 </summary>
        public static void OpenGL_reset(OpenGL gl)
        {
            gl.LoadIdentity();// 重置当前模型观察矩阵          
            gl.Translate(0, -1, -10);
            gl.Rotate(Convert.ToSingle(rot[0]), Convert.ToSingle(rot[1]), 0F);//观察角度
            gl.Scale(GL_Scale, GL_Scale, GL_Scale);//缩放
                                                   //if (exeTest) { if (SelectorSwitch) { gl.Translate(exe_shift[0] * 10, exe_shift[1] * 10, exe_shift[2] * 10); } }
        }

        /// <summary> 鼠标滚轮缩放事件 </summary>
        private void OpenGL_Scale(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && GL_Scale < 3) GL_Scale += 0.05;
            if (e.Delta < 0 && GL_Scale > 0.5) GL_Scale -= 0.05;
            GLScale.Value = GL_Scale;
        }

        #region 视角持续旋转
        private void Round_start(object sender, MouseEventArgs e)
        {
            GLround_status = 1;
            Pre_Timer.Start();
        }

        private void Round_end(object sender, MouseEventArgs e)
        {
            GLround_status = 0;
        }

        private void Round_down(object sender, MouseButtonEventArgs e)
        {
            GLround_status = 2;
        }

        private void Round_up(object sender, MouseButtonEventArgs e)
        {
            GLround_status = 1;
        }

        private void Round_Tick(object sender, EventArgs e)
        {
            switch (GLround_status)
            {
                case 0:
                    GLround_speed -= 0.05f;
                    break;
                case 1:
                    if (GLround_speed < GLnormal) GLround_speed += 0.025f;
                    if (GLround_speed > GLnormal) GLround_speed -= 0.025f;
                    break;
                case 2:
                    if (GLround_speed < GLmax) GLround_speed += 0.025f;
                    break;
            }
            rot[1] += GLround_speed;
            if (GLround_speed <= 0) { GLround_speed = 0; Pre_Timer.Stop(); }
        }
        #endregion

        #region 预览视角旋转事件
        private void OpenGLControl_down(object sender, MouseButtonEventArgs e)
        {
            preview_rot = true;
            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;
        }

        private void OpenGLControl_up(object sender, MouseButtonEventArgs e)
        {
            preview_rot = false;
        }

        private void OpenGLControl_leave(object sender, MouseEventArgs e)
        {
            preview_rot = false;
        }

        private void OpenGLControl_rot_Change(object sender, MouseEventArgs e)
        {
            if (preview_rot)
            {
                rot[1] += (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * 180 / 460;
                rot[0] += (e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 180 / 460;
                mouse_location[0] = e.GetPosition((IInputElement)sender).X;
                mouse_location[1] = e.GetPosition((IInputElement)sender).Y;
            }
            //if (rot[0] > 90) { rot[0] = 90; }
            //if (rot[0] < -90) { rot[0] = -90; }
        }
        #endregion

        #region 预览视角缩放
        /// <summary> 缩放滑条 </summary>
        private void GLScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GL_Scale = GLScale.Value;
        }
        #endregion
        #endregion

        #region 粒子算法
        public static void Line(ref float R, ref float G, ref float B, ref string result, ref string Selector, ref string select_mark, ref int count, ref float particle_d)
        {
            #region"算法处理"
            int particle_num = 0/*粒子个数*/;
            double distance = Math.Sqrt(Math.Pow(del[0], 2) + Math.Pow(del[1], 2) + Math.Pow(del[2], 2));
            particle_d = particle_d == 0 ? 1 : particle_d;
            particle_num = (int)(distance / particle_d);//粒子数量
            for (int i = 0; i < 3; i++)
            {
                par_distance[i] = del[i] / particle_num;
                if (particle_num == 0) { par_distance[i] = 0; }
            }
            #endregion
            do
            {
                for (int i = 0; i < 3; i++)
                { Start[i] += par_distance[i]; }

                string Ones = null;
                if (SelectorSwitch) { Ones = "execute " + Selector + " ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
                //Ones += "particle " + particle_id + " ";
                if (SelectorSwitch) { Ones += "~" + Start[0].ToString("0.###") + " ~" + Start[1].ToString("0.###") + " ~" + Start[2].ToString("0.###"); }
                else { Ones += Start[0].ToString("0.###") + " " + Start[1].ToString("0.###") + " " + Start[2].ToString("0.###"); }
                //if (Colorful) { Ones += " " + R + " " + G + " " + B + " 1 1"; }
                //else { Ones += " 0 0 0 0 1"; }
                MainWindow.commands[MainWindow.k] = Ones; MainWindow.k++;
                result += Ones + "\r\n";
                count++;
            } while (count <= particle_num);
        }

        public static void Roll(/*ref float R, ref float G, ref float B,*/ ref string result, ref string particle, ref string Selector, ref string select_mark, ref double r, ref double Angle, ref int score_tick, ref int score_, ref double par_height)
        {
            double pitch/*圆相关循环参数*/;
            Score_Start(ref result, ref Selector);
            if (Extra_style == "单粒子环绕")
            {
                #region 单粒子环绕
                for (pitch = 0; pitch <= 360; pitch += Angle)
                {
                    if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
                    Roll_math(ref r, ref pitch, ref par_height);
                    Add_ones(/*ref R, ref G, ref B,*/ref particle, ref result, ref Selector);
                }
                Score_End(ref result, ref Selector);
                #endregion
            }
            else if (Extra_style == "双粒子环绕")
            {
                #region 双粒子环绕
                #region 模块一
                result += "#模块一\r\n";
                for (pitch = 0; pitch <= 180; pitch += Angle)
                {
                    if (ScoreSwitch) { if (pitch < 179) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; } }
                    Roll_math(ref r, ref pitch, ref par_height);
                    //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
                }
                #endregion
                par_distance[1] = 0;
                score_ = 0;
                result += "\r\n";
                #region 模块二
                result += "#模块二\r\n";
                for (pitch = 180; pitch <= 360; pitch += Angle)
                {
                    if (ScoreSwitch) { if (pitch < 359) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; } }
                    Roll_math(ref r, ref pitch, ref par_height);
                    //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
                }
                if (ScoreSwitch) { if (pitch < 359) { Selector_reset(ref score_, ref Selector, ref select_mark); } }
                Score_End(ref result, ref Selector);
                #endregion
                #endregion
            }
            else if (Extra_style == "螺旋延伸")
            {
                #region 螺旋延伸
                double dr = r / 73;
                for (pitch = 0; pitch <= 360; pitch += Angle)
                {
                    if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
                    Roll_math(ref r, ref pitch, ref par_height);
                    //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
                    for (int i = 90; i <= 360; i += 90)
                    {
                        par_distance[0] = (float)(Math.Sin((pitch + i) * Math.PI / 180) * r);
                        par_distance[2] = (float)(Math.Cos((pitch + i) * Math.PI / 180) * r);
                        //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
                    }
                    r -= dr;
                }
                Score_End(ref result, ref Selector);
                #endregion
            }
        }

        /// <summary> 圆圈坐标计算模块 </summary>
        public static void Roll_math(ref double r, ref double pitch, ref double par_height)
        {
            par_distance[0] = (float)(Math.Sin(pitch * Math.PI / 180) * r);
            par_distance[1] += par_height;
            par_distance[2] = (float)(Math.Cos(pitch * Math.PI / 180) * r);
        }

        public static void Ball(/*ref float R, ref float G, ref float B,*/ ref string result, ref string Selector, ref string select_mark, ref double r, ref double Angle, ref int score_tick, ref int score_)
        {
            double yaw, pitch/*圆相关循环参数*/;
            float Pi = 361, Pi2 = 181;
            if (Extra_style == "半圈周期") { float mark; mark = Pi; Pi = Pi2; Pi2 = mark; }
            Score_Start(ref result, ref Selector);
            for (yaw = 0; yaw < Pi; yaw += Angle)
            {
                if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
                result += "#\r\n";
                for (pitch = 0; pitch < Pi2; pitch += Angle)
                {
                    Ball_math(ref r, ref pitch, ref yaw);
                    Axis_Test();
                    //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
                }
                if (ScoreSwitch) { if (yaw < Pi) { Selector_reset(ref score_, ref Selector, ref select_mark); } }
            }
            score_ -= score_tick; Selector_reset(ref score_, ref Selector, ref select_mark);
            Score_End(ref result, ref Selector);
        }

        /// <summary> 球体坐标计算模块 </summary>
        public static void Ball_math(ref double r, ref double pitch, ref double yaw)
        {
            par_distance[0] = (float)(Math.Sin(pitch * Math.PI / 180) * Math.Cos(yaw * Math.PI / 180) * r);
            par_distance[1] = (float)(Math.Sin(pitch * Math.PI / 180) * Math.Sin(yaw * Math.PI / 180) * r);
            par_distance[2] = (float)(Math.Cos(pitch * Math.PI / 180) * r);
        }

        public static void test(ref int pre_count, ref double r, ref double Angle, ref double par_height)
        {
            double pitch/*圆相关循环参数*/;
            for (pitch = 0; pitch <= 360; pitch += Angle)
            {
                par_distance[0] = Convert.ToSingle((Math.Sin(pitch * Math.PI / 180) * r));
                par_distance[1] = par_height;
                par_distance[2] = Convert.ToSingle((Math.Cos(pitch * Math.PI / 180) * r));
                // if (ScoreSwitch) { score_ += score_tick; }
                // Add_ones(ref result,ref Selector);
            }
        }

        #region 共用模块
        /// <summary> 写入NBT文件缓存 </summary>
        private static void Add_ones(/*ref float R, ref float G, ref float B,*/ref string particle, ref string result, ref string Selector)
        {
            string Ones = null;
            if (SelectorSwitch) { Ones = "execute as " + Selector + " at @s run ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
            if (SelectorSwitch) { Ones += "particle " + particle + " ~" + par_distance[0].ToString("0.####") + " ~" + par_distance[1].ToString("0.####") + " ~" + par_distance[2].ToString("0.####"); }
            else { Ones += "particle " + particle + " ^" + (exe_shift[0] + par_distance[0]).ToString("0.####") + " ^" + (exe_shift[1] + par_distance[1]).ToString("0.####") + " ^" + (exe_shift[2] + par_distance[2]).ToString("0.####"); }
            //if (Colorful) { Ones += " " + R + " " + G + " " + B + " 1 0"; }
            //else { Ones += " 0 0 0 0 1"; }
            //commands[k] = Ones; k++;
            result += Ones + "\r\n";
        }

        /// <summary> 轴心判定模块 </summary>
        public static void Axis_Test()
        {
            double mark;
            if (axis[0] == true)
            {
                mark = par_distance[0];
                par_distance[0] = par_distance[2];
                par_distance[2] = mark;
            }
            else if (axis[1] == true)
            {
                mark = par_distance[2];
                par_distance[2] = par_distance[1];
                par_distance[1] = mark;
            }
        }
        #endregion
        #endregion

        #region Scoreboard
        /// <summary> 计分板模块初始 </summary>
        public static void Score_Start(ref string result, ref string Selector)
        {
            if (ScoreSwitch)
            {
                string Ones = "scoreboard players add " + Selector + " " + Scorename + " 1";
                MainWindow.commands[MainWindow.k] = Ones; MainWindow.k++;
                result += Ones + "\r\n";
            }
        }

        /// <summary> 选择器重置 </summary>
        public static void Selector_reset(ref int score_, ref string Selector, ref string select_mark)
        {
            Selector = select_mark == null ? "@p" : select_mark;
            int Start_len = Selector.IndexOf("["), End_len = Selector.IndexOf("]");
            string Replace_text;
            if (Start_len == -1)
                Selector += "[score_" + Scorename + "_min=" + score_ + ",score_" + Scorename + "=" + score_ + "]";
            else if (End_len > Start_len)
                Selector = Selector.Replace("[", "[score_" + Scorename + "_min=" + score_ + ",score_" + Scorename + "=" + score_ + "<>");
            Replace_text = End_len - Start_len > 1 ? "," : null;
            Selector = Selector.Replace("<>", Replace_text);
        }

        /// <summary> 计分板模块结束 </summary>
        public static void Score_End(ref string result, ref string Selector)
        {
            if (ScoreSwitch)
            {
                string Ones = "scoreboard players set " + Selector + " " + Scorename + " 0";
                MainWindow.commands[MainWindow.k] = Ones; MainWindow.k++;
                result += Ones;
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