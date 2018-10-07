using ArmorStand.CustomControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Plugins
{
    /// <summary> 主程序接口 </summary>
    interface IProcessControl
    {
        /// <summary> 添加控件 </summary>
        void AddControls(ref WrapPanel canvas, ref IDictionary<int, object> dic);

        /// <summary> 生成指令 </summary>
        /// <param name="point">输出坐标变量</param>
        /// <param name="Controls">0-Angle, 1-Radius, 2-IncreasingHeight</param>
        void Generate(ref string result, IDictionary<int, object> Control);

        /// <summary> 3D预览部分 </summary>
        void Preview(ref Viewport3D viewport, IDictionary<int, object> Control);
    }

    public class Class : IProcessControl
    {
        public string StyleName()
        {
            return "Circle";
        }

        public void AddControls(ref WrapPanel canvas, ref IDictionary<int, object> dic)
        {
            dic.Clear();
            canvas.Children.Add(new TextBlock()
            {
                Text = "Angle",
                Margin = new Thickness() { Top = 15, Left = 10 }
            });
            CustomNumberBox cnb = new CustomNumberBox()
            {
                Value = 5,
                Minimum = 2,
                Maximum = 90,
                Height = 20,
                Margin = new Thickness() { Top = 10, Left = 10 }
            };
            dic.Add(0, cnb);
            canvas.Children.Add(cnb);

            canvas.Children.Add(new TextBlock()
            {
                Text = "Radius",
                Margin = new Thickness() { Top = 15, Left = 10 }
            });
            cnb = new CustomNumberBox()
            {
                Value = 1,
                Minimum = 0.1,
                Maximum = 32767,
                Height = 25,
                Margin = new Thickness() { Top = 10, Left = 10 }
            };
            dic.Add(1, cnb);
            canvas.Children.Add(cnb);

            canvas.Children.Add(new TextBlock()
            {
                Text = "Increasing Height",
                Margin = new Thickness() { Top = 15, Left = 10 }
            });
            cnb = new CustomNumberBox()
            {
                Value = 0,
                Minimum = -32767,
                Maximum = 32767,
                Height = 25,
                Margin = new Thickness() { Top = 10, Left = 10 }
            };
            dic.Add(2, cnb);
            canvas.Children.Add(cnb);
        }

        /// <summary> 生成指令 </summary>
        /// <param name="point">输出坐标变量</param>
        /// <param name="Controls">0-Angle, 1-Radius, 2-IncreasingHeight</param>
        public void Generate(ref string result, IDictionary<int, object> Control)
        {
            double[] point = new double[3];
            for (double pitch = 0; pitch <= 360; pitch += ((CustomNumberBox)Control[0]).Value)
            {
                point[0] = (Math.Sin(pitch * Math.PI / 180) * ((CustomNumberBox)Control[1]).Value);
                point[1] += ((CustomNumberBox)Control[2]).Value;
                point[2] = (Math.Cos(pitch * Math.PI / 180) * ((CustomNumberBox)Control[1]).Value);
            }
        }

        public void Preview(ref Viewport3D viewport, IDictionary<int, object> Control)
        {
            double[] point = new double[3];
            for (double pitch = 0; pitch <= 360; pitch += ((CustomNumberBox)Control[0]).Value)
            {
                point[0] = (Math.Sin(pitch * Math.PI / 180) * ((CustomNumberBox)Control[1]).Value);
                point[1] += ((CustomNumberBox)Control[2]).Value;
                point[2] = (Math.Cos(pitch * Math.PI / 180) * ((CustomNumberBox)Control[1]).Value);

                ModelVisual3D ModelVisual3D = new ModelVisual3D();
                Model3DGroup Model3DGroup = new Model3DGroup();

                Point3DCollection pointcollect = new Point3DCollection();//模型坐标集合
                PointCollection texturepoints = new PointCollection();//纹理坐标集合
                AddPoints(ref pointcollect, ref texturepoints, point);

                DiffuseMaterial Material = new DiffuseMaterial()//纹理变量
                {
                    Brush = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri("pack://application:,,,/textures/masp/particle.png", UriKind.Relative))
                    }
                };

                MeshGeometry3D meshGeometry = new MeshGeometry3D()
                {
                    Positions = pointcollect,
                    TextureCoordinates = texturepoints
                };

                GeometryModel3D GeometryModel = new GeometryModel3D() { Geometry = meshGeometry };//模型实例

                Model3DGroup.Children.Add(GeometryModel);
                ModelVisual3D.Content = Model3DGroup;

                viewport.Children.Add(ModelVisual3D);
            }
        }

        public void AddPoints(ref Point3DCollection points, ref PointCollection texturepoints, double[] point)
        {
            points.Add(new Point3D(0.2, 0.2, 0)); points.Add(new Point3D(0.2, -0.2, 0)); points.Add(new Point3D(-0.2, 0.2, 0)); points.Add(new Point3D(-0.2, -0.2, 0)); points.Add(new Point3D(-0.2, 0.2, 0)); points.Add(new Point3D(0.2, -0.2, 0));
            points.Add(new Point3D(0, 0.2, 0.2)); points.Add(new Point3D(0, -0.2, 0.2)); points.Add(new Point3D(0, 0.2, -0.2)); points.Add(new Point3D(0, -0.2, -0.2)); points.Add(new Point3D(0, 0.2, -0.2)); points.Add(new Point3D(0, -0.2, 0.2));
            points.Add(new Point3D(0.2, 0, 0.2)); points.Add(new Point3D(-0.2, 0, 0.2)); points.Add(new Point3D(0.2, 0, -0.2)); points.Add(new Point3D(-0.2, 0, -0.2)); points.Add(new Point3D(0.2, 0, -0.2)); points.Add(new Point3D(-0.2, 0, 0.2));
            points.Add(new Point3D(0.2, -0.2, 0)); points.Add(new Point3D(-0.2, 0.2, 0)); points.Add(new Point3D(-0.2, -0.2, 0)); points.Add(new Point3D(-0.2, 0.2, 0)); points.Add(new Point3D(0.2, -0.2, 0)); points.Add(new Point3D(0.2, 0.2, 0));
            points.Add(new Point3D(0, -0.2, 0.2)); points.Add(new Point3D(0, 0.2, -0.2)); points.Add(new Point3D(0, -0.2, -0.2)); points.Add(new Point3D(0, 0.2, -0.2)); points.Add(new Point3D(0, -0.2, 0.2)); points.Add(new Point3D(0, 0.2, 0.2));
            points.Add(new Point3D(-0.2, 0, 0.2)); points.Add(new Point3D(0.2, 0, -0.2)); points.Add(new Point3D(-0.2, 0, -0.2)); points.Add(new Point3D(0.2, 0, -0.2)); points.Add(new Point3D(-0.2, 0, 0.2)); points.Add(new Point3D(0.2, 0, 0.2));

            texturepoints.Add(new Point(0, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1));
            texturepoints.Add(new Point(0, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1));
            texturepoints.Add(new Point(0, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1));
            texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(0, 0));
            texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(0, 0));
            texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(1, 1)); texturepoints.Add(new Point(1, 0)); texturepoints.Add(new Point(0, 1)); texturepoints.Add(new Point(0, 0));
        }

        static void Add_ones(ref double[] point, ref string particle, ref string result, ref string Selector)
        {
            string Ones = null;
            //if (SelectorSwitch) { Ones = "execute as " + Selector + " at @s run ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
            /*if (SelectorSwitch)*/
            { Ones += "particle " + particle + " ~" + point[0].ToString("0.####") + " ~" + point[1].ToString("0.####") + " ~" + point[2].ToString("0.####"); }
            //else { Ones += "particle " + particle + " ^" + (exe_shift[0] + point[0]).ToString("0.####") + " ^" + (exe_shift[1] + point[1]).ToString("0.####") + " ^" + (exe_shift[2] + point[2]).ToString("0.####"); }
            //if (Colorful) { Ones += " " + R + " " + G + " " + B + " 1 0"; }
            //else { Ones += " 0 0 0 0 1"; }
            //commands[k] = Ones; k++;
            result += Ones + "\r\n";
        }
    }
}
