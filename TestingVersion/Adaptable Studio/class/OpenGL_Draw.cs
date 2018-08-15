using SharpGL;
using System;

namespace Adaptable_Studio
{
    class OpenGL_Draw
    {
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