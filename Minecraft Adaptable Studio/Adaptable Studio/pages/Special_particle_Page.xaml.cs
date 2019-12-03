using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;
using static Adaptable_Studio.PublicControl;

namespace Adaptable_Studio
{
    /// <summary> special_particle_Page.xaml 的交互逻辑 </summary>
    public partial class Special_particle_Page : Page
    {
        #region Define
        Json particleName;
        public static int pre_count/*预览粒子数量统计*/;

        public double[] ParticlePosition = new double[3];//粒子坐标

        /// <summary> dll列表 </summary>
        public List<string> StyleList = new List<string>();
        /// <summary> 列表项对应特效样式 </summary>
        public List<int> StyleType = new List<int>();
        /// <summary> 特效样式对应粒子 </summary>
        public List<int> StyleParticle = new List<int>();

        /// <summary> 列队控件uid,dll控件集缓存 </summary>
        public List<IDictionary<int, object>> ControlValue = new List<IDictionary<int, object>>();

        #region Viewport3D
        double CameraRadius = 4;//摄像机半径(相对于原点)
        double[] CameraRot = new double[2] { 15, 60 };//水平旋转角,竖直旋转角(相对于原点)
        double[] CameraLookAtPoint = new double[3] { 0, 0, 0 };//摄像机视点
        double[] mouse_location = new double[2];//鼠标位置
        #region 预览视角自动环绕
        float Round_max = 2, Round_normal = 1, Round_speed = 1.5f;
        int Round_status;//环绕视角状态:0-停止,1-匀速,2-加速
        static DispatcherTimer Pre_Timer = new DispatcherTimer();
        #endregion
        #endregion

        #endregion

        public Special_particle_Page()
        {
            InitializeComponent();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log_Write(LogPath, "==========MASP==========");
            Log_Write(LogPath, "环境初始化");
            #region Viewport3D
            Log_Write(LogPath, "Viewport3D初始化");
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
            #endregion

            //Json预读取
            Json.Deserialize(@"json\masp\particle.json", ref particleName);

            StyleFiles_Load("Class", "StyleName", true);

            #region 预览视角旋转Timer
            Pre_Timer.Tick += Round_Tick;
            Pre_Timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            #endregion
        }

