using ArmorStand.CustomControl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using static Adaptable_Studio.Armor_stand_Page;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;
using static Adaptable_Studio.PublicControl;

namespace Adaptable_Studio
{
    class Page_DropInfo : Page
    {
        public SortedList<string, string> BasicData = null;
        public SortedList<string, bool> NBTData = null;
        public List<double> PoseData = null;
        public bool IsAdvanceEnabled = false;
        public ItemData[] ItemsData = null;
        public int TimeAxisTotalTick = 20;
        public Time_axis.Pose[] TimeAxisPoses = null;

        /// <summary> Json反序列化 </summary>
        /// <param name="ReaderPath">文件路径</param>
        /// <param name="json">Json类型变量</param>
        public static void Deserialize(string ReaderPath, ref Page_DropInfo json)
        {
            using (StreamReader line = new StreamReader(ReaderPath))
            {
                try
                {
                    string JSONcontent = line.ReadToEnd();
                    json = JsonConvert.DeserializeObject<Page_DropInfo>(JSONcontent);
                }
                catch { Log_Write(LogPath, "json文件反序列化失败"); }
            }
        }
    }

    /// <summary> armor_stand_Page.xaml 的交互逻辑 </summary>
    public partial class Armor_stand_Page : Page
    {
        #region Define
        #region Data Store
        public struct Data
        {
            public SortedList<string, string> BasicData;
            public SortedList<string, bool> NBTData;
            public List<double> PoseData;
            public bool IsAdvanceEnabled;
            public ItemData[] ItemsData;
            public long TimeAxisTotalTick;
            public Time_axis.Pose[] TimeAxisPoses;
        }
        Page_DropInfo page_DropInfo;//存档结构体
        bool loading = false;
        #endregion

        #region EquipItem
        Json ItemName;
        /// <summary> 物品部位结构体 </summary>
        public struct ItemData
        {
            /// <summary> 部位是否启用 </summary>
            public bool IsEnabled;
            /// <summary> 物品列表索引值 </summary>
            public int ItemListIndex;
            /// <summary> 物品名字 </summary>
            public string ItemName;
            /// <summary> 物品介绍 </summary>
            public string ItemLore;
            /// <summary> 物品数量 </summary>
            public int ItemCount;
            /// <summary> 物品附加值 </summary>
            public int ItemDamage;
            /// <summary> 不可破坏属性 </summary>
            public bool ItemUnbreakable;
            /// <summary> 隐藏NBT属性 </summary>
            public bool ItemHideFlags;

            /// <summary> 附加属性是否启用 </summary>
            public bool IsAttriEnabled;
            /// <summary> 附加属性值列表 </summary>
            public struct AttributeList
            {
                public string uid;
                public string tag;
                public double lvl;
                public bool percent;
            }
            public AttributeList[] AttriList;

            /// <summary> 附魔属性是否启用 </summary>
            public bool IsEnchEnabled;
            /// <summary> 附魔属性列表<id,lvl> </summary>
            public SortedList<string, int> EnchList;
        }

        //0-头,1-躯干,2-主手,3-副手,4-腿部,5-脚部
        /// <summary> 用于存储的部位物品 结构化数据 </summary>
        ItemData[] Item_Data = new ItemData[6];

        /// <summary> 部位物品-序列化输出结果 </summary>
        string[] Item_PartData = new string[6];

        /// <summary> ItemData_List的每个部位的输出结果[部位,索引位置] </summary>
        string[,] OutPutItem;

        /// <summary> NBT变量索引 </summary>
        string[] ItemData_List = new string[]
        {
            "id:\"<value>\"",
            "Count:<value>b",
            "Damage:<value>s",
            "Unbreakable:<value>b",
            "HideFlags:<value>",
            //tag:{
            //  display:{
	        "Name:\"{\\\"text\\\":\\\"<value>\\\"}\"",
            "Lore:[\"<value>\"]",
            //  }

            //  AttributeModifiers:[
	        "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>d,AttributeName:<tag>,Name:<name>,",
            "Operation:<value>b}",
            //  ]

            //  Enchantments:[
	        "{id:\"<tag>\",lvl:<value>s}"
            //  ]
            //}
        };
        #endregion

        #region Viewport3D
        /// <summary> 摄像机半径(相对于原点) </summary>
        double CameraRadius = 50;
        /// <summary> 水平旋转角,竖直旋转角(相对于原点) </summary>
        double[] CameraRot = new double[2] { 15, 80 };
        /// <summary> 摄像机视点 </summary>
        double[] CameraLookAtPoint = new double[3] { 0, 15, 0 };
        /// <summary> 获取鼠标位置 </summary>
        double[] mouse_location = new double[2];
        /// <summary> 与模型动作关联的角度参数 </summary>
        public static double[] pose = new double[19];
        /// <summary> 高级模式下，控制模型是否随当前帧变化而改变 </summary>
        public static bool poseChange;
        #endregion

        #endregion

