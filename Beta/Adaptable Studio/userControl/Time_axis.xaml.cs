using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Adaptable_Studio.PublicControl;

namespace Adaptable_Studio
{
    /// <summary> Time_axis.xaml 的交互逻辑 </summary>
    public partial class Time_axis : UserControl
    {
        Line timePoint = new Line(),//时间线
             timeScale = new Line(),//刻度+隔板
             keyFrame = new Line(),//关键帧
             transition = new Line();//过渡段

        bool mousedown;
        //0~2:head
        //3~5:body
        //6~8:left arm
        //9~11:right arm
        //12~14:left leg
        //15~17:right leg
        //18:rotation
        public struct Pose
        {
            public double[] pos;
            public bool key;
        }//时间轴结构体
        public Pose[] pose = new Pose[32767];//时间轴数据存储

        /// <summary> 当前帧 方向数据缓存 </summary>
        public static bool[] OneReversed = new bool[19];
        /// <summary> 全局 部位方向数据 </summary>
        public static bool[][] IsReversed = new bool[32767][];

        DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 50) };//播放

        #region 暂存数据
        /// <summary> 当前帧位置缓存 </summary>
        public long ClickMark = 0;
        Pose MarkPose = new Pose();//帧结构数据缓存
        #endregion

        #region 自定义属性
        /// <summary> (只读)全局数据结构体,存储控件的所有帧数据 </summary>
        [Description("(只读)全局数据结构体"), Category("User Control")]
        public Pose[] PoseStructure
        {
            get { return pose; }
        }

        /// <summary> 当前帧数据结构体，存储当前选中帧的动作数据 </summary>
        [Description("当前数据结构体"), Category("User Control")]
        public Pose FramePose
        {
            get { return pose[Tick]; }
            set { pose[Tick] = value; }
        }

        /// <summary> 当前Tick值 </summary>
        [Description("当前Tick值"), Category("User Control")]
        public long Tick { get; set; }

        /// <summary> Tick上限值 </summary>
        [Description("Tick上限值"), Category("User Control")]
        public long TotalTick
        {
            get { return (long)(Canvas.Width / KeyWidth); }
            set
            {
                Canvas.Width = value * KeyWidth + KeyWidth / 2;
                TimeGrid.Width = value * KeyWidth + KeyWidth / 2;
            }
        }

        /// <summary> (只读)判断当前帧是否为关键帧 </summary>
        [Description("(只读)判断当前帧是否为关键帧"), Category("User Control")]
        public bool IsKey
        {
            get { return pose[Tick].key; }
        }

        /// <summary> 判断时间轴是否为正在运行 </summary>
        public bool _isplaying;
        [Description("判断时间轴是否为正在运行"), Category("User Control")]
        public bool IsPlaying
        {
            get { return _isplaying; }
            set { _isplaying = play_pause_button.Pressed = value; }
        }
        #endregion

        public Time_axis()
        {
            InitializeComponent();
            timer.Tick += PlayTimer;//timer事件
            pose[0].key = true;
            for (int i = 0; i < 32767; i++)
            {
                pose[i].pos = new double[19];
                IsReversed[i] = new bool[19];
            } //动作数据初始化
        }

        private void First_KeyFrame(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Tick = 0;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (TotalTick < Canvas.Width)
                TotalTick = (long)(Canvas.Width / KeyWidth);
            TimeGridRedraw(sender, e);
            //时间线
            timePoint = new Line()
            {
                Stroke = Brushes.White,
                StrokeThickness = 1,//线条宽度
                X1 = (Tick + 1) * KeyWidth - KeyWidth / 2,
                X2 = (Tick + 1) * KeyWidth - KeyWidth / 2,
                Y1 = 0,
                Y2 = TimeGrid.ActualHeight,
                IsHitTestVisible = false
            };
            TimeGrid.Children.Add(timePoint);

            pose[0].key = true;
        }

        private void Control_Focus(object sender, RoutedEventArgs e)
        {
            KeyChanged(sender, e);
        }

        private void Scroll_Changed(object sender, ScrollChangedEventArgs e)
        {
            double s = e.HorizontalOffset;
            double amount = 3.5;
            //double amount = 1.5;
            if (s <= 0.5 * amount) Shadow_1.Opacity = 0;
            else if (s <= 1 * amount) Shadow_1.Opacity = 0.1;
            else if (s <= 1.5 * amount) Shadow_1.Opacity = 0.2;
            else if (s <= 2 * amount) Shadow_1.Opacity = 0.3;
            else if (s <= 2.5 * amount) Shadow_1.Opacity = 0.4;
            else if (s <= 3 * amount) Shadow_1.Opacity = 0.5;
            else if (s <= 3.5 * amount) Shadow_1.Opacity = 0.6;
            else if (s <= 4 * amount) Shadow_1.Opacity = 0.7;
            else if (s <= 4.5 * amount) Shadow_1.Opacity = 0.8;
            else if (s <= 5 * amount) Shadow_1.Opacity = 0.9;
            else Shadow_1.Opacity = 1;
        }

        /// <summary> 关键帧补间数据运算 </summary>
        private void KeyChanged(object sender, RoutedEventArgs e)
        {
            int start, end = 0,//补间始末数据
                mark = 0,//运算标记
                TimeIndex = 0,//时间轴起点                              
                TransitionIndex = 0;//过渡段索引

            while (TimeIndex < TotalTick)
            {
                start = mark;
                for (TimeIndex = mark + 1; TimeIndex < TotalTick; TimeIndex++)
                {
                    if (pose[TimeIndex].key)
                    {
                        end = TimeIndex;
                        mark = TimeIndex;
                        break;
                    }
                }//提取两个相邻关键帧数据↓

                int TickDelay = end - start + 1;//tick间隔
                if (TickDelay > 0)
                {
                    //p:独立补间区间
                    //q:pos[0~18]
                    for (int p = start; p < end; p++)
                    {
                        for (int q = 0; q < 19; q++)
                        {
                            pose[p].pos[q] = pose[start].pos[q];
                            //判断反向旋转
                            if (IsReversed[TransitionIndex][q])
                            {
                                double Alpha, Beta;
                                if (pose[start].pos[q] >= pose[end].pos[q])
                                {
                                    Alpha = pose[start].pos[q];
                                    Beta = pose[end].pos[q];
                                }
                                else
                                {
                                    Beta = pose[start].pos[q];
                                    Alpha = pose[end].pos[q];
                                }

                                double DeltaAngel = (360 - Math.Abs(Alpha - Beta)) / TickDelay;

                                if (pose[start].pos[q] < pose[end].pos[q]) DeltaAngel = -DeltaAngel;

                                pose[p].pos[q] = (p - start + 1) * DeltaAngel;
                                while (pose[p].pos[q] < -180) pose[p].pos[q] += 360;
                                while (pose[p].pos[q] > 180) pose[p].pos[q] -= 360;
                            }
                            else
                            {
                                //pose[p].pos[q] = pose[start].pos[q];
                                //每元素平均增量
                                if (p != start)
                                    pose[p].pos[q] += (p - start + 1) * (pose[end].pos[q] - pose[start].pos[q]) / TickDelay;
                            }
                        }
                    }

                }
                TransitionIndex++;
            }

            //多余数据重置
            for (TimeIndex = end + 1; TimeIndex < TotalTick; TimeIndex++)
            {
                for (int j = 0; j < 19; j++)
                    pose[TimeIndex].pos[j] = pose[end].pos[j];
            }

            Armor_stand_Page.poseChange = true;
            Page_masc.ChangePose(sender, e);
        }

        #region UI绘制模板     
        /// <summary> 时间线位移 </summary>
        private void TimeLine(long tick)
        {
            timePoint.X1 = (tick + 1) * KeyWidth - KeyWidth / 2;
            timePoint.X2 = (tick + 1) * KeyWidth - KeyWidth / 2;
        }

        /// <summary> 新建关键帧 </summary>
        private void NewKeyFrame(ref Line keyFrame, double X, double Y, double d, string tag = null)
        {
            keyFrame = new Line()
            {
                Tag = tag,
                Stroke = Brushes.LightCyan,
                StrokeThickness = lineWidth,//线条宽度
                X1 = X - d,
                X2 = X + d,
                Y1 = Y - d,
                Y2 = Y + d,
                ContextMenu = (ContextMenu)Resources["KeyFrameMenu"],
                IsHitTestVisible = false
            };
            keyFrame.MouseDown += KeyFrameMouseDown;

        }
        #endregion

        const float lineWidth = 6f,//关键帧线宽
                    top = 25f,//关键帧到顶部距离
                    KeyWidth = 20f;//帧宽度        
        #region Grid元素绘制/事件
        /// <summary> 控件UI重绘 </summary>        
        private void TimeGridRedraw(object sender, EventArgs e)
        {
            #region Clear
            //倒序索引删除 刻度+标识
            int index = Canvas.Children.Count;
            for (int i = index - 1; i >= 0; i--)
            {
                if (Canvas.Children[i] is Line)
                    Canvas.Children.Remove(Canvas.Children[i]);
                else if (Canvas.Children[i] is TextBlock)
                    Canvas.Children.Remove(Canvas.Children[i]);
            }

            //倒序索引删除 时间线+关键帧+过渡段
            index = TimeGrid.Children.Count;
            for (int i = index - 1; i >= 0; i--)
            {
                if (TimeGrid.Children[i] is Line)
                    TimeGrid.Children.Remove(TimeGrid.Children[i]);
            }
            #endregion

            int t = 0;//关键帧偶数量检测
            int TransitionIndex = 0;//过渡段索引
            int start = 0, end = 0;//相邻帧位置记录

            for (int i = 0; i <= TotalTick - 1; i++)
            {
                //添加刻度-tick
                timeScale = new Line()
                {
                    Stroke = Brushes.LightSteelBlue,
                    StrokeThickness = 0.5,//宽度
                    X1 = (i + 1) * KeyWidth - KeyWidth / 2,
                    X2 = (i + 1) * KeyWidth - KeyWidth / 2,
                    Y1 = 0,
                    Y2 = 8,
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(timeScale);

                //添加隔板
                timeScale = new Line()
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 17, 158, 218)),
                    StrokeThickness = 0.8,//宽度
                    X1 = (i + 1) * KeyWidth,
                    X2 = (i + 1) * KeyWidth,
                    Y1 = 0,
                    Y2 = TimeGrid.ActualHeight,
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(timeScale);

                //添加刻度数字标识
                int length = i.ToString().Length;//刻度数字位数
                TextBlock tb = new TextBlock()
                {
                    IsEnabled = false,
                    Margin = new Thickness((i + 1) * KeyWidth - KeyWidth / 2 - 2.2 * length, 7, Margin.Right, Margin.Bottom),
                    Text = i.ToString(),
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(tb);

                //添加关键帧                
                double a = lineWidth * Math.Sin(Math.PI / 4) / 2;//菱形水平始末间距
                if (pose[i].key)
                {
                    NewKeyFrame(ref keyFrame, (i + 0.5) * KeyWidth, top, a, i.ToString());//新建关键帧
                    TimeGrid.Children.Add(keyFrame);
                    t++;
                    end = i;
                }

                //添加过渡段
                if (t != 0 && t % 2 == 0)
                {
                    transition = new Line()
                    {
                        Tag = "transition" + TransitionIndex,
                        Stroke = Brushes.LightCyan,
                        StrokeThickness = 4.5,//宽度
                        X1 = (start + 1) * KeyWidth - KeyWidth / 5,
                        X2 = (end + 1) * KeyWidth - 4 * KeyWidth / 5,
                        Y1 = top,
                        Y2 = top,
                        ContextMenu = new TimeAxis_ContextMenu(IsReversed, OneReversed, ClickMark)
                    };
                    Panel.SetZIndex(transition, 0);
                    transition.MouseRightButtonDown += Transition_MouseRightButtonDown;
                    TimeGrid.Children.Add(transition);
                    TransitionIndex++;
                    t = 1;//数量统计清空
                    start = i;//更改起始计算点
                }
            }

            //添加帧定位指针
            TimeGrid.Children.Remove(timePoint);
            TimeLine(Tick);
            TimeGrid.Children.Add(timePoint);
        }

        private void TimeGridResized(object sender, SizeChangedEventArgs e)
        {
            TimeGridRedraw(sender, e);
        }

        private void TimeGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Armor_stand_Page.poseChange = true;
        }

        private void TimeGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Armor_stand_Page.poseChange = true;
        }

        private void TimeGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            //时间轴暂停
            play_pause_button.Pressed = false;
            timer.Stop();
            long Position = 0;//鼠标对应帧位置
            for (int i = 0; i < TotalTick; i++)
            {
                if (e.GetPosition(TimeGrid).X >= i * KeyWidth && e.GetPosition(TimeGrid).X <= i * KeyWidth + KeyWidth)
                    Position = (long)(i * KeyWidth + KeyWidth / 2);
            }//计算鼠标对应帧位置

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mousedown = true;
                Tick = (long)(Position / KeyWidth);
            }//鼠标左键按下
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                pose[(int)(Position / KeyWidth - 0.5)].key = true;//关键帧位置
            }//鼠标右键按下

            TimeGridRedraw(sender, e);
            KeyChanged(sender, e);
        }

        private void TimeGridMouseMove(object sender, MouseEventArgs e)
        {
            //重绘时间线
            if (mousedown)
            {
                for (int i = 0; i < TotalTick; i++)
                {
                    if (e.GetPosition(TimeGrid).X >= i * KeyWidth && e.GetPosition(TimeGrid).X <= i * KeyWidth + KeyWidth)
                        Tick = (long)(i + 0.5);
                }
                TimeLine(Tick);
            }
            Page_masc.ChangePose(sender, e);
        }

        private void TimeGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;

            //启用所有关键帧点击反馈
            for (int i = 0; i < TotalTick; i++)
            {
                foreach (Line item in TimeGrid.Children)
                    if (item.Tag != null && item.Tag.ToString() == i.ToString())
                        item.IsHitTestVisible = true;
            }
        }

        private void TimeGridMouseLeave(object sender, MouseEventArgs e)
        {
            mousedown = false;
        }

        /// <summary> 关键帧左键定位/右键菜单数据暂存 </summary>
        private void KeyFrameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                Tick = long.Parse(((Line)sender).Tag.ToString());
                TimeGridRedraw(sender, e);
            }//鼠标左键按下
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                ClickMark = long.Parse(((Line)sender).Tag.ToString());
            }//鼠标右键按下            
            KeyChanged(sender, e);
        }

        /// <summary> 过渡段 右键菜单 </summary>
        private void Transition_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ClickMark = long.Parse(((Line)sender).Tag.ToString().Replace("transition", ""));
            OneReversed = IsReversed[ClickMark];
        }
        #endregion

        #region KeyFrame ContestMenu        
        /// <summary> 复制关键帧数据 </summary>
        private void FrameCopy_Click(object sender, RoutedEventArgs e)
        {
            MarkPose.pos = pose[ClickMark].pos;
            MarkPose.key = pose[ClickMark].key;
        }

        /// <summary> 粘贴关键帧数据 </summary>
        private void FramePaste_Click(object sender, RoutedEventArgs e)
        {
            pose[ClickMark].pos = MarkPose.pos;
            pose[ClickMark].key = MarkPose.key;
            KeyChanged(sender, e);
        }

        /// <summary> 关键帧删除 </summary>
        private void DeleteFrame_Click(object sender, RoutedEventArgs e)
        {
            if (ClickMark != 0)
            {
                pose[ClickMark].key = false;
                TimeGridRedraw(sender, e);
                KeyChanged(sender, e);
            }
        }
        #endregion

        #region Media Button
        /// <summary> 时间轴播放 </summary>
        private void PlayTimer(object sender, EventArgs e)
        {
            Armor_stand_Page.poseChange = true;
            Tick++;
            if (Tick > TotalTick - 1)
            {
                if (repeat_button.Pressed)
                {
                    timer.Stop();
                    play_pause_button.Pressed = false;
                }//循环开关
                Scroll.ScrollToHorizontalOffset(0);
                Tick = 0;
            }
            TimeGrid.Children.Remove(timePoint);
            TimeLine(Tick);
            TimeGrid.Children.Add(timePoint);

            Page_masc.ChangePose(sender, e as RoutedEventArgs);

            //判断时间线是否超出视野，是则视野向后滚动
            double p = Scroll.HorizontalOffset;
            if (Tick * KeyWidth >= p + Width)
                Scroll.ScrollToHorizontalOffset(p + Width / 2);
        }

        private void Scroll_Horizontal_Changed(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset + 10);
            else if (e.Delta < 0) Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset - 10);
        }

        private void AddTick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            add_tick.Pressed = false;
            TotalTick++;
            if (TotalTick >= 32767) TotalTick = 32767;
        }

        private void RemoveTick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            add_tick.Pressed = false;
            TotalTick--;
            if (TotalTick < Tick) Tick = 0;
            if (TotalTick < 1) TotalTick = 1;
        }

        private void Play_Mousedown(object sender, MouseButtonEventArgs e)
        {
            KeyChanged(sender, e);
            if (play_pause_button.Pressed) timer.Start();
            else timer.Stop();
        }

        /// <summary> 回到开头 </summary>
        private void BackHome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Armor_stand_Page.poseChange = true;

            play_pause_button.Pressed = false;
            Play_Mousedown(sender, e);

            home_button.Pressed = false;
            Scroll.ScrollToHorizontalOffset(0);
            Tick = 0;
            TimeGridRedraw(sender, e);
        }

        /// <summary> 重置时间轴 </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Armor_stand_Page.poseChange = true;

            for (int i = 0; i < 32767; i++)
            {
                IsReversed[i] = new bool[19];
                pose[i].pos = new double[19];
                pose[i].key = false;
            } //动作数据初始化

            Tick = 0;
            play_pause_button.Pressed = false;

            Play_Mousedown(sender, e as MouseButtonEventArgs);
            Control_Loaded(sender, e);
            TimeGridRedraw(sender, e);
            Page_masc.ChangePose(sender, e);
        }
        #endregion
    }
}