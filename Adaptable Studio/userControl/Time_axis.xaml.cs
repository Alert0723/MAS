using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Adaptable_Studio
{
    /// <summary> Time_axis.xaml 的交互逻辑 </summary>
    public partial class Time_axis : UserControl
    {
        Line timePoint = new Line(),//时间线
             timeScale = new Line(),//刻度
             keyFrame = new Line();//关键帧

        bool mousedown;
        //0~2:head
        //3~5:body
        //6~8:left arm
        //9~11:right arm
        //12~14:left leg
        //15~17:right leg
        //18:rotation
        public struct Pose { public float[] pos; public bool key; }//时间轴结构体

        public Pose[] pose = new Pose[32767];//时间轴数据存储
        DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 50) };//播放

        #region 自定义属性
        /// <summary> (只读)全局数据结构体 </summary>
        [Description("(只读)全局数据结构体"), Category("User Control")]
        public Pose[] PoseStructure
        {
            get { return pose; }
        }

        /// <summary> 独立数据结构体 </summary>
        [Description("独立数据结构体"), Category("User Control")]
        public Pose FramePose
        {
            get { return pose[Tick]; }
            set { pose[Tick] = value; }
        }

        /// <summary> 当前Tick值 </summary>
        [Description("当前Tick值"), Category("User Control")]
        public long Tick { get; set; }

        /// <summary> 总Tick值 </summary>
        [Description("总Tick值"), Category("User Control")]
        public long TotalTick
        {
            get { return (long)(Canvas.Width / KeyWidth); }
            set
            {
                Canvas.Width = value * KeyWidth;
                TimeGrid.Width = value * KeyWidth;
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
            for (int i = 0; i < 32767; i++) pose[i].pos = new float[19]; //动作数据初始化
        }

        private void First_KeyFrame(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Tick = 0;
            TimeGrid.Children.Remove(timePoint);
            TimeLine(0);
            TimeGrid.Children.Add(timePoint);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (TotalTick < Canvas.Width) TotalTick = (long)(Canvas.Width / KeyWidth);
            //时间线
            timePoint = new Line()
            {
                Stroke = Brushes.White,
                StrokeThickness = 1,//线条宽度
                X1 = (Tick + 1) * KeyWidth - KeyWidth / 2,
                X2 = (Tick + 1) * KeyWidth - KeyWidth / 2,
                Y1 = 0,
                Y2 = TimeGrid.ActualHeight
            };
            TimeGrid.Children.Add(timePoint);

            //原点关键帧
            double d = lineWidth * Math.Sin(Math.PI / 4) / 2;//菱形水平始末间距                
            keyFrame = new Line()
            {
                Tag = 0,
                Stroke = Brushes.LightCyan,
                StrokeThickness = lineWidth,//线条宽度
                X1 = KeyWidth / 2 - d,
                X2 = KeyWidth / 2 + d,
                Y1 = top - d,
                Y2 = top + d
            };
            keyFrame.MouseDown += First_KeyFrame;
            TimeGrid.Children.Add(keyFrame);
        }

        private void Control_Focus(object sender, RoutedEventArgs e)
        {
            KeyChanged();
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

        /// <summary> 关键帧数据运算 </summary>
        private void KeyChanged()
        {
            int start, end = 0;//补间始末数据
            int mark = 0;//运算标记
            int i = 0;//时间轴起点

            while (i < TotalTick)
            {
                start = mark;
                for (i = mark + 1; i < TotalTick; i++)
                {
                    if (pose[i].key) { end = i; mark = i; break; }
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
                            //每元素平均增量
                            if (p != start) pose[p].pos[q] += (p - start + 1) * (pose[end].pos[q] - pose[start].pos[q]) / TickDelay;
                        }
                    }
                }
            }
            //多余数据重置
            for (i = end + 1; i < TotalTick; i++)
            {
                for (int j = 0; j < 19; j++) pose[i].pos[j] = 0;
            }
        }

        #region UI绘制模板
        /// <summary> 新建线条 </summary>
        private void NewLine(ref Line line, int lineWidth, long pointX, double pointY)
        {
            line = new Line()
            {
                Stroke = Brushes.LightCyan,
                StrokeThickness = lineWidth,//线宽
                X1 = pointX,
                X2 = pointX,
                Y1 = 0,
                Y2 = pointY
            };
            line.MouseUp += TimeGridMouseUp;
            line.MouseMove += TimeGridMouseMove;
        }

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
                Y2 = Y + d
            };
            keyFrame.MouseDown += KeyFrameMouseDown;
            keyFrame.MouseLeave += KeyFrameMouseLeave;
        }
        #endregion

        const float lineWidth = 6,//线条宽度
                    top = 25,//到顶部距离
                    KeyWidth = 20;//帧宽度
        #region Grid元素绘制/事件
        private void TimeGridResized(object sender, SizeChangedEventArgs e)
        {
            //倒序索引删除刻度+标识
            int index = Canvas.Children.Count;
            for (int i = index - 1; i >= 0; i--)
            {
                if (Canvas.Children[i] is Line) Canvas.Children.Remove(Canvas.Children[i]);
                else if (Canvas.Children[i] is TextBlock) Canvas.Children.Remove(Canvas.Children[i]);
            }

            for (int i = 1; i <= TotalTick; i++)
            {
                //添加刻度-tick
                timeScale = new Line()
                {
                    Stroke = Brushes.LightSteelBlue,
                    StrokeThickness = 0.5,//线条宽度
                    X1 = i * KeyWidth - KeyWidth / 2,
                    X2 = i * KeyWidth - KeyWidth / 2,
                    Y1 = 0,
                    Y2 = 8,
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(timeScale);

                //添加隔板
                timeScale = new Line()
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 17, 158, 218)),
                    StrokeThickness = 0.8,//线条宽度
                    X1 = i * KeyWidth,
                    X2 = i * KeyWidth,
                    Y1 = 0,
                    Y2 = TimeGrid.ActualHeight,
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(timeScale);

                //添加刻度数字标识
                int length = (i - 1).ToString().Length;//刻度数字位数
                TextBlock tb = new TextBlock()
                {
                    IsEnabled = false,
                    Margin = new Thickness(i * KeyWidth - KeyWidth / 2 - 2.2 * length, 7, Margin.Right, Margin.Bottom),
                    Text = (i - 1).ToString(),
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
                    IsHitTestVisible = false
                };
                Canvas.Children.Add(tb);
            }
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
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mousedown = true;
                Tick = (long)(Position / KeyWidth);
                TimeGrid.Children.Remove(timePoint);
                TimeLine(Tick);
                TimeGrid.Children.Add(timePoint);
            }//鼠标左键按下
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (pose[(int)(Position / KeyWidth - 0.5)].key == false)//无关键帧条件
                {
                    double a = lineWidth * Math.Sin(Math.PI / 4) / 2;//菱形水平始末间距
                    NewKeyFrame(ref keyFrame, Position, top, a, ((int)(Position / KeyWidth - 0.5)).ToString());//新建关键帧
                    pose[(int)(Position / KeyWidth - 0.5)].key = true;//关键帧位置
                    TimeGrid.Children.Add(keyFrame);

                    int t = 0;
                    foreach (var item in TimeGrid.Children)
                        if (((Line)item).Tag != null) t++;//检测关键帧数量
                }
            }//鼠标右键按下
            KeyChanged();//关键帧补间事件
        }

        /// <summary> 关键帧点击事件 </summary>
        private void KeyFrameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                Tick = long.Parse(((Line)sender).Tag.ToString());
                TimeGrid.Children.Remove(timePoint);
                TimeLine(Tick);
                TimeGrid.Children.Add(timePoint);
            }//鼠标左键按下
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                pose[long.Parse(((Line)sender).Tag.ToString())].key = false;
                TimeGrid.Children.Remove((Line)sender);
            }//鼠标右键按下
            KeyChanged();//关键帧补间事件
        }

        private void TimeGridMouseEnter(object sender, MouseEventArgs e)
        {
            KeyChanged();
        }

        private void KeyFrameMouseLeave(object sender, MouseEventArgs e)
        {
            KeyChanged();
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
                TimeGrid.Children.Remove(timePoint);
                TimeLine(Tick);
                TimeGrid.Children.Add(timePoint);
            }
        }

        private void TimeGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;
        }

        private void TimeGridMouseLeave(object sender, MouseEventArgs e)
        {
            mousedown = false;
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

            //判断时间线是否超出视野，是则视野向后滚动
            double p = Scroll.HorizontalOffset;
            if (Tick * KeyWidth >= p + Width) Scroll.ScrollToHorizontalOffset(p + Width / 2);
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
            KeyChanged();//关键帧补间事件
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
            TimeGrid.Children.Remove(timePoint);
            TimeLine(Tick);
            this.TimeGrid.Children.Add(timePoint);
        }

        /// <summary> 重置时间轴 </summary>
        private void Reset_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Armor_stand_Page.poseChange = true;

            play_pause_button.Pressed = false;
            Play_Mousedown(sender, e);

            reset_button.Pressed = false;
            TimeGrid.Children.Clear();
            Tick = 0;
            Control_Loaded(sender, e);
            for (int i = 1; i < TotalTick; i++) pose[i].key = false;
            for (int i = 0; i < TotalTick; i++)
            {
                for (int j = 0; j < 19; j++) pose[i].pos[j] = 0;
            }
        }
        #endregion
    }
}