using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adaptable_Studio
{
    /// <summary> MASC_Part_Direction.xaml 的交互逻辑 </summary>
    public partial class MASC_Part_Direction : MetroWindow
    {
        long ClickMark;

        public MASC_Part_Direction()
        {
            InitializeComponent();
            ClickMark = Time_axis.ClickMark;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool[] OneResersed = Time_axis.OneResersed;//方向数据缓存  
            Check0.IsChecked = OneResersed[0]; Check1.IsChecked = OneResersed[1]; Check2.IsChecked = OneResersed[2];
            Check3.IsChecked = OneResersed[3]; Check4.IsChecked = OneResersed[4]; Check5.IsChecked = OneResersed[5];
            Check6.IsChecked = OneResersed[6]; Check7.IsChecked = OneResersed[7]; Check8.IsChecked = OneResersed[8];
            Check9.IsChecked = OneResersed[9]; Check10.IsChecked = OneResersed[10]; Check11.IsChecked = OneResersed[11];
            Check12.IsChecked = OneResersed[12]; Check13.IsChecked = OneResersed[13]; Check14.IsChecked = OneResersed[14];
            Check15.IsChecked = OneResersed[15]; Check16.IsChecked = OneResersed[16]; Check17.IsChecked = OneResersed[17];
            Check18.IsChecked = OneResersed[18];

        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            Time_axis.IsReversed[ClickMark][int.Parse(((CheckBox)sender).Tag.ToString())] = (bool)((CheckBox)sender).IsChecked;
        }

        /// <summary> 窗体失去活动时发生 </summary>        
        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}