        /// <summary> 样式模板列表构建 </summary>
        /// <param name="ClassName">获取的类名</param>
        /// <param name="MethodName">获取方法名</param>
        /// <param name="AddToList">是否将返回结果加入dll索引列表</param>
        void StyleFiles_Load(string ClassName, string MethodName, bool AddToList = false)
        {
            Log_Write(LogPath, "样式模板列表构建");
            try
            {
                //获取dll列表
                foreach (FileInfo file in new DirectoryInfo(@".\appfile\temp\masp").GetFiles("*.dll"))
                {
                    string DllPath = file.FullName;
                    Assembly assem = Assembly.LoadFile(DllPath);
                    Type[] tys = assem.GetTypes();//得到所有的类型名，然后遍历，通过类型名字来区别

                    foreach (Type ty in tys)//获取类名
                    {
                        if (ty.Name == ClassName)
                        {
                            ConstructorInfo Constructor = ty.GetConstructor(Type.EmptyTypes);//获取不带参数的构造函数
                            object ClassObject = Constructor.Invoke(new object[] { });//获取类的实例

                            //执行类class的方法
                            MethodInfo mi = ty.GetMethod("");
                            mi = ty.GetMethod(MethodName);

                            object method_result = mi.Invoke(ClassObject, null);
                            if (AddToList) StyleList.Add((string)method_result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log_Write(LogPath, "样式模板列表构建失败：" + ex);
            }
        }

        /// <summary> 输出控件 </summary>
        void OutPut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (OutPutBox.OutPut)
            {
                Generate_Click(sender, e);
                OutPutBox.OutPut = false;
                OutPut output = new OutPut();
                output.Print.Text = result;
                output.Show();
            }
            else if (OutPutBox.Retrieval)
            {
                OutPutBox.Retrieval = false;
                OutPut output = new OutPut();
                output.Print.Text = result;
                output.Show();
            }
        }

        #region 列队事件
        /// <summary> 添加样式 </summary>
        void Item_add(object sender, RoutedEventArgs e)
        {
            //承载显示选项与命名的主面板
            WrapPanel NewItem = new WrapPanel()
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                Height = 20,
                Tag = "true"
            };

            //显示/隐藏
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path()
            {
                Name = "path",
                Width = 16,
                Height = 16,
                Margin = new Thickness(2),
                Stretch = Stretch.Uniform,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Data = (Geometry)FindResource("Icon.EyeOn"),
            };
            path.MouseLeftButtonDown += Item_SH;//显示/隐藏 切换事件
            NewItem.Children.Add(path);

            //命名
            TextBlock tb = new TextBlock()
            {
                Margin = new Thickness(10, 2, 0, 0),
                Text = "新样式",
                Height = 20
            };
            NewItem.Children.Add(tb);
            Style_list.Items.Add(NewItem);

            StyleType.Add(new int());
            StyleParticle.Add(new int());
            ControlValue.Add(new Dictionary<int, object>());
        }

        /// <summary> 移除样式 </summary>
        void Item_rename_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = new Thickness();
            try
            {
                string text = string.Empty;
                foreach (var item in ((WrapPanel)Style_list.SelectedItem).Children)
                {
                    if (item is TextBlock)
                        text = ((TextBlock)item).Text;
                }

                //动态新建控件+设置属性
                TextBox t = new TextBox() { Text = text };
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
        void Style_edit(object sender, SelectionChangedEventArgs e)
        {
            style_edit.Children.Clear();//清除所有控件
            int index = Style_list.SelectedIndex;
            if (index != -1)
            {
                #region 特效列表
                ComboBox par_style = new ComboBox()
                {
                    Height = 25,
                    Width = style_edit.ActualWidth - 10,
                    Margin = new Thickness() { Top = 35, Left = 5 },
                    SelectedIndex = StyleType[index]
                };

                par_style.Items.Add(new TextBlock() { Text = "< Style >" });
                foreach (string str in StyleList) par_style.Items.Add(new TextBlock() { Text = str });

                par_style.Loaded += Par_style_Loaded;//重加载事件
                par_style.SelectionChanged += Par_style_Changed;//特效更改事件
                #endregion

                #region 粒子id列表
                ComboBox par_id = new ComboBox()
                {
                    Height = 25,
                    Width = style_edit.ActualWidth - 10,
                    Margin = new Thickness() { Top = 10, Left = 5 },
                    SelectedIndex = StyleParticle[index]
                };
                par_id.DropDownOpened += Par_id_DropDownOpened;
                par_id.DropDownClosed += Par_id_DropDownClosed;

                int i = 0;//json列表 索引值
                foreach (var item in particleName.CN)
                {
                    //导入json列表 英文
                    WrapPanel wp = new WrapPanel() { MaxWidth = 750 };
                    wp.Children.Add(new TextBlock() { Text = particleName.EN[i] });
                    par_id.Items.Add(wp);
                    i++;
                }
                #endregion

                //Add
                style_edit.Children.Add(par_style); Canvas.SetTop(par_style, 0);
                style_edit.Children.Add(par_id); Canvas.SetTop(par_id, 0);
            }
        }

        /// <summary> 下拉列表-重写内容为id+中文注释 </summary>
        void Par_id_DrawENCN(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            int i = 0;//json列表 索引值
            foreach (var item in c.Items)
            {
                //导入json列表
                //英文名 + 中文注释
                if (i > 0 && item is WrapPanel)
                {
                    ((WrapPanel)item).Children.Clear();
                    ((WrapPanel)item).Children.Add(new TextBlock() { Text = particleName.EN[i] });
                    ((WrapPanel)item).Children.Add(new TextBlock()
                    {
                        Text = "  (" + particleName.CN[i] + ")",
                        Foreground = new SolidColorBrush(Color.FromArgb(100, 220, 220, 220))
                    });//粒子介绍
                }
                i++;
            }
        }

        /// <summary> 下拉列表-重写内容为纯id </summary>
        void Par_id_DrawEN(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            int index = c.SelectedIndex;//列表选项索引缓存
            if (index == -1) index = 0;
            StyleParticle[Style_list.SelectedIndex] = index;//全局粒子列表 索引记录

            //导入json列表 英文
            int i = 0;//json列表 索引值
            foreach (var item in c.Items)
            {
                if (item is WrapPanel)
                {
                    ((WrapPanel)item).Children.Clear();
                    ((WrapPanel)item).Children.Add(new TextBlock() { Text = particleName.EN[i] });
                }
                i++;
            }
        }

        /// <summary> 粒子ID菜单打开事件 </summary>
        void Par_id_DropDownOpened(object sender, EventArgs e)
        {
            Par_id_DrawENCN(sender, e);
        }
        /// <summary> 粒子ID菜单关闭事件 </summary>
        void Par_id_DropDownClosed(object sender, EventArgs e)
        {
            Par_id_DrawEN(sender, e);
        }

        #region 数组响应事件
        #region"动态Grid行列数"
        /// <summary> 在grid中创建rowCount个height高度的分行 </summary>
        void InitRows(int rowCount, Grid g, double height)
        {
            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(height);
                g.RowDefinitions.Add(rd);
            }
        }
        /// <summary> 在grid中创建colCount个width宽度的分列 </summary>
        void InitColumns(int colCount, Grid g, double width)
        {
            for (int i = 0; i < colCount; i++)
            {
                ColumnDefinition rd = new ColumnDefinition();
                rd.Width = new GridLength(width);
                g.ColumnDefinitions.Add(rd);
            }
        }
        #endregion

        void Par_style_Changed(object sender, SelectionChangedEventArgs e)
        {
            Par_style_Return(sender);
        }
        void Par_style_Loaded(object sender, RoutedEventArgs e)
        {
            Par_style_Return(sender);
        }
        //↓
        /// <summary> 特效更改事件 </summary>
        void Par_style_Return(object sender)
        {
            ComboBox c = (ComboBox)sender;
            StyleType[Style_list.SelectedIndex] = c.SelectedIndex;

            int index = -1;//控件数量
            foreach (var item in style_edit.Children) index++;
            for (int i = index; i >= 0; i--)
            {
                if (style_edit.Children[i] is ComboBox)
                    continue;
                else
                    style_edit.Children.Remove(style_edit.Children[i]);
            }//删除旧控件 (两个ComboBox除外)

            try
            {
                //获取dll列表
                foreach (FileInfo file in new DirectoryInfo(@".\appfile\temp\masp").GetFiles("*.dll"))
                {
                    string DllPath = file.FullName;
                    Assembly assem = Assembly.LoadFile(DllPath);
                    Type[] tys = assem.GetTypes();
                    //得到所有的类型名，然后遍历，通过类型名字来区别

                    foreach (Type ty in tys)//获取类名
                    {
                        if (ty.Name == "Class")
                        {
                            ConstructorInfo Constructor = ty.GetConstructor(Type.EmptyTypes);//获取不带参数的构造函数
                            object ClassObject = Constructor.Invoke(new object[] { });//获取类的实例

                            //执行类class的方法
                            MethodInfo mi = ty.GetMethod("");

                            if (ty.GetMethod("StyleName").Invoke(ClassObject, null).ToString() == ((TextBlock)c.SelectedItem).Text)
                            {
                                mi = ty.GetMethod("AddControls");
                                mi.Invoke(ClassObject, new object[] { style_edit, ControlValue[Style_list.SelectedIndex] });
                            }
                            else continue;

                        }
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }
        #endregion

        /// <summary> 重命名事件 </summary>
        void ItemName_Changed(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            foreach (var item in ((WrapPanel)Style_list.SelectedItem).Children)
            {
                if (item is TextBlock) ((TextBlock)item).Text = t.Text;
            }
            style_board.Children.Remove(t);
        }
        void ItemName_Changed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox t = (TextBox)sender;
                foreach (var item in ((WrapPanel)Style_list.SelectedItem).Children)
                {
                    if (item is TextBlock) ((TextBlock)item).Text = t.Text;
                }
                style_board.Children.Remove(t);
            }
        }
        #endregion

        /// <summary> 样式 显示/隐藏 </summary>
        void Item_SH(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (((((System.Windows.Shapes.Path)sender).Parent) as WrapPanel).Tag.ToString() == "true")
            {
                ((((System.Windows.Shapes.Path)sender).Parent) as WrapPanel).Tag = "false";
                ((System.Windows.Shapes.Path)sender).Data = (Geometry)FindResource("Icon.EyeOff");
            }
            else
            {
                ((((System.Windows.Shapes.Path)sender).Parent) as WrapPanel).Tag = "true";
                ((System.Windows.Shapes.Path)sender).Data = (Geometry)FindResource("Icon.EyeOn");
            }
        }

        /// <summary> 样式删除 </summary>
        void Item_delete(object sender, RoutedEventArgs e)
        {
            int index = Style_list.SelectedIndex;
            if (index != -1)
            {
                //List删除
                StyleType.RemoveAt(index);
                StyleParticle.RemoveAt(index);
                ControlValue.RemoveAt(index);

                //控件删除
                style_edit.Children.Clear();
                Style_list.Items.Remove(Style_list.SelectedItem);
                Style_list.SelectedIndex = -1;
            }
        }

        /// <summary> 样式清空 </summary>
        void Item_clear(object sender, RoutedEventArgs e)
        {
            style_edit.Children.Clear();
            Style_list.Items.Clear();

            StyleParticle.Clear();
            StyleType.Clear();
            ControlValue.Clear();
        }
        #endregion

        /// <summary> 生成指令并弹出检索窗体 </summary>
        void Generate_Click(object sender, RoutedEventArgs e)
        {

            result = string.Empty;//指令输出
            int index = 0;

            try
            {
                foreach (var StyleTypeItem in StyleType)
                {
                    //获取dll列表
                    foreach (FileInfo file in new DirectoryInfo(@"appfile\temp\masp").GetFiles("*.dll"))
                    {
                        string DllPath = file.FullName;
                        Assembly assem = Assembly.LoadFile(DllPath);
                        Type[] tys = assem.GetTypes();//得到所有的类型名，然后遍历，通过类型名字来区别

                        foreach (Type ty in tys)//获取类名
                        {
                            if (ty.Name == "Class")
                            {
                                ConstructorInfo Constructor = ty.GetConstructor(Type.EmptyTypes);//获取不带参数的构造函数
                                object ClassObject = Constructor.Invoke(new object[] { });//获取类的实例

                                //执行类class的方法
                                MethodInfo mi = ty.GetMethod("");

                                if (ty.GetMethod("StyleName").Invoke(ClassObject, null).ToString() == StyleTypeItem.ToString())
                                {
                                    mi = ty.GetMethod("Generate");
                                    mi.Invoke(ClassObject, new object[] { result, ControlValue[index] });
                                }
                                else continue;

                            }
                        }
                    }

                    index++;
                }
            }
            catch { }


        }

        void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            IniWrite("System", "PageIndex", "0", iniPath);
            PageIndex = 0;
            Page_masp = this;
            NavigationService.Navigate(Page_menu);
        }

        #region 视角持续旋转
        void Round_start(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            Round_status = 1;
            Pre_Timer.Start();
        }

        void Round_end(object sender, MouseEventArgs e)
        {
            Round_status = 0;
        }

        void Round_down(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Round_status = 2;
        }

        void Round_up(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Round_status = 1;
        }

        void Round_Tick(object sender, EventArgs e)
        {
            switch (Round_status)
            {
                case 0:
                    Round_speed -= 0.05f;
                    break;
                case 1:
                    if (Round_speed < Round_normal) Round_speed += 0.025f;
                    if (Round_speed > Round_normal) Round_speed -= 0.025f;
                    break;
                case 2:
                    if (Round_speed < Round_max) Round_speed += 0.025f;
                    break;
            }
            CameraRot[0] += Round_speed;
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
            if (Round_speed <= 0) { Round_speed = 0; Pre_Timer.Stop(); }
        }
        #endregion

        #region Viewport3D
        void ViewportToolsBar_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        void ViewportToolsBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        #region Buttons
        /// <summary> 工具栏折叠 </summary>
        void ToolBarSwitch_LeftDown(object sender, MouseButtonEventArgs e)
        {
            if (ToolBarSwitch.Pressed)
                ToolBarSwitch.Margin = new Thickness() { Top = ToolBarSwitch.Margin.Top + 35 };
            else
                ToolBarSwitch.Margin = new Thickness() { Top = ToolBarSwitch.Margin.Top - 35 };
        }

        /// <summary> 预览视角重置 </summary>        
        void Viewport_Relocation(object sender, RoutedEventArgs e)
        {
            CameraRadius = 4;
            CameraRot = new double[2] { 15, 60 };//水平旋转角,竖直旋转角(相对于原点)
            CameraLookAtPoint = new double[3] { 0, 10, 0 };//摄像机视点
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
        }
        #endregion

        /// <summary> 鼠标滚轮控制</summary>
        void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                CameraRadius += 0.1;
            else if (e.Delta > 0)
                CameraRadius -= 0.1;

            double max = 15, min = 1;

            if (CameraRadius > max) CameraRadius = max;
            else if (CameraRadius < min) CameraRadius = min;

            Scale.Value = CameraRadius;
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);//主摄像机
        }

