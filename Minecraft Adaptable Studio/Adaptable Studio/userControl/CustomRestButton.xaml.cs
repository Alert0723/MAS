using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace Adaptable_Studio.userControl
{
    /// <summary> CustomRestButton.xaml 的交互逻辑 </summary>
    public partial class CustomRestButton : UserControl
    {
        public CustomRestButton()
        {
            InitializeComponent();
        }

        public bool _IsConfirmRest = false;
        /// <summary> 点击确认是否通过 </summary>
        [Description("UserControl"), Category("User Control")]
        public bool IsConfirmRest
        {
            get { return _IsConfirmRest; }
            set { confirmstate = _IsConfirmRest = value; }
        }

        public string _restcontent;
        /// <summary> 确认提示显示的文本内容 </summary>
        [Description("UserControl"), Category("User Control")]
        public string RestContent
        {
            get { return _restcontent; }
            set { label.Content = _restcontent = value; }
        }

        bool checkstate = false;
        bool confirmstate = false;

        private void CheckRestEvent(object sender, MouseButtonEventArgs e)
        {
            if (checkstate)
            {
                IsConfirmRest = confirmstate = true;
                VisualStateManager.GoToState(this, "ConfirmRest", true);
            }
            else VisualStateManager.GoToState(this, "CheckRest", true);

            checkstate = !checkstate;
        }

        private void CancelRestEvent(object sender, MouseEventArgs e)
        {
            checkstate = false;
            VisualStateManager.GoToState(this, "Base", true);
        }
    }
}