using SharpGL;
using System;

namespace Adaptable_Studio
{
    class OpenGL_Draw
    {
        //一格=0.09
        /// <summary> 六面体模型通用函数 </summary>
        public static void Draw_Block(OpenGL gl, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(1f, 0f);
            gl.Vertex(x1, y1, z1);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x1, y2, z1);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x2, y2, z1);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x2, y1, z1);

            gl.TexCoord(1f, 0f);
            gl.Vertex(x1, y1, z2);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x1, y2, z2);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x1, y2, z1);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x1, y1, z1);

            gl.TexCoord(1f, 0f);
            gl.Vertex(x2, y1, z2);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x2, y2, z2);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x1, y2, z2);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x1, y1, z2);

            gl.TexCoord(1f, 0f);
            gl.Vertex(x2, y1, z1);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x2, y2, z1);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x2, y2, z2);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x2, y1, z2);

            gl.TexCoord(1f, 0f);
            gl.Vertex(x1, y2, z1);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x1, y2, z2);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x2, y2, z2);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x2, y2, z1);

            gl.TexCoord(1f, 0f);
            gl.Vertex(x1, y1, z2);
            gl.TexCoord(1f, 1f);
            gl.Vertex(x1, y1, z1);
            gl.TexCoord(0f, 1f);
            gl.Vertex(x2, y1, z1);
            gl.TexCoord(0f, 0f);
            gl.Vertex(x2, y1, z2);
            gl.End();
        }

        /// <summary> 主坐标轴函数 </summary>
        public static void Draw_Axis(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(0f, 0f);
            gl.Vertex(100f, 0f);
            gl.End();
        }

        /// <summary> 网格绘制 </summary>
        public void Network(OpenGL gl)
        {
            int c = 50,//网格绘制数
                d = 50;//坐标网格线最远距离
            gl.Color(0.3f, 0.3f, 0.3f);
            for (int i = 0; i < (c * 2); i++)
            {
                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(i - c, 0f, d);
                gl.Vertex(i - c, 0f, -d);
                //网格定值距离递推
                gl.Vertex(-d, 0f, i - c);
                gl.Vertex(d, 0f, i - c);
                gl.End();
            }
        }

        /// <summary> 史蒂夫模型框架绘制 </summary>        
        public void Steve_framework(OpenGL gl)
        {
            gl.Color(0.75f, 0.75f, 0f);
            gl.Translate(0f, 0.006f, 0f);
            gl.Begin(OpenGL.GL_LINES);
            Draw_Block(gl, -1.125f, 0f, -2.25f, 1.125f, 6.75f, 0f);
            Draw_Block(gl, -1.125f, 0f, 0f, 1.125f, 6.75f, 2.25f);
            Draw_Block(gl, -1.125f, 6.75f, -2.25f, 1.125f, 13.5f, 2.25f);
            Draw_Block(gl, -1.125f, 6.75f, -4.5f, 1.125f, 13.5f, -2.25f);
            Draw_Block(gl, -1.125f, 6.75f, 2.25f, 1.125f, 13.5f, 4.5f);
            Draw_Block(gl, -2.25f, 13.5f, -2.25f, 2.25f, 18f, 2.25f);
            gl.End();
        }

        #region armor_stand
        /// <summary> 箭头绘制 </summary>
        public static void Draw_arrow(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2.068564f, 0.025f, 0.001824f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(2.068564f, 0.075f, 0.001824f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.602196f, 0.075f, 0.468837f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.602196f, 0.025f, 0.468837f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.997804f, 0.025f, -0.068837f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.997804f, 0.075f, -0.068837f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(2.068564f, 0.075f, 0.001824f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(2.068564f, 0.025f, 0.001824f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.531436f, 0.025f, 0.398176f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.531436f, 0.075f, 0.398176f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.997804f, 0.075f, -0.068837f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.997804f, 0.025f, -0.068837f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.602196f, 0.025f, 0.468837f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.602196f, 0.075f, 0.468837f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.531436f, 0.075f, 0.398176f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.531436f, 0.025f, 0.398176f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2.068564f, 0.075f, 0.001824f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.997804f, 0.075f, -0.068837f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.531436f, 0.075f, 0.398176f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.602196f, 0.075f, 0.468837f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.997804f, 0.025f, -0.068837f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(2.068564f, 0.025f, 0.001824f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.602196f, 0.025f, 0.468837f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.531436f, 0.025f, 0.398176f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.99809f, 0.025f, 0.068627f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.99809f, 0.075f, 0.068627f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.531226f, 0.075f, -0.39789f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.531226f, 0.025f, -0.39789f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2.068774f, 0.025f, -0.00211f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(2.068774f, 0.075f, -0.00211f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.99809f, 0.075f, 0.068627f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.99809f, 0.025f, 0.068627f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.60191f, 0.025f, -0.468627f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.60191f, 0.075f, -0.468627f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(2.068774f, 0.075f, -0.00211f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(2.068774f, 0.025f, -0.00211f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.531226f, 0.025f, -0.39789f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.531226f, 0.075f, -0.39789f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.60191f, 0.075f, -0.468627f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.60191f, 0.025f, -0.468627f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(1.99809f, 0.075f, 0.068627f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(2.068774f, 0.075f, -0.00211f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.60191f, 0.075f, -0.468627f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.531226f, 0.075f, -0.39789f);

            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2.068774f, 0.025f, -0.00211f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(1.99809f, 0.025f, 0.068627f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(1.531226f, 0.025f, -0.39789f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(1.60191f, 0.025f, -0.468627f);
            gl.End();
        }
        #endregion

        #region special particle
        public void Par_Draw(OpenGL gl, ref double[] Axis_points)
        {
            gl.Translate(Axis_points[0], Axis_points[1], Axis_points[2]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(1f, 1f); gl.Vertex(0.08f, 0.08f, 0f);
            gl.TexCoord(1f, 0f); gl.Vertex(0.08f, -0.08f, 0f);
            gl.TexCoord(0f, 0f); gl.Vertex(-0.08f, -0.08f, 0f);
            gl.TexCoord(0f, 1f); gl.Vertex(-0.08f, 0.08f, 0f);

            gl.TexCoord(1f, 1f); gl.Vertex(0.08f, 0f, 0.08f);
            gl.TexCoord(1f, 0f); gl.Vertex(0.08f, 0f, -0.08f);
            gl.TexCoord(0f, 0f); gl.Vertex(-0.08f, 0f, -0.08f);
            gl.TexCoord(0f, 1f); gl.Vertex(-0.08f, 0f, 0.08f);

            gl.TexCoord(1f, 1f); gl.Vertex(0f, 0.08f, 0.08f);
            gl.TexCoord(1f, 0f); gl.Vertex(0f, 0.08f, -0.08f);
            gl.TexCoord(0f, 0f); gl.Vertex(0f, -0.08f, -0.08f);
            gl.TexCoord(0f, 1f); gl.Vertex(0f, -0.08f, 0.08f);
            gl.End();
        }

        #region"效果预览"
        /// <summary> 直线绘制 </summary>
        public void Particle_line(OpenGL gl, ref int pre_count, /*ref float R, ref float G, ref float B,*/ ref double particle_d)
        {
            //int count = 0;
            #region"算法处理"
            //int particle_num = 0/*粒子个数*/;
            //double distance = Math.Sqrt(Math.Pow(Special_particle_Page.del[0], 2) + Math.Pow(Special_particle_Page.del[1], 2) + Math.Pow(Special_particle_Page.del[2], 2));
            //particle_d = particle_d == 0 ? 1 : particle_d;
            //particle_num = (int)(distance / particle_d);//粒子数量            
            //for (int i = 0; i < 3; i++)
            //{
            //    Special_particle_Page.par_distance[i] = Special_particle_Page.del[i] / particle_num;
            //    if (particle_num == 0) { Special_particle_Page.par_distance[i] = 0; }
            //}
            #endregion            
            //do
            //{
            //    for (int i = 0; i < 3; i++) { Special_particle_Page.Start[i] += Special_particle_Page.par_distance[i]; }
            //    Par_Draw(gl, ref Special_particle_Page.Start);
            //    count++; pre_count++;
            //} while (count <= particle_num);

        }

        /// <summary> 环绕绘制 </summary>
        public void Particle_roll(OpenGL gl, ref int pre_count,/*ref float R, ref float G, ref float B,*/ ref double r, ref double Angle, ref double par_height)
        {
            double pitch/*圆相关循环参数*/;
            if (Special_particle_Page.Extra_style == "单粒子环绕")
            {
                #region"单粒子环绕"
                for (pitch = 0; pitch <= 360; pitch += Angle)
                {
                    Special_particle_Page.Roll_math(ref r, ref pitch, ref par_height);
                    Par_Draw(gl, ref Special_particle_Page.par_distance);
                    pre_count++;
                }
                #endregion
            }
            else if (Special_particle_Page.Extra_style == "双粒子环绕")
            {
                #region"双粒子环绕"
                for (pitch = 0; pitch <= 360; pitch += Angle)
                {
                    if (pitch > 180 - Angle & pitch < 180 + Angle) { Special_particle_Page.par_distance[1] = 0; }
                    Special_particle_Page.Roll_math(ref r, ref pitch, ref par_height);
                    Par_Draw(gl, ref Special_particle_Page.par_distance);
                    pre_count++;
                }
                #endregion
            }
            else if (Special_particle_Page.Extra_style == "螺旋延伸")
            {
                #region"螺旋延伸"
                double dr = r / 73;
                for (pitch = 0; pitch <= 360; pitch += Angle)
                {
                    Special_particle_Page.Roll_math(ref r, ref pitch, ref par_height);
                    Par_Draw(gl, ref Special_particle_Page.par_distance);
                    for (int i = 90; i <= 360; i += 90)
                    {
                        Special_particle_Page.par_distance[0] = (float)(Math.Sin((pitch + i) * Math.PI / 180) * r);
                        Special_particle_Page.par_distance[2] = (float)(Math.Cos((pitch + i) * Math.PI / 180) * r);
                        Par_Draw(gl, ref Special_particle_Page.par_distance);
                    }
                    r -= dr;
                    pre_count++;
                }
                #endregion
            }
        }

        /// <summary> 球体绘制 </summary>
        public void Particle_ball(OpenGL gl, ref int pre_count, /*ref float R, ref float G, ref float B,*/ ref double r, ref double Angle)
        {
            double pitch, yaw/*圆相关循环参数*/;
            float Pi = 361, Pi2 = 181;
            if (Special_particle_Page.Extra_style == "半圈周期") { float mark = Pi; Pi = Pi2; Pi2 = mark; }
            for (yaw = 0; yaw < Pi; yaw += Angle)
            {
                //if (special_particle_Page.particle_get[i].visible)
                //{
                for (pitch = 0; pitch < Pi2; pitch += Angle)
                {
                    Special_particle_Page.Ball_math(ref r, ref pitch, ref yaw);//计算模块
                    Special_particle_Page.Axis_Test();
                    Par_Draw(gl, ref Special_particle_Page.par_distance);
                    pre_count++;
                }
                //}                
            }
        }
        #endregion
        #endregion

        #region super_banner
        public static void Base(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_POLYGON);
            gl.TexCoord(1f, 0f);
            gl.Vertex(128f, -485f, 55f);
            gl.TexCoord(1f, 1f);
            gl.Vertex(128f, 15f, 10f);
            gl.TexCoord(0f, 1f);
            gl.Vertex(-128f, 15f, 10f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-128f, -485f, 55f);
            gl.End();
        }

        public static void Part_Color(OpenGL gl, int Name_Color)
        {
            //颜色(0-黑色,1-红色,2-绿色,3-棕色,4-蓝色,5-紫色,6-青色,7-灰色,8-深灰,
            //9-粉色,10-浅绿,11-黄色,12-浅蓝,13-浅紫,14-橙色,15-白色)
            switch (Name_Color)
            {
                case 15: gl.Color(0.9f, 0.9f, 0.9f, 1.0f); break;
                case 14: gl.Color(0.86f, 0.61f, 0.16f, 1.0f); break;
                case 13: gl.Color(0.69f, 0.18f, 0.9f, 1.0f); break;
                case 12: gl.Color(0f, 0.45f, 0.85f, 1.0f); break;
                case 11: gl.Color(0.88f, 0.84f, 0.16f, 1.0f); break;
                case 10: gl.Color(0.2f, 0.75f, 0f, 1.0f); break;
                case 9: gl.Color(0.82f, 0.02f, 0.57f, 1.0f); break;
                case 8: gl.Color(0.41f, 0.41f, 0.41f, 1.0f); break;
                case 7: gl.Color(0.66f, 0.66f, 0.66f, 1.0f); break;
                case 6: gl.Color(0f, 0.71f, 0.78f, 1.0f); break;
                case 5: gl.Color(0.47f, 0f, 0.76f, 1.0f); break;
                case 4: gl.Color(0f, 0.3f, 0.9f, 1.0f); break;
                case 3: gl.Color(0.33f, 0.24f, 0f, 1.0f); break;
                case 2: gl.Color(0f, 0.43f, 0f, 1.0f); break;
                case 1: gl.Color(0.63f, 0f, 0f, 1.0f); break;
                case 0: gl.Color(0.1f, 0.1f, 0.1f, 1.0f); break;
            }
        }
        #endregion
    }
}