        public Armor_stand_Page()
        {
            InitializeComponent();

            #region Events
            Loaded += Page_Loaded;
            DragEnter += Page_DragEnter;
            Drop += Page_DragDrop;
            BackHome.Click += BackToMenu_Click;
            #region 分类状态栏
            Main.Click += Main_Settings;
            Nbt.Click += Nbt_Settings;
            Pose.Click += Rotation_Settings;
            Item.Click += Item_Settings;
            #endregion

            #region 选项交互
            Settings.ScrollChanged += SelectBar_Move;
            //Pose
            Rotation_Setting.ValueChanged += Pose_Changed;
            Pose_Selector.SelectionChanged += PoseTabChanged;
            X_Pose.ValueChanged += Pose_Changed;
            Y_Pose.ValueChanged += Pose_Changed;
            Z_Pose.ValueChanged += Pose_Changed;
            Pose_Reset.MouseLeftButtonDown += PoseReset_LeftButtonDown;
            //Items
            Part_Slector.SelectionChanged += Item_TabChanged;
            ItemPanel.MouseEnter += ItemGrid_update;
            ItemPanel.MouseLeave += ItemGrid_update;
            Item_Reset.MouseLeftButtonDown += ItemReset_LeftButtonDown;
            #endregion

            #region Viewport3D
            PreviewGrid.MouseUp += Viewport_MouseUp;
            PreviewGrid.MouseMove += Viewport_MouseMove;
            PreviewGrid.MouseLeave += Viewport_MouseLeave;
            PreviewGrid.MouseWheel += Viewport_MouseWheel;

            Relocation.Click += Viewport_Relocation;
            #endregion
            OutPutBox.MouseDown += OutPut_MouseDown;
            #endregion

            #region 参数初始化
            for (int i = 0; i < 6; i++)
            {
                Item_Data[i].ItemCount = 1;

                #region 附加属性
                int count = 0;
                foreach (var item in grid2.Children)
                    if (item is CustomNumberBox) count++;

                Item_Data[i].AttriList = new ItemData.AttributeList[count];
                for (int j = 0; j < count; j++)
                {
                    Item_Data[i].AttriList[j].uid = "";
                    Item_Data[i].AttriList[j].tag = "";
                    Item_Data[i].AttriList[j].lvl = 0;
                    Item_Data[i].AttriList[j].percent = false;
                }
                #endregion

                #region 附魔属性
                Item_Data[i].EnchList = new SortedList<string, int>();

                count = 0;
                foreach (var item in grid3.Children)
                {
                    if (item is CustomNumberBox) count++;
                }
                for (int j = 0; j < count; j++)
                {
                    Item_Data[i].EnchList.Add(j.ToString(), 0);
                }
                #endregion
            }
            #endregion
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }
        /// <summary> 存档数据读取 </summary>
        private void Page_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Page_DropInfo.Deserialize(path, ref page_DropInfo);
            loading = true;

            //BasicData
            foreach (var item in page_DropInfo.BasicData)
            {
                foreach (var element in BasicPannel.Children)
                {
                    //匹配元素名,赋值
                    if (element is TextBox && ((TextBox)element).Name == item.Key)
                    {
                        ((TextBox)element).Text = item.Value;
                        break;
                    }
                }
            }

            //NBTData
            foreach (var item in page_DropInfo.NBTData)
            {
                foreach (var element in NbtPannel.Children)
                {
                    //匹配元素名,赋值
                    if (element is CheckBox && ((CheckBox)element).Name == item.Key)
                    {
                        ((CheckBox)element).IsChecked = item.Value;
                        break;
                    }
                }
            }

            //PoseData
            for (int i = 0; i < page_DropInfo.PoseData.Count; i++)
                pose[i] = page_DropInfo.PoseData[i];

            UI_advancedmode.IsChecked = page_DropInfo.IsAdvanceEnabled;
            {
                //将控件值存储到变量
                int i = Pose_Selector.SelectedIndex * 3;
                pose[18] = Rotation_Setting.Value;
                X_Pose.Value = pose[i];
                Y_Pose.Value = pose[i + 1];
                Z_Pose.Value = pose[i + 2];
            }//用户页面更新

            //ItemData
            for (int i = 0; i < 6; i++)
                Item_Data[i] = page_DropInfo.ItemsData[i];
            Item_TabChanged(sender, e);//用户页面更新

            //TimeAxis
            TimeAxis.TotalTick = page_DropInfo.TimeAxisTotalTick;

            for (int i = 0; i < page_DropInfo.TimeAxisTotalTick; i++)
            {
                TimeAxis.pose[i] = page_DropInfo.TimeAxisPoses[i];
            }

            loading = false;
        }

