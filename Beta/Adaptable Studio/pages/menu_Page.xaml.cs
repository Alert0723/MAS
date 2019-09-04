using System.Windows;
using System.Windows.Controls;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;
using static Adaptable_Studio.PublicControl;

namespace Adaptable_Studio
{
    /// <summary> menu_Page.xaml 的交互逻辑 </summary>
    public partial class menu_Page : Page
    {
        public menu_Page()
        {
            InitializeComponent();
            #region Events
            Loaded += Page_Loaded;
            masb.Click += MASB;
            masc.Click += MASC;
            masp.Click += MASP;
            #endregion
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.LoadCompleted += NavigationService_LoadCompleted;
        }

        private void NavigationService_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (PageIndex == 0)
            {
                (Main as MainWindow).VersionIcon.Visibility = Visibility.Visible;
                (Main as MainWindow).VersionText.Visibility = Visibility.Visible;
            }
            else
            {
                (Main as MainWindow).VersionIcon.Visibility = Visibility.Hidden;
                (Main as MainWindow).VersionText.Visibility = Visibility.Hidden;
            }
        }

        void MASB(object sender, RoutedEventArgs e)
        {
            IniWrite("System", "PageIndex", "2", iniPath);
            PageIndex = 2;
            NavigationService.Navigate(Page_masb);
        }

        void MASC(object sender, RoutedEventArgs e)
        {
            IniWrite("System", "PageIndex", "3", iniPath);
            PageIndex = 3;
            NavigationService.Navigate(Page_masc);
        }

        void MASP(object sender, RoutedEventArgs e)
        {
            IniWrite("System", "PageIndex", "16", iniPath);
            PageIndex = 16;
            NavigationService.Navigate(Page_masp);
        }


    }
}