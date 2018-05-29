using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
            StatDataGet(sender, e);
        }

        /// <summary> 抓取网页数据 </summary>
        private void StatDataGet(object sender, RoutedEventArgs e)
        {
            try
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd");

                using (StreamReader reader = new StreamReader(WebRequest.Create("https://web.umeng.com/main.php?c=site&a=frame&siteid=1273799449#!/1527593073479/site/overview/1/1273799449/" + time + "/" + time).GetResponse().GetResponseStream(), Encoding.GetEncoding("gb2312")))
                {
                    string a = reader.ReadToEnd();
                }
            }
            catch { }
        }
    }
}
