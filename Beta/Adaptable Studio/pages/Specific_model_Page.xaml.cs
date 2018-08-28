using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Collections;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Lighting;
using SharpGL.SceneGraph.Effects;
using SharpGL.SceneGraph.Primitives;

namespace Adaptable_Studio
{
    /// <summary> Specific_model_Page.xaml 的交互逻辑 </summary>
    public partial class Specific_model_Page : Page
    {
        private ArcBallEffect arcBallEffect = new ArcBallEffect();

        public Specific_model_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SceneView_Draw();
        }

        private void SceneView_Draw()
        {
            OpenGL gl = SceneView_all.OpenGL;
            

            //gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);//深度清除缓存
            //gl.LoadIdentity();
            //gl.ClearColor(0.6F, 0.6F, 0.6F, 0F);

            //gl.Color(1f, 1f, 1f);
            //gl.Enable(OpenGL.GL_TEXTURE_2D);//启用2D纹理映射
            //gl.Translate(0f, -1.5f, -8.5f);

            //添加场景基元

            //SceneView_all.Scene.SceneContainer.AddChild(new SharpGL.SceneGraph.Primitives.Grid());
            //SceneView_all.Scene.SceneContainer.AddChild(new SharpGL.SceneGraph.Primitives.Axies());

            Cube cube = new Cube();
            cube.AddEffect(arcBallEffect);
            //Add it.
            //SceneView_all.Scene.SceneContainer.AddChild(cube);

            //SceneView_all.Scene.Draw();
        }
    }
}