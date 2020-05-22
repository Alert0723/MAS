using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Adaptable_Studio
{
    /// <summary> Curves.xaml 的交互逻 </summary>
    public partial class Curves : UserControl
    {
        #region 插值点参数
        Point p1 = new Point(), p2 = new Point();
        private Point controlPoint0 { get; set; }
        public Point controlPoint1
        {
            get { return p1; }
            set { p1 = value; }
        }
        public Point controlPoint2
        {
            get { return p2; }
            set { p2 = value; }
        }
        private Point controlPoint3 { get; set; }
        #endregion

        #region 可视化对象
        //曲线
        Path bezier = new Path()
        {
            Tag = "curve",
            Stroke = Brushes.White,
            StrokeThickness = 2
        };

        //控制点
        Path cp1 = new Path()
        {
            Tag = "",
            Uid = "cp1",
            Fill = Brushes.Gray
        };
        Path cp2 = new Path()
        {
            Tag = "",
            Uid = "cp2",
            Fill = Brushes.Gray
        };

        //辅助线
        Line l1 = new Line()
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 1
        };
        Line l2 = new Line()
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 1
        };
        #endregion

        public Curves()
        {
            InitializeComponent();

            Loaded += Control_Loaded;
            canvas.MouseMove += canva_MouseMove;

            //控制点事件
            cp1.MouseDown += point_MouseDown;
            cp2.MouseDown += point_MouseDown;
            cp1.MouseUp += point_MouseUp;
            cp2.MouseUp += point_MouseUp;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();

            controlPoint0 = new Point(0, ActualHeight);
            controlPoint1 = new Point(ActualWidth / 3, ActualHeight * 2 / 3);
            controlPoint2 = new Point(ActualWidth * 2 / 3, ActualHeight / 3);
            controlPoint3 = new Point(ActualWidth, 0);

            //辅助线
            l1.X1 = controlPoint0.X;
            l1.Y1 = controlPoint0.Y;
            l1.X2 = controlPoint1.X;
            l1.Y2 = controlPoint1.Y;

            l2.X1 = controlPoint3.X;
            l2.Y1 = controlPoint3.Y;
            l2.X2 = controlPoint2.X;
            l2.Y2 = controlPoint2.Y;

            canvas.Children.Add(l1);
            canvas.Children.Add(l2);

            //绘制曲线
            bezier.Data = new PathGeometry()
            {
                Figures = new PathFigureCollection()
                {
                    new PathFigure()
                    {
                        StartPoint = controlPoint0,
                        Segments = new PathSegmentCollection()
                        {
                           new BezierSegment
                           {
                               Point1 = controlPoint1,
                               Point2 = controlPoint2,
                               Point3 = controlPoint3
                           }
                        }
                    }
                }
            };
            canvas.Children.Add(bezier);

            //绘制控制点
            cp1.Data = new EllipseGeometry()
            {
                Center = controlPoint1,
                RadiusX = 4,
                RadiusY = 4
            };
            cp2.Data = new EllipseGeometry()
            {
                Center = controlPoint2,
                RadiusX = 4,
                RadiusY = 4
            };

            canvas.Children.Add(cp1);
            canvas.Children.Add(cp2);
        }

        private void point_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Path)sender).Tag += "move";
            ((Path)sender).Fill = Brushes.Yellow;
        }
        private void point_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Path)sender).Tag = "";
            ((Path)sender).Fill = Brushes.Gray;
        }

        private void canva_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (var point in canvas.Children)
            {
                if (point is Path && ((Path)point).Uid != null && ((Path)point).Tag.ToString() == "move")
                {
                    ((EllipseGeometry)(((Path)point).Data)).Center = e.GetPosition(canvas);
                    if (((Path)point).Uid == "cp1")
                        controlPoint1 = e.GetPosition(canvas);
                    else if (((Path)point).Uid == "cp2")
                        controlPoint2 = e.GetPosition(canvas);

                    Redraw(e);
                    Recal();
                    break;
                }
            }
        }

        /// <summary> 刷新视觉交互 </summary>
        /// <param name="e">视觉反馈必须变量</param>
        void Redraw(EventArgs e)
        {
            bezier.Data = new PathGeometry()
            {
                Figures = new PathFigureCollection()
                {
                    new PathFigure()
                    {
                        StartPoint = new Point(0, this.ActualHeight),
                        Segments = new PathSegmentCollection()
                        {
                           new BezierSegment
                           {
                               Point1 = controlPoint1,
                               Point2 = controlPoint2,
                               Point3 = new Point(ActualWidth, 0)
                           }
                        }
                    }
                }
            };

            // 辅助线
            l1.X1 = 0;
            l1.Y1 = ActualHeight;
            l1.X2 = controlPoint1.X;
            l1.Y2 = controlPoint1.Y;

            l2.X1 = ActualWidth;
            l2.Y1 = 0;
            l2.X2 = controlPoint2.X;
            l2.Y2 = controlPoint2.Y;
        }

        /// <summary> 计算导入的数据，换算为贝塞尔曲线的插值 </summary>
        void Recal()
        {
            List<Point> points = new List<Point>
            {
                controlPoint0,
                controlPoint1,
                controlPoint2,
                controlPoint3
            };

            List<Point> resultPoints = Bezier_generate(points, 3);
            // Δ = (1 - resultPoints[i].Y/ActualHeight)%
        }

        /// <summary> Bezier曲线算法 </summary>
        /// <param name="points">特征多边形的点集合</param>
        /// <param name="step">曲线平滑程度，数值越大越平滑</param>
        /// <returns></returns>
        List<Point> Bezier_generate(List<Point> points, int step = 100)
        {
            if (points.Count == 0)
                return new List<Point>();

            List<Point> resultPoints = new List<Point>();
            for (double t = 0; t < 1; t += controlPoint3.X / step)
            {
                resultPoints.Add(Bezier_loop(points, new List<Point>(), t));
            }

            return resultPoints;
        }
        Point Bezier_loop(List<Point> points, List<Point> temp, double t)
        {
            for (int i = 1; i < points.Count; i++)
            {
                int deltaX = (int)(t * (points[i].X - points[i - 1].X) + 0.5),
                    deltaY = (int)(t * (points[i].Y - points[i - 1].Y) + 0.5);

                temp.Add(new Point(points[i - 1].X + deltaX, points[i - 1].Y + deltaY));
            }
            if (temp.Count > 1)
            {
                return Bezier_loop(temp, new List<Point>(), t);
            }
            else
            {
                return temp[0];
            }
        }

    }
}
