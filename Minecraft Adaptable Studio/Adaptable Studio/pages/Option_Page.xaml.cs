using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Adaptable_Studio.IniConfig;
using static Adaptable_Studio.MainWindow;

namespace Adaptable_Studio
{
    /// <summary> Option_Page.xaml 的交互逻辑 </summary>
    public partial class Option_Page : Page
    {
        public Option_Page()
        {
            InitializeComponent();

            #region Events
            this.Loaded += Page_Loaded;
            Lang_CN.Click += LangSave;
            Lang_EN.Click += LangSave;
            Back.Click += Back_Click;

            Theme_Blue.Click += ThemeSave;
            Theme_Green.Click += ThemeSave;
            Theme_Purple.Click += ThemeSave;

            OptionGrid.MouseDown += OptionSave;
            OptionGrid.MouseLeave += OptionSave;
            #endregion
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //设置选项加载
            //语言
            if (_langCN)
                Lang_CN.IsChecked = true;
            else
                Lang_EN.IsChecked = true;

            //引导
            if (Guidance)
                Guidance_on.IsChecked = true;
            else
                Guidance_off.IsChecked = true;

            //主题色
            foreach (var item in Theme_Panel.Children)
            {
                if (item is RadioButton && ((RadioButton)item).Name == DefaultTheme)
                    ((RadioButton)item).IsChecked = true;
            }

        }

        /// <summary> 语言切换 </summary>
        void LangSave(object sender, RoutedEventArgs e)
        {
            _langCN = (bool)Lang_CN.IsChecked;//语言
            //语言文件初始化
            ResourceDictionary dict = new ResourceDictionary();
            if (_langCN)
                dict.Source = new Uri(@"lang\CN.xaml", UriKind.Relative);
            else
                dict.Source = new Uri(@"lang\EN.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = dict;//资源赋值

            IniWrite("System", "Lang", _langCN.ToString(), iniPath);
        }

        void ThemeSave(object sender, RoutedEventArgs e)
        {
            ResourceDictionary ThemeDict = new ResourceDictionary();
            ThemeDict.Source = new Uri(((RadioButton)sender).Tag.ToString(), UriKind.Relative);

            Application.Current.Resources.MergedDictionaries[1] = ThemeDict;//资源赋值

            DefaultTheme = ((RadioButton)sender).Name;
            IniWrite("System", "Theme", DefaultTheme, iniPath);
        }

        void OptionSave(object sender, MouseEventArgs e)
        {
            Option_save();
        }

        void OptionSave(object sender, MouseButtonEventArgs e)
        {
            Option_save();
        }

        /// <summary> 保存设置 </summary>
        void Option_save()
        {
            Guidance = (bool)Guidance_on.IsChecked;//引导开关

            #region ini写入
            IniWrite("System", "Guidance", Guidance.ToString(), iniPath);//引导

            #endregion
        }

    }
}