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
    /// <summary> Developer_Page.xaml 的交互逻辑 </summary>
    public partial class Developer_Page : Page
    {
        public Developer_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {                        
            WebView.Navigate(new Uri("http://minecraft-adaptable-studio-1254149191.cos.ap-chengdu.myqcloud.com/stat.html"));
        }
    }
}
