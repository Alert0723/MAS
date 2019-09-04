using System;
using System.Windows.Media.Media3D;

namespace Adaptable_Studio
{
    /// <summary> Viewport3D 系统框架函数 </summary>
    class Viewport_3D
    {
        /// <summary> 摄像机位置设定 </summary>
        /// <param name="camera">摄像机</param>
        /// <param name="rotation">摄像机角度朝向(水平,竖直)</param>
        /// <param name="LookAtPoint">摄像机观察目标点</param>
        /// <param name="radius">摄像机环绕半径</param>
        public static void CameraReset(ref PerspectiveCamera camera, double[] rotation, double[] LookAtPoint, double radius)
        {
            Point3D Point = new Point3D()
            {
                X = LookAtPoint[0] + Math.Sin(rotation[1] * Math.PI / 180) * Math.Cos(rotation[0] * Math.PI / 180) * radius,
                Y = LookAtPoint[1] + Math.Cos(rotation[1] * Math.PI / 180) * radius,
                Z = LookAtPoint[2] + Math.Sin(rotation[1] * Math.PI / 180) * Math.Sin(rotation[0] * Math.PI / 180) * radius
            };
            camera.Position = Point;
            camera.LookDirection = new Vector3D() { X = -Point.X + LookAtPoint[0], Y = -Point.Y + LookAtPoint[1], Z = -Point.Z + LookAtPoint[2] };
        }

        /// <summary> 平面法向量计算 </summary>
        /// <param name="vector1">构成平面的向量1</param>
        /// <param name="vector2">构成平面的向量2</param>
        public static Vector3D UpDirection_Get(Vector3D vector1, Vector3D vector2)
        {
            //叉乘 a×b=(y1z2-y2z1,z1x2-z2x1,x1y2-x2y1)            
            return new Vector3D()
            {
                X = vector1.Y * vector2.Z - vector2.Y * vector1.Z,
                Y = vector1.Z * vector2.X - vector1.X * vector2.Z,
                Z = vector1.X * vector2.Y - vector1.Y * vector2.X
            };
        }

    }
}
