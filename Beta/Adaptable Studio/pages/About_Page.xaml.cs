using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Adaptable_Studio
{
    /// <summary> About_Page.xaml 的交互逻辑 </summary>
    public partial class About_Page : Page
    {
        public About_Page()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
            Back.Click += Back_Click;
            feedback.Click += Feedback_Click;
            Donate.Click += Donate_Click;
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        void Feedback_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.mcbbs.net/thread-580119-1-1.html");
        }

        void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://afdian.net/@MsYqgzt");
        }


    }
}
