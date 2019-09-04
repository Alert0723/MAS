using MahApps.Metro.Controls;
using System.Windows;

namespace Adaptable_Studio
{
    /// <summary> OutPut.xaml 的交互逻辑 </summary>
    public partial class OutPut : MetroWindow
    {
        public OutPut()
        {
            InitializeComponent();
            Copy.Click += Copy_Click;
        }

        /// <summary> 复制代码 </summary>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(Print.Text);
        }

    }
}