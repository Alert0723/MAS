using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArmorStand.CustomControl
{
    /// <summary> CustomNumberBox.xaml 的交互逻辑 </summary>
    public partial class CustomNumberBox : UserControl
    {
        public CustomNumberBox()
        {
            InitializeComponent();
        }

        string _input;

        private void CheckNumber(object sender, TextChangedEventArgs e)
        {
            Regex rgx = new Regex(@"^[-]?\d+[.]?\d*$");
            _input = NumberBox.Text;
            if (!rgx.IsMatch(_input)) NumberBox.Text = "0";
        }

        #region 自定义属性
        double _value;
        [Description("当前值"), Category("User Control")]
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NumberBox.Text = _value.ToString();
            }
        }

        [Description("增减量"), Category("User Control")]
        public double Interval { get; set; }

        [Description("最大值"), Category("User Control")]
        public double Maximum { get; set; }

        [Description("最小值"), Category("User Control")]
        public double Minimum { get; set; }
        #endregion

        private void Increase_Value(object sender, MouseButtonEventArgs e)
        {
            Value = Convert.ToDouble(NumberBox.Text);
            Value += Interval;
            if (Value > Maximum) Value = Maximum;
            NumberBox.Text = Value.ToString();
            VisualStateManager.GoToState(this, "IncreasePressed", true);
        }

        private void Decrease_Value(object sender, MouseButtonEventArgs e)
        {
            Value = Convert.ToDouble(NumberBox.Text);
            Value -= Interval;
            if (Value < Minimum) Value = Minimum;
            NumberBox.Text = Value.ToString();
            VisualStateManager.GoToState(this, "DecreasePressed", true);
        }

        #region Dynamic control
        private void IncreaseMouseOverEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "IncreaseMouseOver", true);
        }

        private void DecreaseMouseOverEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "DecreaseMouseOver", true);
        }

        private void DecreaseNormalEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "DecreaseNormal", true);
        }

        private void IncreaseNormalEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "IncreaseNormal", true);
        }

        private void IncreaseOverPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "IncreaseMouseOver", true);
        }

        private void DecreaseOverPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "DecreaseMouseOver", true);
        }
        #endregion

        private void Value_Change(object sender, RoutedEventArgs e)
        {
            Value = double.Parse(NumberBox.Text);
            if (Value > Maximum) Value = Maximum;
            if (Value < Minimum) Value = Minimum;
            NumberBox.Text = Value.ToString();
        }

        private void Value_Change(object sender, MouseEventArgs e)
        {
            Value = double.Parse(NumberBox.Text);
            if (Value > Maximum) Value = Maximum;
            if (Value < Minimum) Value = Minimum;
            NumberBox.Text = Value.ToString();
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Value = double.Parse(NumberBox.Text);
                if (Value > Maximum) Value = Maximum;
                if (Value < Minimum) Value = Minimum;
                NumberBox.Text = Value.ToString();
                NumberBox.MoveFocus(new TraversalRequest(new FocusNavigationDirection()));
            }
        }
    }
}
