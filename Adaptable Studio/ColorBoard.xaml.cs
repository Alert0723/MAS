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

namespace Adaptable_Studio
{
    /// <summary>
    /// ColorBoard.xaml 的交互逻辑
    /// </summary>
    public partial class ColorBoard : UserControl
    {
        public ColorBoard()
        {
            InitializeComponent();
        }

        private int _Color = 15;
        public int Color { get { return _Color; } set { _Color = value; } }

        #region 颜色按钮

        private void C0_Click(object sender, RoutedEventArgs e)
        {
            Color = 0;
        }

        private void C1_Click(object sender, RoutedEventArgs e)
        {
            Color = 1;
        }

        private void C2_Click(object sender, RoutedEventArgs e)
        {
            Color = 2;
        }

        private void C3_Click(object sender, RoutedEventArgs e)
        {
            Color = 3;
        }

        private void C4_Click(object sender, RoutedEventArgs e)
        {
            Color = 4;
        }

        private void C5_Click(object sender, RoutedEventArgs e)
        {
            Color = 5;
        }

        private void C6_Click(object sender, RoutedEventArgs e)
        {
            Color = 6;
        }

        private void C7_Click(object sender, RoutedEventArgs e)
        {
            Color = 7;
        }

        private void C8_Click(object sender, RoutedEventArgs e)
        {
            Color = 8;
        }

        private void C9_Click(object sender, RoutedEventArgs e)
        {
            Color = 9;
        }

        private void C10_Click(object sender, RoutedEventArgs e)
        {
            Color = 10;
        }

        private void C11_Click(object sender, RoutedEventArgs e)
        {
            Color = 11;
        }

        private void C12_Click(object sender, RoutedEventArgs e)
        {
            Color = 12;
        }

        private void C13_Click(object sender, RoutedEventArgs e)
        {
            Color = 13;
        }

        private void C14_Click(object sender, RoutedEventArgs e)
        {
            Color = 14;
        }

        private void C15_Click(object sender, RoutedEventArgs e)
        {
            Color = 15;
        }
        #endregion
    }
}
