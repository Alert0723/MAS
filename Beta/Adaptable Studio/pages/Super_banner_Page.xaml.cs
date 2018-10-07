using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Assets;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Adaptable_Studio
{
    /// <summary> super_banner_Page.xaml 的交互逻辑 </summary>
    public partial class Super_banner_Page : Page
    {
        string AppPath = Environment.CurrentDirectory,//应用程序根目录
               LogPath;//日志文件路径

        //底层+6层样式纹理
        Texture Base = new Texture();
        Texture F1 = new Texture();
        Texture F2 = new Texture();
        Texture F3 = new Texture();
        Texture F4 = new Texture();
        Texture F5 = new Texture();
        Texture F6 = new Texture();
        //Texture Base = new Texture();
        //颜色(0-黑色,1-红色,2-绿色,3-棕色,4-蓝色,5-紫色,6-青色,7-灰色,8-深灰,
        //     9-粉色,10-浅绿,11-黄色,12-浅蓝,13-浅紫,14-橙色,15-白色)
        public int[] Color = new int[7];//1+6 层颜色参数
        int floor_total,//总层数统计
            floor_selected//当前选中层
            ;

        #region ini配置文件
        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //定义写入函数
        //用途：若存在给定路径下的ini文件，就在其中写入给定节和键的值（若已存在此键就覆盖之前的值），若不存在ini文件，就创建该ini文件并写入。

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        //定义读入函数

        string iniPath = Environment.CurrentDirectory + @"\config.ini";//ini文件路径
        StringBuilder StrName = new StringBuilder(255);//定义字符串  
        #endregion

        public Super_banner_Page()
        {
            InitializeComponent();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogPath = AppPath + @"\log.txt";

            for (int i = 0; i < 7; i++) { Color[i] = 15; }
            #region OpenGL初始化
            OpenGL gl = OpenGLControl.OpenGL;
            gl.ClearDepth(1.0f);//设置深度缓存
            gl.Enable(OpenGL.GL_DEPTH_TEST);//启用深度测试
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);//透视修正
            gl.ShadeModel(OpenGL.GL_SMOOTH);//启用阴影平滑
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);//基于源像素alpha通道值的半透明混合函数
            try
            {
                Base.Create(gl, AppPath + @"\textures\masb\base.png");//底板纹理
                F1.Create(gl, AppPath + @"\textures\masb\flower.png");
                F2.Create(gl, AppPath + @"\textures\masb\stripe_downleft.png");
                F3.Create(gl, AppPath + @"\textures\masb\mojang.png");
                F4.Create(gl, AppPath + @"\textures\masb\mojang.png");
                F5.Create(gl, AppPath + @"\textures\masb\mojang.png");
                F6.Create(gl, AppPath + @"\textures\masb\mojang.png");

                MainWindow.Log_Write(LogPath, "[OpenGL]纹理绑定完成");
            }
            catch { MainWindow.Log_Write(LogPath, "[Error]纹理绑定失败"); }//纹理绑定
            MainWindow.Log_Write(LogPath, "[masb]OpenGL初始化完成");
            #endregion
        }

        #region OpenGL绘制
        /// <summary> 绘制主体 </summary>
        void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = OpenGLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);//深度清除缓存       
            gl.LoadIdentity();//重置当前模型观察矩阵
            gl.ClearColor(0.65f, 0.65f, 0.65f, 0f);

            gl.Color(0.9f, 0.9f, 0.9f, 1.0f);
            OpenGL_reset(gl);
            gl.Translate(0f, 250.0f, 0f);

            //编辑层数判定
            for (int i = 0; i < 6; i++) if (floor_selected == i) Color[i] = Color_Set.Color;

            gl.Enable(OpenGL.GL_TEXTURE_2D);//启用2D纹理映射

            Base.Bind(gl);
            OpenGL_Draw.Part_Color(gl, Color[0]); //底层颜色
            OpenGL_Draw.Base(gl);

            gl.Enable(OpenGL.GL_BLEND);//启用混色
            //gl.Color(0f, 0f, 0f, 0.5f)           
            if (floor_total > 0)
            {
                BannerFloor(gl, F1, 1);//第1层颜色
                if (floor_total > 1)
                {
                    BannerFloor(gl, F2, 2);//第1层颜色
                    if (floor_total > 2)
                    {
                        BannerFloor(gl, F3, 3);//第1层颜色
                        if (floor_total > 3)
                        {
                            BannerFloor(gl, F3, 3);//第1层颜色
                            if (floor_total > 4)
                            {
                                BannerFloor(gl, F3, 3);//第1层颜色
                                if (floor_total > 5)
                                {
                                    BannerFloor(gl, F4, 4);//第1层颜色
                                }
                            }
                        }
                    }
                }
            }

            gl.Disable(OpenGL.GL_TEXTURE_2D);//禁用2D纹理映射
            gl.Disable(OpenGL.GL_BLEND);//禁用混色
            gl.Flush();
        }

        void BannerFloor(OpenGL gl, Texture tex, int color)
        {
            tex.Bind(gl);
            OpenGL_Draw.Part_Color(gl, Color[color]); //第1层颜色
            gl.Translate(0f, 0f, 0.5f);
            OpenGL_Draw.Base(gl);
        }

        /// <summary> 坐标重置</summary>
        public void OpenGL_reset(OpenGL gl)
        {
            gl.LoadIdentity();
            gl.Translate(0f, 0f, -8.0f);
            gl.Rotate(20f, 20f, 0f);
            gl.Scale(0.01f, 0.01f, 0.01f); //缩放
        }
        #endregion

        void Button_Click_1(object sender, RoutedEventArgs e)
        {
            floor_total++;
            if (floor_total >= 6) floor_total = 0;
            l1.Content = "总层数" + floor_total;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            floor_selected++;
            if (floor_selected >= 6) floor_selected = 0;
            Color_Set.Color = Color[floor_selected];
            l2.Content = "当前层数" + floor_selected;
        }
    }
}
