using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static Adaptable_Studio.PublicControl;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;

namespace Adaptable_Studio
{
    /// <summary> super_banner_Page.xaml 的交互逻辑 </summary>
    public partial class Super_banner_Page : Page
    {
        public Super_banner_Page()
        {
           
            InitializeComponent();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log_Write(LogPath, "==========MASB==========");
            Log_Write(LogPath, "环境初始化");
        }

    }
}
