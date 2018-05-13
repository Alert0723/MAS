
using System;

namespace Plugins
{
    //[PlugIn("Ball")]
    public class Ball
    {
        /// <summary> 算法样式名 </summary>
        public string StyleSign
        {
            get { return "Ball"; }
        }


        double Radius;
        double Angle;

        public string Preview()
        {
            string result = String.Empty;
            double[] par_distance = new double[3];

            float Pi = 361, Pi2 = 181;
            //if (Extra_style == "半圈周期") { float mark; mark = Pi; Pi = Pi2; Pi2 = mark; }

            for (double yaw = 0; yaw < Pi; yaw += Angle)
            {
                for (double pitch = 0; pitch < Pi2; pitch += Angle)
                {
                    par_distance[0] = Math.Sin(pitch * Math.PI / 180) * Math.Cos(yaw * Math.PI / 180) * Radius;
                    par_distance[1] = Math.Sin(pitch * Math.PI / 180) * Math.Sin(yaw * Math.PI / 180) * Radius;
                    par_distance[2] = Math.Cos(pitch * Math.PI / 180) * Radius;
                    result += par_distance[0] + "/" + par_distance[1] + "/" + par_distance[2] + "|";
                }
            }
            return result;
        }

        public string Generate()
        {
            string result = String.Empty;

            double[] par_distance = new double[3];
            string par = "<id>";
            double yaw, pitch;
            float Pi = 361, Pi2 = 181;
            //if (Extra_style == "半圈周期") { float mark; mark = Pi; Pi = Pi2; Pi2 = mark; }

            for (yaw = 0; yaw < Pi; yaw += Angle)
            {
                for (pitch = 0; pitch < Pi2; pitch += Angle)
                {
                    Ball_math(ref par_distance, ref Radius, ref pitch, ref yaw);
                    Add_ones(ref result, ref par, ref par_distance);
                }
            }
            return result;
        }

        public static void Ball_math(ref double[] par_distance, ref double Radius, ref double pitch, ref double yaw)
        {
            par_distance[0] = (Math.Sin(pitch * Math.PI / 180) * Math.Cos(yaw * Math.PI / 180) * Radius);
            par_distance[1] = (Math.Sin(pitch * Math.PI / 180) * Math.Sin(yaw * Math.PI / 180) * Radius);
            par_distance[2] = (Math.Cos(pitch * Math.PI / 180) * Radius);
        }

        /// <summary> 写入NBT文件缓存 </summary>
        private static void Add_ones(ref string result, ref string particle, ref double[] par_distance)
        {
            string Ones = null;
            //if (SelectorSwitch) { Ones = "execute as " + Selector + " at @s run ~" + exe_shift[0] + " ~" + exe_shift[1] + " ~" + exe_shift[2] + " "; }
            if (true)
                Ones += "particle " + particle + " ~" + par_distance[0].ToString("0.####") + " ~" + par_distance[1].ToString("0.####") + " ~" + par_distance[2].ToString("0.####");
            //else 
            //	Ones += "particle " + particle + " ^" + (exe_shift[0] + par_distance[0]).ToString("0.####") + " ^" + (exe_shift[1] + par_distance[1]).ToString("0.####") + " ^" + (exe_shift[2] + par_distance[2]).ToString("0.####"); 
            //if (Colorful) { /*Ones += " " + R + " " + G + " " + B + " 1 0";*/ }
            //else { Ones += " 0 0 0 0 1"; }
            //commands[k] = Ones; k++;
            result += Ones + "\r\n";
        }

    }
}
