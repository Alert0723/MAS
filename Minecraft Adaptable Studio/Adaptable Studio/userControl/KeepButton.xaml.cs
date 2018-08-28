using System.ComponentModel;
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
        [Description("UserControl"), Category("User Control")]
        public ImageSource Defult_Image { get; set; }

        [Description("UserControl"), Category("User Control")]
        public ImageSource Pressed_Image { get; set; }

        [Description("UserControl"), Category("User Control")]
        public bool Pressed
        {
            get { return pressed; }
            set
            {
                pressed = value;
                image.Source = value ? Pressed_Image : Defult_Image;
            }
        }
        #endregion

        public KeepButton()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            image.Source = Pressed ? Pressed_Image : Defult_Image;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Pressed = !Pressed;
        }
    }
}