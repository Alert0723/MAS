using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Adaptable_Studio
{
    /// <summary> KeepButton.xaml 的交互逻辑 </summary>
    public partial class KeepButton : UserControl
    {
        public bool pressed;

        #region 自定义属性
        public static readonly DependencyProperty DefaultPath = DependencyProperty.Register("Default_Path", typeof(Geometry), typeof(KeepButton));
        [Description("UserControl"), Category("User Control")]
        public Geometry Default_Path { get; set; }


        public static readonly DependencyProperty PressedPath = DependencyProperty.Register("Pressed_Path", typeof(Geometry), typeof(KeepButton));
        [Description("UserControl"), Category("User Control")]
        public Geometry Pressed_Path { get; set; }

        [Description("UserControl"), Category("User Control")]
        public bool Pressed
        {
            get { return pressed; }
            set
            {
                pressed = value;
                PathControl.Data = value ? Pressed_Path : Default_Path;
            }
        }
        #endregion

        public KeepButton()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            PathControl.Data = Pressed ? Pressed_Path : Default_Path;
        }

        private void Path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Pressed = !Pressed;
        }
    }
}