using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Adaptable_Studio
{
    /// <summary> MagneticMagnager.xaml 的交互逻辑 </summary>
    public partial class MagneticMagnager : MetroWindow
    {
        MetroWindow parent;
        int dir;

        public MagneticMagnager()
        {
            InitializeComponent();
        }

        /// <summary> 磁力窗体初始化 </summary>
        /// <param name="title">窗体标题</param>
        /// <param name="width">窗体宽度，默认800</param>
        /// <param name="height">窗体高度，默认450</param>
        /// <param name="direction">0=上吸附，1=右吸附，2=下吸附，3=左吸附</param>
        public MagneticMagnager(MetroWindow target, string title, double width = 800, double height = 450, int direction = 1)
        {
            InitializeComponent();

            parent = target;

            Title = title;
            Width = width;
            Height = height;

            dir = direction;
            WindowStartupLocation = parent.WindowStartupLocation;

            #region Events
            Loaded += Window_Loaded;
            parent.LocationChanged += LocationChanged;
            #endregion
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            Relocation();
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            Relocation();
        }

        void Relocation()
        {
            switch (dir)
            {
                case 0:
                    Left = parent.Left;
                    Top = parent.Top - ActualHeight;
                    break;

                case 1:
                    Left = parent.Left + parent.ActualWidth;
                    Top = parent.Top;
                    break;

                case 2:
                    Left = parent.Left;
                    Top = parent.Top + parent.ActualHeight;
                    break;

                case 3:
                    Left = parent.Left - ActualWidth;
                    Top = parent.Top;
                    break;
            }
        }

    }
}
