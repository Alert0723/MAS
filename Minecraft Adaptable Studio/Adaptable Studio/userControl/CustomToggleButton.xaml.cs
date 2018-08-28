using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Adaptable_Studio.userControl
{
    /// <summary> CustomToggleButton.xaml 的交互逻辑 </summary>
    public partial class CustomToggleButton : UserControl
    {
        public bool _IsChecked;
        [Description("UserControl"), Category("User Control")]
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { ToggleButton.IsChecked = _IsChecked = value; }
        }

        private void IsChecked_Event(object sender, RoutedEventArgs e)
        {
            IsChecked = (bool)ToggleButton.IsChecked;
        }

        public CustomToggleButton()
        {
            InitializeComponent();
        }
    }
}