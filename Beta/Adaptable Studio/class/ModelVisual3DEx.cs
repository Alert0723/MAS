using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace Adaptable_Studio
{
    public class ModelVisual3DEx : ModelVisual3D
    {
        public object Tag { get; set; }
    }

    public class RotateManipulatorEx : RotateManipulator
    {
        public object Tag { get; set; }
    }
}