        void Scale_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        void Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CameraRadius = Scale.Value;
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
        }

        #region 预览视角旋转-摄像机坐标计算
        void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;

            if (e.LeftButton == MouseButtonState.Pressed) PreviewGrid.Cursor = Cursors.SizeAll;
            else if (e.RightButton == MouseButtonState.Pressed) PreviewGrid.Cursor = Cursors.ScrollAll;
        }

        void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviewGrid.Cursor = Cursors.Arrow;
        }

        void Viewport_MouseLeave(object sender, MouseEventArgs e)
        {
            PreviewGrid.Cursor = Cursors.Arrow;
        }

        /// <summary> 鼠标拖拽 </summary>
        void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PreviewGrid.Cursor = Cursors.SizeAll;
                CameraRot[0] += (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * 180 / 460;
                CameraRot[1] += -(e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 180 / 460;
            }//左键转向
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                PreviewGrid.Cursor = Cursors.ScrollAll;
                Vector3D MoveDir = Viewport_3D.UpDirection_Get(MainCamera.LookDirection, new Vector3D() { X = 0, Y = 1, Z = 0 });

                //水平移动
                if (e.GetPosition((IInputElement)sender).X - mouse_location[0] != 0)
                {
                    CameraLookAtPoint[0] -= (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * MoveDir.X / 360;
                    CameraLookAtPoint[2] -= (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * MoveDir.Z / 360;
                }

                //竖直移动
                if (e.GetPosition((IInputElement)sender).Y - mouse_location[1] != 0)
                    CameraLookAtPoint[1] += (e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 0.01;

                MainCamera.LookDirection = new Vector3D()
                {
                    X = CameraLookAtPoint[0] - MainCamera.Position.X,
                    Y = CameraLookAtPoint[1] - MainCamera.Position.Y,
                    Z = CameraLookAtPoint[2] - MainCamera.Position.Z
                };
            }//右键平面移动

            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);

            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;

            if (CameraRot[0] > 360) CameraRot[0] = 0;
            else if (CameraRot[0] < 0) CameraRot[0] = 360;
            if (CameraRot[1] > 175) CameraRot[1] = 175;
            else if (CameraRot[1] < 5) CameraRot[1] = 5;
        }
        #endregion
        #endregion

        #region 粒子算法
        public static void Line(ref float R, ref float G, ref float B, ref string result, ref string Selector, ref string select_mark, ref int count, ref float particle_d)
        {
            //#region"算法处理"
            //int particle_num = 0/*粒子个数*/;
            //double distance = Math.Sqrt(Math.Pow(del[0], 2) + Math.Pow(del[1], 2) + Math.Pow(del[2], 2));
            //particle_d = particle_d == 0 ? 1 : particle_d;
            //particle_num = (int)(distance / particle_d);//粒子数量
            //for (int i = 0; i < 3; i++)
            //{
            //    ParticlePosition[i] = del[i] / particle_num;
            //    if (particle_num == 0) { ParticlePosition[i] = 0; }
            //}
            //#endregion
            //do
            //{
            //    for (int i = 0; i < 3; i++)
            //    { Start[i] += ParticlePosition[i]; }

            //    string Ones = null;
            //    if (SelectorSwitch) { Ones = "execute " + Selector + " ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
            //    //Ones += "particle " + particle_id + " ";
            //    if (SelectorSwitch) { Ones += "~" + Start[0].ToString("0.###") + " ~" + Start[1].ToString("0.###") + " ~" + Start[2].ToString("0.###"); }
            //    else { Ones += Start[0].ToString("0.###") + " " + Start[1].ToString("0.###") + " " + Start[2].ToString("0.###"); }
            //    //if (Colorful) { Ones += " " + R + " " + G + " " + B + " 1 1"; }
            //    //else { Ones += " 0 0 0 0 1"; }
            //    commands[k] = Ones; k++;
            //    result += Ones + "\r\n";
            //    count++;
            //} while (count <= particle_num);
        }

        public static void Roll(/*ref float R, ref float G, ref float B,*/ ref string result, ref string particle, ref string Selector, ref string select_mark, ref double r, ref double Angle, ref int score_tick, ref int score_, ref double par_height)
        {
            //double pitch/*圆相关循环参数*/;
            //Score_Start(ref result, ref Selector);
            //if (Extra_style == "单粒子环绕")
            //{
            //    #region 单粒子环绕
            //    for (pitch = 0; pitch <= 360; pitch += Angle)
            //    {
            //        if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
            //        Roll_math(ref r, ref pitch, ref par_height);
            //        Add_ones(/*ref R, ref G, ref B,*/ref particle, ref result, ref Selector);
            //    }
            //    Score_End(ref result, ref Selector);
            //    #endregion
            //}
            //else if (Extra_style == "双粒子环绕")
            //{
            //    #region 双粒子环绕
            //    #region 模块一
            //    result += "#模块一\r\n";
            //    for (pitch = 0; pitch <= 180; pitch += Angle)
            //    {
            //        if (ScoreSwitch) { if (pitch < 179) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; } }
            //        Roll_math(ref r, ref pitch, ref par_height);
            //        //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
            //    }
            //    #endregion
            //    ParticlePosition[1] = 0;
            //    score_ = 0;
            //    result += "\r\n";
            //    #region 模块二
            //    result += "#模块二\r\n";
            //    for (pitch = 180; pitch <= 360; pitch += Angle)
            //    {
            //        if (ScoreSwitch) { if (pitch < 359) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; } }
            //        Roll_math(ref r, ref pitch, ref par_height);
            //        //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
            //    }
            //    if (ScoreSwitch) { if (pitch < 359) { Selector_reset(ref score_, ref Selector, ref select_mark); } }
            //    Score_End(ref result, ref Selector);
            //    #endregion
            //    #endregion
            //}
            //else if (Extra_style == "螺旋延伸")
            //{
            //    #region 螺旋延伸
            //    double dr = r / 73;
            //    for (pitch = 0; pitch <= 360; pitch += Angle)
            //    {
            //        if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
            //        Roll_math(ref r, ref pitch, ref par_height);
            //        //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
            //        for (int i = 90; i <= 360; i += 90)
            //        {
            //            ParticlePosition[0] = (float)(Math.Sin((pitch + i) * Math.PI / 180) * r);
            //            ParticlePosition[2] = (float)(Math.Cos((pitch + i) * Math.PI / 180) * r);
            //            //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
            //        }
            //        r -= dr;
            //    }
            //    Score_End(ref result, ref Selector);
            //    #endregion
            //}
        }

        /// <summary> 圆圈坐标计算模块 </summary>
        public static void Roll_math(ref double r, ref double pitch, ref double par_height)
        {
            //ParticlePosition[0] = (float)(Math.Sin(pitch * Math.PI / 180) * r);
            //ParticlePosition[1] += par_height;
            //ParticlePosition[2] = (float)(Math.Cos(pitch * Math.PI / 180) * r);
        }

        public static void Ball(/*ref float R, ref float G, ref float B,*/ ref string result, ref string Selector, ref string select_mark, ref double r, ref double Angle, ref int score_tick, ref int score_)
        {
            //double yaw, pitch/*圆相关循环参数*/;
            //float Pi = 361, Pi2 = 181;
            //if (Extra_style == "半圈周期") { float mark; mark = Pi; Pi = Pi2; Pi2 = mark; }
            //Score_Start(ref result, ref Selector);
            //for (yaw = 0; yaw < Pi; yaw += Angle)
            //{
            //    if (ScoreSwitch) { Selector_reset(ref score_, ref Selector, ref select_mark); score_ += score_tick; }
            //    result += "#\r\n";
            //    for (pitch = 0; pitch < Pi2; pitch += Angle)
            //    {
            //        Ball_math(ref r, ref pitch, ref yaw);
            //        Axis_Test();
            //        //Add_ones(ref R, ref G, ref B, ref result, ref Selector);
            //    }
            //    if (ScoreSwitch) { if (yaw < Pi) { Selector_reset(ref score_, ref Selector, ref select_mark); } }
            //}
            //score_ -= score_tick; Selector_reset(ref score_, ref Selector, ref select_mark);
            //Score_End(ref result, ref Selector);
        }

        /// <summary> 球体坐标计算模块 </summary>
        public static void Ball_math(ref double r, ref double pitch, ref double yaw)
        {
            //ParticlePosition[0] = (float)(Math.Sin(pitch * Math.PI / 180) * Math.Cos(yaw * Math.PI / 180) * r);
            //ParticlePosition[1] = (float)(Math.Sin(pitch * Math.PI / 180) * Math.Sin(yaw * Math.PI / 180) * r);
            //ParticlePosition[2] = (float)(Math.Cos(pitch * Math.PI / 180) * r);
        }

        public static void test(ref int pre_count, ref double r, ref double Angle, ref double par_height)
        {
            //double pitch/*圆相关循环参数*/;
            //for (pitch = 0; pitch <= 360; pitch += Angle)
            //{
            //    ParticlePosition[0] = Convert.ToSingle((Math.Sin(pitch * Math.PI / 180) * r));
            //    ParticlePosition[1] = par_height;
            //    ParticlePosition[2] = Convert.ToSingle((Math.Cos(pitch * Math.PI / 180) * r));
            //    // if (ScoreSwitch) { score_ += score_tick; }
            //    // Add_ones(ref result,ref Selector);
            //}
        }

        #region 共用模块
        /// <summary> 写入NBT文件缓存 </summary>
        static void Add_ones(/*ref float R, ref float G, ref float B,*/ref string particle, ref string result, ref string Selector)
        {
            //string Ones = null;
            //if (SelectorSwitch) { Ones = "execute as " + Selector + " at @s run ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
            //if (SelectorSwitch) { Ones += "particle " + particle + " ~" + ParticlePosition[0].ToString("0.####") + " ~" + ParticlePosition[1].ToString("0.####") + " ~" + ParticlePosition[2].ToString("0.####"); }
            //else { Ones += "particle " + particle + " ^" + (exe_shift[0] + ParticlePosition[0]).ToString("0.####") + " ^" + (exe_shift[1] + ParticlePosition[1]).ToString("0.####") + " ^" + (exe_shift[2] + ParticlePosition[2]).ToString("0.####"); }
            ////if (Colorful) { Ones += " " + R + " " + G + " " + B + " 1 0"; }
            ////else { Ones += " 0 0 0 0 1"; }
            ////commands[k] = Ones; k++;
            //result += Ones + "\r\n";
        }

        /// <summary> 轴心判定模块 </summary>
        public static void Axis_Test()
        {
            //double mark;
            //if (axis[0] == true)
            //{
            //    mark = ParticlePosition[0];
            //    ParticlePosition[0] = ParticlePosition[2];
            //    ParticlePosition[2] = mark;
            //}
            //else if (axis[1] == true)
            //{
            //    mark = ParticlePosition[2];
            //    ParticlePosition[2] = ParticlePosition[1];
            //    ParticlePosition[1] = mark;
            //}
        }
        #endregion
        #endregion
    }
}