        /// <summary> 页面数据存储 </summary>
        public void PageSaveFile(object sender, EventArgs e)
        {
            Data data = new Data()
            {
                BasicData = new SortedList<string, string>(),
                NBTData = new SortedList<string, bool>(),
                PoseData = new List<double>(),
                IsAdvanceEnabled = (bool)UI_advancedmode.IsChecked,
                ItemsData = new ItemData[6],
                TimeAxisTotalTick = TimeAxis.TotalTick,
                TimeAxisPoses = new Time_axis.Pose[TimeAxis.TotalTick]
            };

            //basic
            foreach (var item in BasicPannel.Children)
            {
                if (item is TextBox)
                    data.BasicData.Add(((TextBox)item).Name, ((TextBox)item).Text);
            }

            //nbts
            foreach (var item in NbtPannel.Children)
            {
                if (item is CheckBox)
                    data.NBTData.Add(((CheckBox)item).Name, (bool)((CheckBox)item).IsChecked);
            }

            //pose
            for (int i = 0; i < pose.Length; i++)
                data.PoseData.Add(pose[i]);

            //items
            for (int i = 0; i < 6; i++)
                data.ItemsData[i] = Item_Data[i];

            //time_axis
            for (int i = 0; i < data.TimeAxisPoses.Length; i++)
                data.TimeAxisPoses[i] = TimeAxis.pose[i];

            //序列化 -> json
            string DataResult = JsonConvert.SerializeObject(data);
            //保存到路径
            System.Windows.Forms.SaveFileDialog file = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "(*.masc)|*.masc",
                FileName = "Save.masc",
                AddExtension = true
            };
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(file.FileName, DataResult);
            }
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log_Write(LogPath, "==========MASC==========");
            Log_Write(LogPath, "环境初始化");
            //引导页面
            if (Guiding)
            {
                Settings_guide.Visibility = Visibility.Visible;
                Timeaxis_guide.Visibility = Visibility.Visible;
            }
            else
            {
                Settings_guide.Visibility = Visibility.Hidden;
                Timeaxis_guide.Visibility = Visibility.Hidden;
            }

            #region Viewport3D 初始化
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
            LightDirectionReset();
            Log_Write(LogPath, "Viewport3D初始化完成");
            #endregion

            //json列表获取
            try
            {
                //判断版本读取对应json
                switch (true)
                {
                    default:
                        Json.Deserialize(@"json\masc\1_12.json", ref ItemName);//1.12
                        break;
                }

                //导入物品列表
                if (_langCN)
                {
                    foreach (var item in ItemName.CN)
                        ItemList.Items.Add(new TextBlock() { Text = item });
                }
                else
                {
                    foreach (var item in ItemName.EN)
                    {
                        string itemName = item.Replace("minecraft:", "");
                        ItemList.Items.Add(new TextBlock() { Text = itemName });
                    }
                }
                ItemList.SelectedIndex = 0;
                Log_Write(LogPath, "物品json读取完成");
            }
            catch (Exception ex)
            {
                Log_Write(LogPath, "json读取失败:" + ex);
            }
        }

        #region 页面交互
        Thickness thick = new Thickness();
        void Main_Settings(object sender, RoutedEventArgs e)
        {
            Settings.ScrollChanged -= SelectBar_Move;
            e.Handled = true;
            Settings.ScrollToVerticalOffset(0);
            thick = new Thickness(0, 47, 0, 0);
            SelectBar.Margin = thick;

            Thread th = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(10);
                Settings.ScrollChanged += SelectBar_Move;
            }));
            th.Start();
        }

        void Nbt_Settings(object sender, RoutedEventArgs e)
        {
            Settings.ScrollChanged -= SelectBar_Move;
            e.Handled = true;
            Settings.ScrollToVerticalOffset(90);
            thick = new Thickness(0, 87, 0, 0);
            SelectBar.Margin = thick;

            Thread th = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(10);
                Settings.ScrollChanged += SelectBar_Move;
            }));
            th.Start();
        }

        void Rotation_Settings(object sender, RoutedEventArgs e)
        {
            Settings.ScrollChanged -= SelectBar_Move;
            e.Handled = true;
            Settings.ScrollToVerticalOffset(343);
            thick = new Thickness(0, 127, 0, 0);
            SelectBar.Margin = thick;

            Thread th = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(10);
                Settings.ScrollChanged += SelectBar_Move;
            }));
            th.Start();
        }

        void Item_Settings(object sender, RoutedEventArgs e)
        {
            Settings.ScrollChanged -= SelectBar_Move;
            e.Handled = true;
            Settings.ScrollToVerticalOffset(524);
            thick = new Thickness(0, 167, 0, 0);
            SelectBar.Margin = thick;

            Thread th = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(10);
                Settings.ScrollChanged += SelectBar_Move;
            }));
            th.Start();
        }

        void SelectBar_Move(object sender, ScrollChangedEventArgs e)
        {
            if (Settings.VerticalOffset <= 90)
                thick = new Thickness(0, 47, 0, 0);
            else if (Settings.VerticalOffset > 90 && Settings.VerticalOffset <= 343)
                thick = new Thickness(0, 87, 0, 0);
            else if (Settings.VerticalOffset > 343 && Settings.VerticalOffset <= 524)
                thick = new Thickness(0, 127, 0, 0);
            else if (Settings.VerticalOffset > 524)
                thick = new Thickness(0, 167, 0, 0);
            SelectBar.Margin = thick;
        }

        /// <summary> 切换选项卡时，动作数据实时反馈给Slider </summary>
        void PoseTabChanged(object sender, SelectionChangedEventArgs e)
        {
            double X = 0, Y = 0, Z = 0;
            int index = Pose_Selector.SelectedIndex;

            switch (index)
            {
                case 0: X = pose[0]; Y = pose[1]; Z = pose[2]; break;
                case 1: X = pose[3]; Y = pose[4]; Z = pose[5]; break;
                case 2: X = pose[6]; Y = pose[7]; Z = pose[8]; break;
                case 3: X = pose[9]; Y = pose[10]; Z = pose[11]; break;
                case 4: X = pose[12]; Y = pose[13]; Z = pose[14]; break;
                case 5: X = pose[15]; Y = pose[16]; Z = pose[17]; break;
            }//部位值传递

            X_Pose.Value = X; Y_Pose.Value = Y; Z_Pose.Value = Z;
        }
        #endregion

        #region Reset Button
        /// <summary> 动作UI重置 </summary>
        void PoseReset_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Pose_Reset.IsConfirmRest)
            {
                for (int i = 0; i < 19; i++)
                {
                    pose[i] = 0;
                    if ((bool)UI_advancedmode.IsChecked && TimeAxis.FramePose.key)
                        TimeAxis.FramePose.pos[i] = 0;
                }

                poseChange = true;
                X_Pose.Value = 0;
                Y_Pose.Value = 0;
                Z_Pose.Value = 0;
                Pose_Reset.IsConfirmRest = false;
                Rotation_Setting.Value = 0;
            }

        }

        /// <summary> 装备UI重置 </summary>        
        void ItemReset_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Item_Reset.IsConfirmRest)
            {
                EnablePart_Settings.IsChecked = false;
                foreach (var item in grid1.Children)
                {
                    if (item is ComboBox)
                        ((ComboBox)item).SelectedIndex = 0;
                    else if (item is CustomNumberBox)
                        ((CustomNumberBox)item).Value = ((CustomNumberBox)item).Minimum;
                    else if (item is CheckBox)
                        ((CheckBox)item).IsChecked = false;
                    else if (item is TextBox)
                        ((TextBox)item).Text = string.Empty;
                }

                foreach (var item in grid2.Children)
                {
                    if (item is CheckBox)
                        ((CheckBox)item).IsChecked = false;
                    else if (item is CustomNumberBox)
                        ((CustomNumberBox)item).Value = ((CustomNumberBox)item).Minimum;
                }

                foreach (var item in grid3.Children)
                {
                    if (item is CustomNumberBox)
                        ((CustomNumberBox)item).Value = ((CustomNumberBox)item).Minimum;
                }
                ItemGrid_update(sender, e);
                Item_Reset.IsConfirmRest = false;
            }
        }
        #endregion

        /// <summary> 不可编辑参数运算 </summary>
        public void DisabledSlots_print(ref double a, ref int b)
        {
            a += Math.Pow(2, b);
            a += Math.Pow(2, b + 8);
            a += Math.Pow(2, b + 16);
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

        /// <summary> 基础NBT输出判定 </summary>
        void Generate_BasicNBT(ref string result)
        {
            if (UI_name.Text != string.Empty)
                result += "CustomName:\"{\\\"text\\\":\\\"" + UI_name.Text + "\\\"}\",";
            if (UI_tags.Text != string.Empty)
                result += "Tags:[\"" + UI_tags.Text + "\"],";
            if ((bool)disabled_hand.IsChecked | (bool)disabled_head.IsChecked | (bool)disabled_chest.IsChecked | (bool)disabled_legs.IsChecked | (bool)disabled_boots.IsChecked)
            {
                int b;
                double a = 0;
                if ((bool)disabled_hand.IsChecked) { b = 0; DisabledSlots_print(ref a, ref b); }
                if ((bool)disabled_boots.IsChecked) { b = 1; DisabledSlots_print(ref a, ref b); }
                if ((bool)disabled_legs.IsChecked) { b = 2; DisabledSlots_print(ref a, ref b); }
                if ((bool)disabled_chest.IsChecked) { b = 3; DisabledSlots_print(ref a, ref b); }
                if ((bool)disabled_head.IsChecked) { b = 4; DisabledSlots_print(ref a, ref b); }
                result += "DisabledSlots:" + a + ",";
            }
            if ((bool)UI_glowing.IsChecked) result += "Glowing:1b,";
            if ((bool)UI_NoAI.IsChecked) result += "NoAI:1b,";
            if ((bool)UI_invulnerable.IsChecked) result += "Invulnerable:1b,";
            if ((bool)UI_invisible.IsChecked) result += "Invisible:1b,";
            if ((bool)UI_silent.IsChecked) result += "Silent:1b,";
            if ((bool)UI_small.IsChecked) result += "Small:1b,";
            if ((bool)UI_showarms.IsChecked) result += "ShowArms:1b,";
            if ((bool)UI_nobaseplate.IsChecked) result += "NoBasePlate:1b,";
            if ((bool)UI_nogravity.IsChecked) result += "NoGravity:1b,";
            if ((bool)UI_marker.IsChecked) result += "Marker:1b,";
            if ((bool)UI_lefthanded.IsChecked) result += "LeftHanded:1b,";

        }

        /// <summary> 反序列化指令并输出 </summary>
        void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)UI_advancedmode.IsChecked)
            {
                result = "summon armor_stand ~ ~1 ~ {";

                Generate_BasicNBT(ref result);
                if ((bool)Pose_Settings.IsChecked)
                {
                    result += "Pose:{Head:["
                        + pose[0].ToString("0.#") + "f," + pose[1].ToString("0.#") + "f," + pose[2].ToString("0.#") + "f],Body:["
                        + pose[3].ToString("0.#") + "f," + pose[4].ToString("0.#") + "f," + pose[5].ToString("0.#") + "f],LeftArm:["
                        + pose[6].ToString("0.#") + "f," + pose[7].ToString("0.#") + "f," + pose[8].ToString("0.#") + "f],RightArm:["
                        + pose[9].ToString("0.#") + "f," + pose[10].ToString("0.#") + "f," + pose[11].ToString("0.#") + "f],LeftLeg:["
                        + pose[12].ToString("0.#") + "f," + pose[13].ToString("0.#") + "f," + pose[14].ToString("0.#") + "f],RightLeg:["
                        + pose[15].ToString("0.#") + "f," + pose[16].ToString("0.#") + "f," + pose[17].ToString("0.#") + "f]},";
                }

                if (Rotation_Setting.Value != 0)
                    result += "Rotation:[" + pose[18].ToString("0.#") + "f," + "0f],";

                #region 物品数据处理

                OutPutItem = new string[6, ItemData_List.Length];
                //部位循环
                for (int r = 0; r < 6; r++)
                {
                    List<string> BasicResult = new List<string>();//底层属性标签存储
                    List<string> DisplayResult = new List<string>();//Display属性结果存储
                    List<string> AttriResult = new List<string>();//附加属性结果存储
                    List<string> EnchResult = new List<string>();//附魔属性结果存储
                                                                 //数组数据初始化
                    for (int i = 0; i < ItemData_List.Length; i++)
                    {
                        OutPutItem[r, i] = ItemData_List[i];
                    }

                    List<string> Temp = new List<string>();//tag内部标签缓存
                    Item_PartData[r] = null;
                    if (Item_Data[r].IsEnabled)
                    {
                        #region 赋值操作
                        //id
                        BasicResult.Add(ItemData_List[0].Replace("<value>", ItemName.EN[Item_Data[r].ItemListIndex]));

                        //count
                        BasicResult.Add(ItemData_List[1].Replace("<value>", Item_Data[r].ItemCount.ToString()));

                        //damage
                        BasicResult.Add(ItemData_List[2].Replace("<value>", Item_Data[r].ItemDamage.ToString()));

                        //Unbreakable
                        if (Item_Data[r].ItemUnbreakable)
                            BasicResult.Add(ItemData_List[3].Replace("<value>", Item_Data[r].ItemDamage.ToString()));

                        //HideFlags
                        if (Item_Data[r].ItemHideFlags)
                            BasicResult.Add(
                                Item_Data[r].ItemHideFlags ?
                                ItemData_List[4].Replace("<value>", "63") : ItemData_List[4].Replace("<value>", "0")
                            );

                        BasicResult.Add("tag:{");
                        BasicResult.Add("<tagTags>");//标记替换位置
                        {
                            //判断是否自定义display
                            if (Item_Data[r].ItemName != string.Empty || Item_Data[r].ItemLore != string.Empty)
                            {
                                Temp.Add("display:{");
                                Temp.Add("<DisplayTags>");//标记替换位置
                                {
                                    //Name
                                    DisplayResult.Add(ItemData_List[5].Replace("<value>", Item_Data[r].ItemName));
                                    //Lore
                                    DisplayResult.Add(ItemData_List[6].Replace("<value>", Item_Data[r].ItemLore));
                                }
                                Temp.Add("}");
                            }

                            if (Item_Data[r].IsAttriEnabled)
                            {
                                Temp.Add("AttributeModifiers:[");
                                Temp.Add("<AttributeModifiersTags>");//标记替换位置
                                for (int i = 0; i < Item_Data[r].AttriList.Length; i++)
                                {
                                    ItemData.AttributeList list = Item_Data[r].AttriList[i];
                                    if (list.lvl != 0)
                                    {
                                        //调用："{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:<tag>,Name:<name>,"
                                        string info = OutPutItem[r, 7];
                                        //替换tag and lvl
                                        info = info.Replace("<tag>", list.tag);
                                        info = info.Replace("<value>", list.lvl.ToString());
                                        //替换name and percent
                                        info = info.Replace("<name>", list.uid);

                                        //调用："Operation:<value>}"
                                        info += OutPutItem[r, 8].Replace("<value>", list.percent ? "1b" : "0b");

                                        AttriResult.Add(info);
                                    }
                                }
                                Temp.Add("]");
                            }

                            if (Item_Data[r].IsEnchEnabled)
                            {
                                //Temp.Add("ench:[");
                                Temp.Add("Enchantments:[");
                                Temp.Add("<EnchTag>");//标记替换位置 
                                foreach (var item in Item_Data[r].EnchList)
                                {
                                    //调用模板： "{id:<tag>,lvl:<value>}"
                                    string info = OutPutItem[r, 9];
                                    //替换id and lvl
                                    info = info.Replace("<tag>", item.Key.ToString());
                                    info = info.Replace("<value>", item.Value.ToString());

                                    EnchResult.Add(info);
                                }
                                Temp.Add("]");
                            }
                        }
                        BasicResult.Add("}");
                        #endregion

                        #region 反序列化
                        //将列表元素合并，以逗号隔开
                        Item_PartData[r] = string.Join(",", BasicResult);
                        string temp = string.Join(",", Temp);
                        temp = temp.Replace(",<DisplayTags>,", string.Join(",", DisplayResult));
                        temp = temp.Replace(",<AttributeModifiersTags>,", string.Join(",", AttriResult));
                        temp = temp.Replace(",<EnchTag>,", string.Join(",", EnchResult));
                        Item_PartData[r] = Item_PartData[r].Replace(",<tagTags>,", string.Join(",", temp));

                        #endregion
                    }
                }
                #endregion

                #region 部位输出
                //0-头,1-躯干,2-主手,3-副手,4-腿部,5-脚部
                if (Item_Data[2].IsEnabled || Item_Data[3].IsEnabled)
                    result += "HandItems:[{"
                        + (Item_Data[2].IsEnabled ? Item_PartData[2] : null) + "},{"
                        + (Item_Data[3].IsEnabled ? Item_PartData[3] : null) + "}]";

                if ((Item_Data[2].IsEnabled || Item_Data[3].IsEnabled) &&
                    (Item_Data[0].IsEnabled || Item_Data[1].IsEnabled || Item_Data[4].IsEnabled || Item_Data[5].IsEnabled))
                    result += ",";

                if (Item_Data[0].IsEnabled || Item_Data[1].IsEnabled || Item_Data[4].IsEnabled || Item_Data[5].IsEnabled)
                    result += "ArmorItems:[{"
                        + (Item_Data[5].IsEnabled ? Item_PartData[5] : null) + "},{"
                        + (Item_Data[4].IsEnabled ? Item_PartData[4] : null) + "},{"
                        + (Item_Data[1].IsEnabled ? Item_PartData[1] : null) + "},{"
                        + (Item_Data[0].IsEnabled ? Item_PartData[0] : null) + "}]";
                #endregion

                result += "}";
            }//常规模式
            else
            {
                string tag = UI_tags.Text;

                for (int i = 0; i < TimeAxis.TotalTick; i++)
                {
                    result = "data merge entity @e[type=armor_stand,limit=1";

                    if (UI_tags.Text != string.Empty) result += ",tag=" + tag;
                    result += ",scores={" + "ScoreName=" + i.ToString() + "}"
                     + "] {";
                    Generate_BasicNBT(ref result);
                    if ((bool)Pose_Settings.IsChecked)
                        result += "Pose:{Head:["
                            + TimeAxis.pose[i].pos[0].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[1].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[2].ToString("0.#") + "f],Body:["
                            + TimeAxis.pose[i].pos[3].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[4].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[5].ToString("0.#") + "f],LeftArm:["
                            + TimeAxis.pose[i].pos[6].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[7].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[8].ToString("0.#") + "f],RightArm:["
                            + TimeAxis.pose[i].pos[9].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[10].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[11].ToString("0.#") + "f],LeftLeg:["
                            + TimeAxis.pose[i].pos[12].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[13].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[14].ToString("0.#") + "f],RightLeg:["
                            + TimeAxis.pose[i].pos[15].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[16].ToString("0.#") + "f,"
                            + TimeAxis.pose[i].pos[17].ToString("0.#") + "f]},";
                    if (Rotation_Setting.Value != 0) result += "Rotation:["
                            + TimeAxis.pose[i].pos[18].ToString("0.#") + "f," + "0f],";

                    result += "}";
                }//时间轴数据

                //加分
                result = "scoreboard players add @e";

                if (UI_tags.Text != string.Empty)
                    result += "[tag=" + tag + "]";

                result += " ScoreName 1";
            }//高级模式            
        }

        void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            IniWrite("System", "PageIndex", "0", iniPath);
            PageIndex = 0;
            Page_masc = this;
            NavigationService.Navigate(Page_menu);
        }

        #region 物品设置
        /// <summary> 物品部位切换,读取参数 </summary>
        void Item_TabChanged(object sender, RoutedEventArgs e)
        {
            //获取当前编辑部位
            int part = Part_Slector.SelectedIndex;
            //控制当前部位输出开关
            EnablePart_Settings.IsChecked = Item_Data[part].IsEnabled;

            #region 基本属性
            ItemList.SelectedIndex = Item_Data[part].ItemListIndex;//物品索引
            UI_itemName.Text = Item_Data[part].ItemName;//物品名
            UI_itemLore.Text = Item_Data[part].ItemLore;//介绍文本
            UI_itemCount.Value = Item_Data[part].ItemCount;//数量
            UI_itemDamage.Value = Item_Data[part].ItemDamage;//附加值
            UI_itemUnbreakable.IsChecked = Item_Data[part].ItemUnbreakable;//不可破坏
            UI_itemHideFlags.IsChecked = Item_Data[part].ItemHideFlags;//隐藏标签

            Enable_Attribute_Settings.IsChecked = Item_Data[part].IsAttriEnabled;//附加属性开关
            Enable_Enchant_Settings.IsChecked = Item_Data[part].IsEnchEnabled;//附魔属性开关
            #endregion

            #region 附加属性列表
            //清空容器内元素设置
            foreach (var item in grid2.Children)
            {
                if (item is CustomNumberBox)
                {
                    ((CustomNumberBox)item).Value = ((CustomNumberBox)item).Minimum;
                }
                else if (item is CheckBox)
                {
                    ((CheckBox)item).IsChecked = false;
                }
            }

            //遍历数组元素
            for (int i = 0; i < Item_Data[part].AttriList.Length; i++)
            {
                ItemData.AttributeList list = Item_Data[part].AttriList[i];
                //若数组存储值不为0
                if (list.lvl != 0)
                {
                    //根据uid赋值控件
                    foreach (var numbox in grid2.Children)
                    {
                        if (numbox is CustomNumberBox && ((CustomNumberBox)numbox).Uid == list.uid)
                        {
                            ((CustomNumberBox)numbox).Value = list.lvl;
                            break;
                        }
                    }

                    foreach (var chkbox in grid2.Children)
                    {
                        if (chkbox is CheckBox && ((CheckBox)chkbox).Uid == list.uid)
                        {
                            ((CheckBox)chkbox).IsChecked = list.percent;
                            break;
                        }
                    }
                }
            }
            #endregion

            #region 附魔属性列表
            //清空容器内元素设置
            foreach (var item in grid3.Children)
            {
                if (item is CustomNumberBox)
                {
                    ((CustomNumberBox)item).Value = ((CustomNumberBox)item).Minimum;
                }
            }

            //根据非零键值对进行id匹配
            foreach (string id in Item_Data[part].EnchList.Keys)
            {
                //每遍历一个键，在容器内搜索一遍 存在键值的控件
                foreach (var numbox in grid3.Children)
                {
                    //元素为数字控件 并且 控件uid与索引id匹配
                    if (numbox is CustomNumberBox && ((CustomNumberBox)numbox).Uid == id)
                    {
                        ((CustomNumberBox)numbox).Value = Item_Data[part].EnchList.Values[Item_Data[part].EnchList.IndexOfKey(id)];
                        break;
                    }
                }
            }
            #endregion

        }

        /// <summary> 物品数据存储 </summary>
        void ItemGrid_update(object sender, EventArgs e)
        {
            int part = Part_Slector.SelectedIndex;
            Item_Data[part].IsEnabled = (bool)EnablePart_Settings.IsChecked;

            #region 基本属性
            Item_Data[part].ItemListIndex = ItemList.SelectedIndex;//物品索引
            Item_Data[part].ItemName = UI_itemName.Text;//物品名
            Item_Data[part].ItemLore = UI_itemLore.Text;//介绍文本
            Item_Data[part].ItemCount = (int)UI_itemCount.Value;//数量
            Item_Data[part].ItemDamage = (int)UI_itemDamage.Value;//附加值
            Item_Data[part].ItemUnbreakable = (bool)UI_itemUnbreakable.IsChecked;//不可破坏
            Item_Data[part].ItemHideFlags = (bool)UI_itemHideFlags.IsChecked;//隐藏标签

            Item_Data[part].IsAttriEnabled = (bool)Enable_Attribute_Settings.IsChecked;//特殊属性开关
            Item_Data[part].IsEnchEnabled = (bool)Enable_Enchant_Settings.IsChecked;//附魔属性开关
            #endregion

            Item_Data[part].EnchList.Clear();
            #region 附加属性遍历
            int index = 0;
            foreach (var numbox in grid2.Children)
            {
                if (numbox is CustomNumberBox)
                {
                    //根据uid存储
                    Item_Data[part].AttriList[index].uid = ((CustomNumberBox)numbox).Uid;
                    Item_Data[part].AttriList[index].tag = ((CustomNumberBox)numbox).Tag.ToString();
                    Item_Data[part].AttriList[index].lvl = ((CustomNumberBox)numbox).Value;
                    //匹配对应控件
                    foreach (var chkbox in grid2.Children)
                    {
                        if (chkbox is CheckBox && ((CheckBox)chkbox).Uid == ((CustomNumberBox)numbox).Uid)
                        {
                            Item_Data[part].AttriList[index].percent = (bool)((CheckBox)chkbox).IsChecked;
                        }
                    }
                    index++;
                }
            }
            #endregion

            #region 附魔属性遍历
            //从控件Tag获取id键，控件值获取键值
            foreach (var item in grid3.Children)
            {
                //仅存储有效属性的键值对
                if (item is CustomNumberBox && ((CustomNumberBox)item).Value != 0)
                {
                    Item_Data[part].EnchList.Add(((CustomNumberBox)item).Uid, (int)((CustomNumberBox)item).Value);
                }
            }
            #endregion
        }
        #endregion

        #region Viewport3D
        #region Buttons
        /// <summary> 预览视重置 </summary>        
        void Viewport_Relocation(object sender, RoutedEventArgs e)
        {
            CameraRadius = 50;
            CameraRot = new double[2] { 15, 80 };
            CameraLookAtPoint = new double[3] { 0, 15, 0 };
            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
            AxisCamera.LookDirection = MainCamera.LookDirection;
            LightDirectionReset();
        }
        #endregion

        /// <summary> 鼠标滚轮控制</summary>
        void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                CameraRadius += 2;
            else if (e.Delta > 0)
                CameraRadius -= 2;

            if (CameraRadius >= 80)
                CameraRadius = 80;
            else if (CameraRadius <= 10)
                CameraRadius = 10;

            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);//主摄像机   
        }

        #region 预览视角旋转-摄像机坐标计算
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

            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                PreviewGrid.Cursor = Cursors.SizeAll;
                CameraRot[0] += (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * 180 / 460;
                CameraRot[1] += -(e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 180 / 460;

                LightDirectionReset();
            }//右键转向
            else if (e.MiddleButton == MouseButtonState.Pressed)
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
                    CameraLookAtPoint[1] += (e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 0.08;

                MainCamera.LookDirection = new Vector3D()
                {
                    X = CameraLookAtPoint[0] - MainCamera.Position.X,
                    Y = CameraLookAtPoint[1] - MainCamera.Position.Y,
                    Z = CameraLookAtPoint[2] - MainCamera.Position.Z
                };
            }//中键平移

            Viewport_3D.CameraReset(ref MainCamera, CameraRot, CameraLookAtPoint, CameraRadius);
            AxisCamera.LookDirection = MainCamera.LookDirection;

            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;

            if (CameraRot[0] > 360) CameraRot[0] = 0;
            else if (CameraRot[0] < 0) CameraRot[0] = 360;
            if (CameraRot[1] > 175) CameraRot[1] = 175;
            else if (CameraRot[1] < 5) CameraRot[1] = 5;
        }

        /// <summary> 方向光源计算 </summary>
        void LightDirectionReset()
        {
            PerspectiveCamera mark = new PerspectiveCamera();
            Viewport_3D.CameraReset(ref mark, new double[] { CameraRot[0] + 45, CameraRot[1] }, new double[3], 10);

            DirectionalLight.Direction = new Vector3D()
            {
                X = mark.LookDirection.Z,
                Y = -5,
                Z = -mark.LookDirection.X
            };
        }
        #endregion
        #endregion

        void Pose_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!loading) ChangePose(sender, e);
        }

        /// <summary> 3D视窗反馈 </summary>
        void setPose()
        {
            HeadRotX.Angle = -pose[0];
            HeadRotY.Angle = -pose[1];
            HeadRotZ.Angle = -pose[2];

            ChestRotX.Angle = -pose[3];
            ChestRotY.Angle = -pose[4];
            ChestRotZ.Angle = -pose[5];

            LeftArmRotX.Angle = -pose[6];
            LeftArmRotY.Angle = -pose[7];
            LeftArmRotZ.Angle = -pose[8];

            RightArmRotX.Angle = -pose[9];
            RightArmRotY.Angle = -pose[10];
            RightArmRotZ.Angle = -pose[11];

            LeftLegRotX.Angle = -pose[12];
            LeftLegRotY.Angle = -pose[13];
            LeftLegRotZ.Angle = -pose[14];

            RightLegRotX.Angle = -pose[15];
            RightLegRotY.Angle = -pose[16];
            RightLegRotZ.Angle = -pose[17];

            Rotation.Angle = -pose[18];
        }

        /// <summary> 预览-模式衔接 动作结果反馈 </summary>
        public void ChangePose(object sender, RoutedEventArgs e)
        {
            //判断动作选项卡的索引值
            int i = Pose_Selector.SelectedIndex * 3;

            if (!(bool)UI_advancedmode.IsChecked)
            {
                //将控件值存储到变量
                double X = X_Pose.Value,
                       Y = Y_Pose.Value,
                       Z = Z_Pose.Value;

                pose[18] = Rotation_Setting.Value;
                if ((bool)Pose_Settings.IsChecked)
                {
                    pose[i] = X;
                    pose[i + 1] = Y;
                    pose[i + 2] = Z;
                }
            }//普通模式
            else
            {
                TimeAxis.Tick = TimeAxis.Tick;
                //若模型需要改变动作，且时间轴不在播放状态
                if (poseChange && !TimeAxis.IsPlaying)
                    Rotation_Setting.Value = TimeAxis.FramePose.pos[18];//从控件中获取当前朝向

                //无论动作是否改变，动作的存储值必须与控件同步
                TimeAxis.FramePose.pos[18] = Rotation_Setting.Value;

                //将改变后的模型参数，赋值给与显示动作关联的变量
                pose[18] = TimeAxis.FramePose.pos[18];

                //将控件选中的帧数据 赋值给与显示动作关联的变量
                for (int j = 0; j < 19; j++)
                    pose[j] = TimeAxis.FramePose.pos[j];

                //若动作选项已启用
                if ((bool)Pose_Settings.IsChecked)
                {
                    //若模型需要改变动作，且时间轴不在播放状态
                    if (poseChange && !TimeAxis.IsPlaying)
                    {
                        //将控件中的属性赋值给 <需要显示> 的控件
                        X_Pose.Value = TimeAxis.FramePose.pos[i];
                        Y_Pose.Value = TimeAxis.FramePose.pos[i + 1];
                        Z_Pose.Value = TimeAxis.FramePose.pos[i + 2];
                    }

                    //无论动作是否改变，动作的存储值必须与控件同步
                    TimeAxis.FramePose.pos[i] = X_Pose.Value;
                    TimeAxis.FramePose.pos[i + 1] = Y_Pose.Value;
                    TimeAxis.FramePose.pos[i + 2] = Z_Pose.Value;
                }
                poseChange = false;
            }//高级模式
            setPose();
        }
    }
}
