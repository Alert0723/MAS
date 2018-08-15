using ArmorStand.CustomControl;
using System;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;

namespace Plugins
{
    public class Class
    {

        public string _StyleName = "Circle";

        public Class()
        {
        }

        public string StyleName()
        {
            return _StyleName;
        }

        public void AddControls(ref Canvas canvas)
        {
            canvas.Children.Add(new TextBlock()
            {
                Text = "Angle",
                Margin = new Thickness() { Top = 30, Left = 2 }
            });
            canvas.Children.Add(new CustomNumberBox()
            {
                Name = "Angle",
                Value = 5,
                Minimum = 2,
                Maximum = 90,
                Margin = new Thickness() { Top = 30, Left = 2 }
            });

            canvas.Children.Add(new TextBlock()
            {
                Text = "Radius",
                Margin = new Thickness() { Top = 30, Left = 2 }
            });
            canvas.Children.Add(new CustomNumberBox()
            {
                Name = "Radius",
                Value = 1,
                Minimum = 0.1,
                Maximum = 32767,
                Margin = new Thickness() { Top = 30, Left = 2 }
            });

            canvas.Children.Add(new TextBlock()
            {
                Text = "Increasing height",
                Margin = new Thickness() { Top = 30, Left = 2 }
            });
            canvas.Children.Add(new CustomNumberBox()
            {
                Name = "Increasing height",
                Value = 0,
                Minimum = -32767,
                Maximum = 32767,
                Margin = new Thickness() { Top = 30, Left = 2 }
            });
        }

        public void Generate(ref string result, ref double[] point, ref double Angle)
        {
            for (double pitch = 0; pitch <= 360; pitch += Angle)
            {
                //ClassMath(ref point, ref ControlValue[1], ref pitch, ref ControlValue[2]);
            }
        }

        public void Preview()
        {

        }

        /// <summary>
        /// 运算
        /// </summary>
        /// <param name="point">输出坐标点</param>
        /// <param name="radius">半径</param>
        /// <param name="pitch">角度</param>
        /// <param name="AddHeight">递增高度</param>
        public static void ClassMath(ref double[] point, ref double radius, ref double pitch, ref double AddHeight)
        {
            point[0] = (Math.Sin(pitch * Math.PI / 180) * radius);
            point[1] += AddHeight;
            point[2] = (Math.Cos(pitch * Math.PI / 180) * radius);
        }

        private static void Add_ones(ref double[] point, ref string particle, ref string result, ref string Selector)
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
