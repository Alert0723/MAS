using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circle
{
    public class Class
    {
        public string[] Styles;

        public string[] ControlName = { "Angle", "Raduis", "Increasing height" };
        public double[] ControlValue = { 5, 1, 0 },
                        ControlMaxValue = { 90, 32767, 32767 },
                        ControlMinValue = { 5, 0.1, -32767 };


        public void Generate(ref string result, ref double[] point, ref double Angle)
        {
            for (double pitch = 0; pitch <= 360; pitch += Angle)
            {
                ClassMath(ref point, ref ControlValue[1], ref pitch, ref ControlValue[2]);
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
