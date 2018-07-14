using ArmorStand.CustomControl;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Adaptable_Studio
{
    /// <summary> armor_stand_Page.xaml 的交互逻辑</summary>
    public partial class Armor_stand_Page : Page
    {
        #region Define
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径
        #region EquipItem
        Json ItemName;
        int part = 0;//当前编辑部位
        public struct ItemData
        {
            public bool IsEnabled;
            public string[] Content;
            public bool[] Bool;
            public double[] Value;
            public object[] Tag;
        }
        //0-头,1-躯干,2-主手,3-副手,4-腿部,5-脚部
        ItemData[] Item_Data = new ItemData[6];//分部位物品详细数据
        string[] Item_PartData = new string[6];//分部位物品输出结果
        string[,] OutPutItem = new string[6, 28];//物品数据输出变量
        string[] ItemData_List = new string[]
        {
            "id:\"<value>\",",
            "Count:<value>b,",
            "Damage:<value>s,",
            "Unbreakable:<value>b,",
            "HideFlags:<value>,",
            "<value>",//附加属性开关
            "<value>",//附魔属性开关
            //display:{
	        "Name:\"{\\\"text\\\":\\\"<value>\\\"}\",",
            "Lore:[\"<value>\"]",
            //},
            //AttributeModifiers:[
	        "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.attackDamage,Name:Attack,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.followRange,Name:FollowRange,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.maxHealth,Name:MaxHealth,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.knockbackResistance,Name:KnockbackResistance,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.movementSpeed,Name:Speed,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.luck,Name:Luck,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.armor,Name:Armor,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.attackSpeed,Name:AttackSpeed,",
            "Operation:<value>},",
            "{UUIDLeast:-163254654,UUIDMost:-163254654,Amount:<value>,AttributeName:generic.armorToughness,Name:Toughness,",
            "Operation:<value>},",
            //]
            //ench:[
	        "{id:<tag>,lvl:<value>},"
            //]
        };//物品数据生成索引
        #endregion

        #region Viewport3D
        double CameraRadius = 15;//摄像机半径(相对于原点)
        double[] CameraRot = new double[2] { 15, 60 };//水平旋转角,竖直旋转角(相对于原点)        
        double[] CameraLookAtPoint = new double[3] { 0, 0, 0 };//摄像机视点

        double[] mouse_location = new double[2];//鼠标位置

        float[] pose = new float[19];
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

        public Armor_stand_Page()
        {
            InitializeComponent();
            for (int i = 0; i < 6; i++)
            {
                Item_Data[i].IsEnabled = new bool();
                Item_Data[i].Content = new string[30];
                Item_Data[i].Bool = new bool[30];
                Item_Data[i].Value = new double[50];
                Item_Data[i].Tag = new object[30];
                Item_Data[i].Value[0] = 1;
            }//物品部位-结构参数初始化
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();

            LogPath = AppPath + @"\log.txt";

            //引导页面
            if (MainWindow.Guiding)
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
            MainWindow.Log_Write(LogPath, "[masc]Viewport3D初始化");
            CameraReset();
            MainWindow.Log_Write(LogPath, "[Viewport3D]初始化完成");
            #endregion           

            #region json列表获取
            try
            {
                StreamReader line = null;
                //判断版本读取对应json
                switch (true)
                {
                    default:
                        try
                        {
                            line = new StreamReader(AppPath + @"\json\masc\1_12.json");
                            string JSONcontent = line.ReadToEnd();
                            ItemName = JsonConvert.DeserializeObject<Json>(JSONcontent);
                        }
                        catch { MainWindow.Log_Write(LogPath, "json文件读取异常，文件丢失或损坏"); }
                        break;//1.12
                }
                line.Close();

                //导入物品列表
                if (MainWindow._langCN)
                    foreach (var item in ItemName.CN) ItemList.Items.Add(new TextBlock() { Text = item });
                else
                {
                    foreach (var item in ItemName.EN)
                    {
                        string itemName = item.Replace("minecraft:", "");
                        ItemList.Items.Add(new TextBlock() { Text = itemName });
                    }
                }
                ItemList.SelectedIndex = 0;
                MainWindow.Log_Write(LogPath, "[masc]物品列表json读取完成");
            }
            catch { MainWindow.Log_Write(LogPath, "[masc]json读取失败"); }
            #endregion
        }

        #region 页面交互
        Thickness thick = new Thickness();
        private void Main_Settings(object sender, RoutedEventArgs e)
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

        private void Nbt_Settings(object sender, RoutedEventArgs e)
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

        private void Rotation_Settings(object sender, RoutedEventArgs e)
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

        private void Item_Settings(object sender, RoutedEventArgs e)
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

        private void SelectBar_Move(object sender, ScrollChangedEventArgs e)
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

        private void PostTabChanged(object sender, SelectionChangedEventArgs e)
        {
            float X = 0, Y = 0, Z = 0;
            int index = Pose_Slector.SelectedIndex;

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
        private void PoseReset_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Pose_Reset.IsConfirmRest)
            {
                for (int i = 0; i < 19; i++) pose[i] = 0;
                X_Pose.Value = 0;
                Y_Pose.Value = 0;
                Z_Pose.Value = 0;
                Pose_Reset.IsConfirmRest = false;
            }
        }

        /// <summary> 装备UI重置 </summary>        
        private void ItemReset_LeftButtonDown(object sender, MouseButtonEventArgs e)
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
                        ((TextBox)item).Text = String.Empty;
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

        /// <summary> 指令输出</summary>
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            //结构初始化
            MainWindow.k = 0;
            for (int i = 0; i < 10000; i++) { MainWindow.commands[i] = ""; }//NBT框架搭建数据              
            MainWindow.StructureNbt.Clear();

            string result = String.Empty;
            MainWindow.result = String.Empty;
            if (!UI_advancedmode.IsChecked)
            {
                result = "summon armor_stand ~ ~1 ~ {";

                #region  基础NBT输出判定
                if (UI_name.Text != String.Empty) result += "CustomName:\"{\\\"text\\\":\\\"" + UI_name.Text + "\\\"}\",";
                if (UI_tags.Text != String.Empty) result += "Tags:[\"" + UI_tags.Text + "\"],";
                if ((bool)disabled_hand.IsChecked | (bool)disabled_head.IsChecked | (bool)disabled_chest.IsChecked | (bool)disabled_legs.IsChecked | (bool)disabled_boots.IsChecked)
                {
                    double a = 0; int b;
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
                if ((bool)Pose_Settings.IsChecked)
                    result += "Pose:{Head:["
                        + pose[0].ToString("0.#") + "f," + pose[1].ToString("0.#") + "f," + pose[2].ToString("0.#") + "f],Body:["
                        + pose[3].ToString("0.#") + "f," + pose[4].ToString("0.#") + "f," + pose[5].ToString("0.#") + "f],LeftArm:["
                        + pose[6].ToString("0.#") + "f," + pose[7].ToString("0.#") + "f," + pose[8].ToString("0.#") + "f],RightArm:["
                        + pose[9].ToString("0.#") + "f," + pose[10].ToString("0.#") + "f," + pose[11].ToString("0.#") + "f],LeftLeg:["
                        + pose[12].ToString("0.#") + "f," + pose[13].ToString("0.#") + "f," + pose[14].ToString("0.#") + "f],RightLeg:["
                        + pose[15].ToString("0.#") + "f," + pose[16].ToString("0.#") + "f," + pose[17].ToString("0.#") + "f]},";
                if (Rotation_Setting.Value != 0) result += "Rotation:[" + pose[18].ToString("0.#") + "f," + "0f],";
                #endregion

                #region 物品数据处理
                for (int r = 0; r < 6; r++)
                {
                    for (int i = 0; i < 28; i++) OutPutItem[r, i] = ItemData_List[i];//数组数据初始化

                    Item_PartData[r] = null;
                    if (Item_Data[r].IsEnabled)
                    {
                        //Text-i,Bool-j,Value-k,Tag-t
                        int i = 0, j = 0, k = 0, t = 0, index = 0;

                        while (index < 28)
                        {
                            if (index == 7) Item_PartData[r] += "tag:{display:{";
                            else if (index == 9)
                            {
                                Item_PartData[r] += "},";
                                if (bool.Parse(OutPutItem[r, 5])) Item_PartData[r] += "AttributeModifiers:[";
                            }
                            else if (index == 27)
                            {
                                if (bool.Parse(OutPutItem[r, 5])) Item_PartData[r] += "],";
                                if (bool.Parse(OutPutItem[r, 6])) Item_PartData[r] += "ench:[";
                            }

                            #region 赋值操作
                            if (index == 0)
                                OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", ItemName.EN[ComboBox_Index[r]]);

                            //Count + Damage
                            else if (index == 1 || index == 2)
                                OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Value[k].ToString());

                            //Unbreakable + AttributeModifiers/Ench
                            else if (index == 3 || index == 5 || index == 6)
                                OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Bool[j].ToString());

                            //HideFlags
                            else if (index == 4)
                                OutPutItem[r, index] = Item_Data[r].Bool[1] ? OutPutItem[r, index].Replace("<value>", "63") : OutPutItem[r, index].Replace("<value>", "0");

                            //Display Name + Lore
                            else if (index == 7 || index == 8)
                                OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Content[i]);

                            //AttributeModifiers-Value
                            else if (index >= 9 && index <= 25 && index % 2 != 0)
                            { if (bool.Parse(OutPutItem[r, 5])) OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Value[k].ToString()); }

                            //AttributeModifiers-Percent
                            else if (index >= 10 && index <= 26 && index % 2 == 0)
                            { if (bool.Parse(OutPutItem[r, 5])) OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Bool[j].ToString()); }

                            //Enchant
                            else if (index == 27)
                            {
                                if (bool.Parse(OutPutItem[r, 6]))
                                {
                                    for (k = k; k < 50; k++)
                                    {
                                        if (Item_Data[r].Value[k] != 0)
                                        {
                                            OutPutItem[r, index] = ItemData_List[index];
                                            OutPutItem[r, index] = OutPutItem[r, index].Replace("<tag>", Item_Data[r].Tag[t].ToString());
                                            t++;
                                            OutPutItem[r, index] = OutPutItem[r, index].Replace("<value>", Item_Data[r].Value[k].ToString());
                                            k++;
                                            Item_PartData[r] += OutPutItem[r, index];
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Print result
                            if (index >= 0 && index <= 2)
                                Item_PartData[r] += OutPutItem[r, index];

                            else if (index == 7 || index == 8)
                            { if (Item_Data[r].Content[i] != String.Empty) Item_PartData[r] += OutPutItem[r, index]; }

                            else if (index == 3 || index == 4)
                            { if (Item_Data[r].Bool[j]) Item_PartData[r] += OutPutItem[r, index]; }
                            //AttributeModifiers-Value
                            else if (index >= 9 && index <= 25 && index % 2 != 0 && bool.Parse(OutPutItem[r, 5]) && Item_Data[r].Value[k] != 0)
                                Item_PartData[r] += OutPutItem[r, index];
                            //AttributeModifiers-Percent
                            else if (index >= 10 && index <= 26 && index % 2 == 0 && bool.Parse(OutPutItem[r, 5]) && Item_Data[r].Value[k - 1] != 0)
                                Item_PartData[r] += OutPutItem[r, index];
                            #endregion

                            #region Index stats
                            //Content
                            if (index == 7 || index == 8)
                                i++;
                            //Value
                            else if (index == 1 || index == 2 || (index >= 9 && index <= 25 && index % 2 != 0))
                                k++;
                            //Bool
                            else if (index == 3 || (index >= 4 && index <= 6) || index >= 10 && index <= 26 && index % 2 == 0)
                                j++;
                            #endregion
                            index++;
                        }
                        if (bool.Parse(OutPutItem[r, 6])) Item_PartData[r] += "]";
                        Item_PartData[r] += "}";
                    }
                }
                #endregion

                #region 部位输出
                //0-头,1-躯干,2-主手,3-副手,4-腿部,5-脚部
                if (Item_Data[2].IsEnabled || Item_Data[3].IsEnabled)
                    result += "HandItems:[{"
                        + (Item_Data[2].IsEnabled ? Item_PartData[2] : null) + "},{"
                        + (Item_Data[3].IsEnabled ? Item_PartData[3] : null) + "}],";

                if (Item_Data[0].IsEnabled || Item_Data[1].IsEnabled || Item_Data[4].IsEnabled || Item_Data[5].IsEnabled)
                    result += "ArmorItems:[{"
                        + (Item_Data[5].IsEnabled ? Item_PartData[5] : null) + "},{"
                        + (Item_Data[4].IsEnabled ? Item_PartData[4] : null) + "},{"
                        + (Item_Data[1].IsEnabled ? Item_PartData[1] : null) + "},{"
                        + (Item_Data[0].IsEnabled ? Item_PartData[0] : null) + "}]";
                #endregion

                result += "}";
                Add_ones(result, false);
            }//常规模式
            else
            {
                string tag = String.Empty;

                tag = UI_tags.Text;

                for (int i = 0; i < TimeAxis.TotalTick; i++)
                {
                    result = "data merge entity @e[type=armor_stand,limit=1";
                    if (UI_tags.Text != String.Empty) result += ",tag=" + tag;
                    result += ",scores={" + "ScoreName=" + i.ToString() + "}"
                     + "] {";

                    if ((bool)Pose_Settings.IsChecked) result += "Pose:{Head:["
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
                    Add_ones(result, true);
                }//时间轴数据

                //加分
                result = "scoreboard players add @e";
                if (UI_tags.Text != String.Empty) result += "[tag=" + tag + "]";
                result += " ScoreName 1";
                Add_ones(result, true);
            }//高级模式            
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Restart = true;
            IniWrite("System", "PageIndex", "0", iniPath);
            Process.Start(Assembly.GetExecutingAssembly().Location);//重新开启当前程序
            Application.Current.Shutdown();//关闭当前程序
        }

        /// <summary> 结构文件-单条存储 </summary>
        /// <param name="Ones">添加的字符</param>
        /// <param name="Add">是否为添加模式（否则直接替换结果）</param>
        private static void Add_ones(string Ones, bool Add = false)
        {
            Ones = Ones.Replace("False", "0");
            Ones = Ones.Replace("True", "1");
            Ones = Ones.Replace(",]", "]");
            Ones = Ones.Replace(",}", "}");
            MainWindow.commands[MainWindow.k] = Ones;
            MainWindow.k++;

            if (Add) MainWindow.result += Ones + "\r\n";
            else MainWindow.result = Ones + "\r\n";
        }

        #region 装备页面
        int[] ComboBox_Index = new int[6];//部位物品索引值
        /// <summary> 物品部位切换/读取参数 </summary>
        private void Item_TabChanged(object sender, SelectionChangedEventArgs e)
        {
            part = Part_Slector.SelectedIndex;
            EnablePart_Settings.IsChecked = Item_Data[part].IsEnabled;
            //页面Index索引,Text-i,Bool-j,Value-k
            int i = 0, j = 0, k = 0;
            #region 基本属性遍历        
            foreach (var item in grid1.Children)
            {
                if (item is ComboBox)
                    ((ComboBox)item).SelectedIndex = ComboBox_Index[part];
                else if (item is CustomNumberBox)
                {
                    ((CustomNumberBox)item).Value = Item_Data[part].Value[k];
                    k++;
                }
            }//物品名,附加值,数量

            foreach (var item in grid1.Children)
            {
                if (item is CheckBox)
                {
                    ((CheckBox)item).IsChecked = Item_Data[part].Bool[j];
                    j++;
                }
            }//不可破坏,隐藏标签

            foreach (var item in grid1.Children)
            {
                if (item is TextBox)
                {
                    ((TextBox)item).Text = Item_Data[part].Content[i];
                    i++;
                }
            }//命名,介绍
            #endregion

            #region 附加属性遍历
            foreach (var item in grid2.Children)
            {
                if (item is CustomNumberBox)
                {
                    ((CustomNumberBox)item).Value = Item_Data[part].Value[k];
                    k++;
                }
                else if (item is CheckBox)
                {
                    ((CheckBox)item).IsChecked = Item_Data[part].Bool[j];
                    j++;
                }
            }//属性等级+百分比判定
            #endregion

            #region 附魔属性遍历
            foreach (var item in grid3.Children)
            {
                if (item is CustomNumberBox)
                {
                    ((CustomNumberBox)item).Value = Item_Data[part].Value[k];
                    k++;
                }
            }//Tag:附魔id,Value:附魔等级
            #endregion
        }

        /// <summary> 物品数据存储 </summary>
        private void ItemGrid_update(object sender, MouseEventArgs e)
        {
            part = Part_Slector.SelectedIndex;
            Item_Data[part].IsEnabled = (bool)EnablePart_Settings.IsChecked;
            //页面Index索引,Text-i,Bool-j,Value-k
            int i = 0, j = 0, k = 0;

            #region 基本属性遍历
            foreach (var item in grid1.Children)
            {
                if (item is ComboBox)
                {
                    ComboBox_Index[part] = ((ComboBox)item).SelectedIndex;
                }
                else if (item is CustomNumberBox)
                {
                    Item_Data[part].Value[k] = ((CustomNumberBox)item).Value;
                    k++;
                }
            }//附加值,数量

            foreach (var item in grid1.Children)
            {
                if (item is CheckBox)
                {
                    Item_Data[part].Bool[j] = (bool)((CheckBox)item).IsChecked;
                    j++;
                }
            }//不可破坏,隐藏标签

            foreach (var item in grid1.Children)
            {
                if (item is TextBox)
                {
                    Item_Data[part].Content[i] = ((TextBox)item).Text;
                    i++;
                }
            }//命名,介绍
            #endregion

            #region 附加属性遍历
            foreach (var item in grid2.Children)
            {
                if (item is CustomNumberBox)
                {
                    Item_Data[part].Value[k] = ((CustomNumberBox)item).Value;
                    k++;
                }
                else if (item is CheckBox)
                {
                    Item_Data[part].Bool[j] = (bool)((CheckBox)item).IsChecked;
                    j++;
                }
            }//属性等级+百分比判定
            #endregion

            #region 附魔属性遍历
            int t = 0;
            foreach (var item in grid3.Children)
            {
                if (item is CustomNumberBox)
                {
                    Item_Data[part].Tag[t] = ((CustomNumberBox)item).Tag;
                    t++;
                    Item_Data[part].Value[k] = ((CustomNumberBox)item).Value;
                    k++;
                }
            }//Tag:附魔id,Value:附魔等级
            #endregion
        }

        private void Equipment_check(object sender, RoutedEventArgs e)
        {
            OutPut output = new OutPut();
            output.ToNBT.IsEnabled = false;
            output.Print.Text = "#头部\n" + Item_PartData[0]
                             + "\n#胸甲\n" + Item_PartData[1]
                             + "\n#主手\n" + Item_PartData[2]
                             + "\n#副手\n" + Item_PartData[3]
                             + "\n#腿部\n" + Item_PartData[4]
                             + "\n#脚部\n" + Item_PartData[5];
            output.Show();
        }
        #endregion

        //#region OpenGL绘制
        ///// <summary> 绘制主体 </summary>
        //private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        //{
        //    OpenGL gl = args.OpenGL;
        //    gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);//深度清除缓存

        //    //光源设置
        //    gl.LoadIdentity();//重置模型观察矩阵
        //    gl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        //    gl.Translate(0f, -1.5f, -8.5f);
        //    gl.Rotate((float)rot[0], (float)rot[1], 0f);
        //    float LightX = (float)(Math.Cos(rot[1] - 45) * (Math.PI / 180)) * LightR,
        //          LightZ = (float)(Math.Sin(rot[1] - 45) * (Math.PI / 180)) * LightR;
        //    float[] light_position = new float[4] { LightX, 2f, LightZ, 1f };//光源位置
        //    gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light_position);

        //    gl.LoadIdentity();//重置模型观察矩阵

        //    //动作赋值
        //    PoseGet(args);

        //    if (TimeAxis.IsKey && UI_advancedmode.IsChecked)
        //    {
        //        gl.DrawText(1, 10, 1, 0, 0, "KeyText", 80, ".");
        //        gl.DrawText(30, 13, 1, 0, 0, "KeyText", 10, "Key");
        //    }//关键帧提示

        //    gl.Color(1f, 1f, 1f);
        //    gl.Enable(OpenGL.GL_TEXTURE_2D);//启用2D纹理映射

        //    gl.Enable(OpenGL.GL_LIGHTING);
        //    #region 底座绘制
        //    Stone.Bind(gl);
        //    gl.Translate(0f, -1.5f, -8.5f);
        //    gl.Rotate((float)rot[0], (float)rot[1], 0f);
        //    gl.Scale(GL_Scale, GL_Scale, GL_Scale);
        //    if (!(bool)UI_nobaseplate.IsChecked) OpenGL_Draw.Draw_BasePlate(gl);//显示底座            
        //    #endregion

        //    gl.Disable(OpenGL.GL_LIGHTING);
        //    #region 坐标轴绘制
        //    gl.Disable(OpenGL.GL_TEXTURE_2D);//禁用2D纹理映射
        //    gl.Rotate(0f, 0f, 0f); //X坐标轴  
        //    gl.Color(0.9f, 0.1f, 0.1f);
        //    OpenGL_Draw.Draw_Axis(gl);
        //    gl.Rotate(0f, 0f, 90.0f); //Y坐标轴
        //    gl.Color(0.1f, 0.9f, 0.1f);
        //    OpenGL_Draw.Draw_Axis(gl);
        //    gl.Rotate(0f, -90.0f, 0f);//Z坐标轴                 
        //    gl.Color(0.1f, 0.1f, 0.9f);
        //    OpenGL_Draw.Draw_Axis(gl);
        //    gl.Enable(OpenGL.GL_TEXTURE_2D);//启用2D纹理映射
        //    #endregion

        //    gl.Enable(OpenGL.GL_LIGHTING);
        //    #region 朝向旋转部分
        //    gl.Color(1f, 1f, 1f);
        //    OpenGL_reset(gl);
        //    OpenGL_Draw.Draw_arrow(gl);//箭头
        //    gl.Color(0.9f, 0.9f, 0.9f);
        //    Wood.Bind(gl);
        //    #region 头部绘制
        //    OpenGL_reset(gl);
        //    if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //    gl.Translate(0f, 2.07f, 0f);
        //    gl.Rotate(-pose[2], -pose[1], -pose[0]);
        //    OpenGL_Draw.Draw_Head(gl);
        //    #endregion

        //    #region 躯干绘制
        //    OpenGL_reset(gl);
        //    if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //    gl.Translate(0f, 1.98f, 0f);
        //    gl.Rotate(-pose[5], -pose[4], -pose[3]);
        //    OpenGL_Draw.Draw_Chest(gl);
        //    #endregion

        //    #region 双臂绘制
        //    if ((bool)UI_showarms.IsChecked)
        //    {
        //        OpenGL_reset(gl);
        //        if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //        gl.Translate(0f, 1.98f, -0.54f);
        //        gl.Rotate(-pose[8], -pose[7], -pose[6]);
        //        OpenGL_Draw.Draw_Arm(gl);
        //        OpenGL_reset(gl);
        //        if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //        gl.Translate(0f, 1.98f, 0.54f);
        //        gl.Rotate(-pose[11], -pose[10], -pose[9]);
        //        OpenGL_Draw.Draw_Arm(gl);
        //    }
        //    #endregion

        //    #region 双腿绘制
        //    OpenGL_reset(gl);
        //    if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //    gl.Translate(0f, 0.99f, -0.18f);
        //    gl.Rotate(-pose[14], -pose[13], -pose[12]);
        //    OpenGL_Draw.Draw_Leg(gl);
        //    OpenGL_reset(gl);
        //    if ((bool)UI_small.IsChecked) gl.Scale(0.6f, 0.6f, 0.6f);
        //    gl.Translate(0f, 0.99f, 0.18f);
        //    gl.Rotate(-pose[17], -pose[16], -pose[15]);
        //    OpenGL_Draw.Draw_Leg(gl);
        //    #endregion
        //    #endregion

        //    gl.Disable(OpenGL.GL_TEXTURE_2D);//禁用2D纹理映射            

        //    gl.Flush();
        //}

        ///// <summary> 坐标重置</summary>
        //public void OpenGL_reset(OpenGL gl)
        //{
        //    gl.LoadIdentity();// 重置当前模型观察矩阵
        //    gl.Translate(0f, -1.5f, -8.5f);
        //    gl.Rotate((float)rot[0], (float)rot[1], 0f);//观察角度
        //    gl.Scale(GL_Scale, GL_Scale, GL_Scale);//缩放
        //    gl.Rotate(0f, -pose[18], 0f);
        //}

        ///// <summary> 鼠标滚轮控制</summary>
        //private void OpenGLControl_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (e.Delta > 0 && GL_Scale < 3) GL_Scale += 0.02;
        //    if (e.Delta < 0 && GL_Scale > 1) GL_Scale -= 0.02;
        //}

        //#region 预览视角旋转事件
        //private void OpenGLControl_down(object sender, MouseButtonEventArgs e)
        //{
        //    preview_rot = true;
        //    mouse_location[0] = e.GetPosition((IInputElement)sender).X;
        //    mouse_location[1] = e.GetPosition((IInputElement)sender).Y;
        //}

        //private void OpenGLControl_up(object sender, MouseButtonEventArgs e)
        //{
        //    preview_rot = false;
        //}

        //private void OpenGLControl_leave(object sender, MouseEventArgs e)
        //{
        //    preview_rot = false;
        //}

        //private void OpenGLControl_rot_Change(object sender, MouseEventArgs e)
        //{
        //    if (preview_rot)
        //    {
        //        rot[1] += (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * 180 / 460;
        //        rot[0] += (e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 180 / 460;
        //        mouse_location[0] = e.GetPosition((IInputElement)sender).X;
        //        mouse_location[1] = e.GetPosition((IInputElement)sender).Y;
        //    }
        //    if (rot[0] > 90) { rot[0] = 90; }
        //    if (rot[0] < -90) { rot[0] = -90; }
        //}
        //#endregion
        //#endregion

        #region Viewport3D
        /// <summary> 鼠标滚轮控制</summary>
        private void ArmorStandView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta >= 0)
                CameraRadius += 0.5;
            else
                CameraRadius -= 0.5;

            CameraReset();

            lab.Content = "Radius:" + CameraRadius;
        }

        #region 预览视角旋转-摄像机坐标计算
        private void ArmorStandView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;

            if (e.LeftButton == MouseButtonState.Pressed) PreviewGrid.Cursor = Cursors.SizeAll;
            else if (e.RightButton == MouseButtonState.Pressed) PreviewGrid.Cursor = Cursors.ScrollAll;
        }

        private void ArmorStandView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviewGrid.Cursor = Cursors.Arrow;
        }

        Vector3D LookDir, UpDir, MoveDir;

        private void ArmorStandView_MouseMove(object sender, MouseEventArgs e)
        {
            PreviewGrid.Cursor = Cursors.Arrow;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PreviewGrid.Cursor = Cursors.SizeAll;
                CameraRot[0] += (e.GetPosition((IInputElement)sender).X - mouse_location[0]) * 180 / 460;
                CameraRot[1] += -(e.GetPosition((IInputElement)sender).Y - mouse_location[1]) * 180 / 460;

                lab.Content = "CameraRot: " + CameraRot[0] + " , " + CameraRot[1];
            }//左键转向
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                PreviewGrid.Cursor = Cursors.ScrollAll;

                //叉乘 a×b=(y1z2-y2z1,z1x2-z2x1,x1y2-x2y1)
                LookDir = MainCamera.LookDirection;//Camera朝向向量
                UpDir = new Vector3D()//Camera垂直法向量 (LookDir×(0,0,1))
                {
                    X = LookDir.Y,
                    Y = -LookDir.X,
                    Z = 0
                };

                //平面法向量                
                MoveDir.X = LookDir.Y * UpDir.Z - UpDir.Y * LookDir.Z;
                MoveDir.Y = LookDir.Z * UpDir.X - LookDir.X * UpDir.Z;
                MoveDir.Z = LookDir.X * UpDir.Y - LookDir.Y * UpDir.X;

                if (e.GetPosition((IInputElement)sender).X - mouse_location[0] > 0)
                {
                    CameraLookAtPoint[0] += -MoveDir.X / 1800;
                    CameraLookAtPoint[2] += -MoveDir.Z / 1800;
                }
                else if (e.GetPosition((IInputElement)sender).X - mouse_location[0] < 0)
                {
                    CameraLookAtPoint[0] += MoveDir.X / 1800;
                    CameraLookAtPoint[2] += MoveDir.Z / 1800;
                }

                if (e.GetPosition((IInputElement)sender).Y - mouse_location[1] > 0)
                {
                    CameraLookAtPoint[1] += 0.08;
                    MainCamera.Position = new Point3D()
                    {
                        X = MainCamera.Position.X,
                        Y = MainCamera.Position.Y + 0.08,
                        Z = MainCamera.Position.Z
                    };

                }
                else if (e.GetPosition((IInputElement)sender).Y - mouse_location[1] < 0)
                {
                    CameraLookAtPoint[1] -= 0.08;
                    MainCamera.Position = new Point3D()
                    {
                        X = MainCamera.Position.X,
                        Y = MainCamera.Position.Y - 0.08,
                        Z = MainCamera.Position.Z
                    };
                }

                MainCamera.LookDirection = new Vector3D()
                {
                    X = CameraLookAtPoint[0] - MainCamera.Position.X,
                    Y = CameraLookAtPoint[1] - MainCamera.Position.Y,
                    Z = CameraLookAtPoint[2] - MainCamera.Position.Z
                };

                lab.Content = "Camera LookAt: " + CameraLookAtPoint[0] + "," + CameraLookAtPoint[1] + "," + CameraLookAtPoint[2]
                    + "\n Look Direction:" + MainCamera.LookDirection;
            }//右键平面移动

            CameraReset();
            mouse_location[0] = e.GetPosition((IInputElement)sender).X;
            mouse_location[1] = e.GetPosition((IInputElement)sender).Y;

            if (CameraRot[0] > 360) CameraRot[0] = 0;
            else if (CameraRot[0] < 0) CameraRot[0] = 360;
            if (CameraRot[1] > 175) CameraRot[1] = 175;
            else if (CameraRot[1] < 5) CameraRot[1] = 5;
        }

        void CameraReset()
        {
            //主视窗Camera
            Point3D Point = new Point3D()
            {
                X = CameraLookAtPoint[0] + Math.Sin(CameraRot[1] * Math.PI / 180) * Math.Cos(CameraRot[0] * Math.PI / 180) * CameraRadius,
                Y = CameraLookAtPoint[1] + Math.Cos(CameraRot[1] * Math.PI / 180) * CameraRadius,
                Z = CameraLookAtPoint[2] + Math.Sin(CameraRot[1] * Math.PI / 180) * Math.Sin(CameraRot[0] * Math.PI / 180) * CameraRadius
            };
            MainCamera.Position = Point;
            MainCamera.LookDirection = new Vector3D() { X = -Point.X + CameraLookAtPoint[0], Y = -Point.Y + CameraLookAtPoint[1], Z = -Point.Z + CameraLookAtPoint[2] };

            //坐标系视窗Camera
            double AxisRadius = 10;
            Point = new Point3D()
            {
                X = Math.Sin(CameraRot[1] * Math.PI / 180) * Math.Cos(CameraRot[0] * Math.PI / 180) * AxisRadius,
                Y = Math.Cos(CameraRot[1] * Math.PI / 180) * AxisRadius,
                Z = Math.Sin(CameraRot[1] * Math.PI / 180) * Math.Sin(CameraRot[0] * Math.PI / 180) * AxisRadius
            };
            AxisCamera.Position = Point;
            AxisCamera.LookDirection = new Vector3D() { X = -Point.X, Y = -Point.Y, Z = -Point.Z };
        }
        #endregion
        #endregion

        public static bool poseChange;
        private void PoseGet(EventArgs e)
        {
            float X = (float)X_Pose.Value,
                  Y = (float)Y_Pose.Value,
                  Z = (float)Z_Pose.Value;
            int i = 0;
            if (PoseHead.IsSelected) i = 0;
            else if (PoseBody.IsSelected) i = 3;
            else if (PoseLeftArm.IsSelected) i = 6;
            else if (PoseRightArm.IsSelected) i = 9;
            else if (PoseLeftLeg.IsSelected) i = 12;
            else if (PoseRightLeg.IsSelected) i = 15;

            if (!UI_advancedmode.IsChecked)
            {
                pose[18] = (float)Rotation_Setting.Value;
                if ((bool)Pose_Settings.IsChecked)
                {
                    pose[i] = X;
                    pose[i + 1] = Y;
                    pose[i + 2] = Z;
                }
            }//普通模式
            else
            {
                if (poseChange && !TimeAxis.IsPlaying) Rotation_Setting.Value = TimeAxis.FramePose.pos[18];
                TimeAxis.FramePose.pos[18] = (float)Rotation_Setting.Value;
                pose[18] = TimeAxis.FramePose.pos[18];

                if ((bool)Pose_Settings.IsChecked)
                {
                    if (poseChange && !TimeAxis.IsPlaying)
                    {
                        X_Pose.Value = TimeAxis.FramePose.pos[i];
                        Y_Pose.Value = TimeAxis.FramePose.pos[i + 1];
                        Z_Pose.Value = TimeAxis.FramePose.pos[i + 2];
                    }

                    TimeAxis.FramePose.pos[i] = (float)X_Pose.Value;
                    TimeAxis.FramePose.pos[i + 1] = (float)Y_Pose.Value;
                    TimeAxis.FramePose.pos[i + 2] = (float)Z_Pose.Value;

                    //控件属性赋值于滑条数据
                    pose[i] = TimeAxis.FramePose.pos[i];
                    pose[i + 1] = TimeAxis.FramePose.pos[i + 1];
                    pose[i + 2] = TimeAxis.FramePose.pos[i + 2];

                    for (int j = 0; j < 19; j++) pose[j] = TimeAxis.FramePose.pos[j];//控件属性赋值于实时变量
                }
                poseChange = false;
            }//高级模式
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