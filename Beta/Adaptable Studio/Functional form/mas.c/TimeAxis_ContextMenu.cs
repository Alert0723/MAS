using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Adaptable_Studio
{
    class TimeAxis_ContextMenu_CheckBox : CheckBox
    {
        bool[][] isReversed;
        long clickMark;
        public TimeAxis_ContextMenu_CheckBox(bool[][] IsReversed, long ClickMark)
        {
            isReversed = IsReversed;
            clickMark = ClickMark;
            Click += ClickBox_Click;
        }

        void ClickBox_Click(object sender, RoutedEventArgs e)
        {
            isReversed[clickMark][int.Parse(((CheckBox)sender).Tag.ToString())] = (bool)((CheckBox)sender).IsChecked;
        }
    }

    class TimeAxis_ContextMenu : ContextMenu
    {
        //0~2:head
        //3~5:body
        //6~8:left arm
        //9~11:right arm
        //12~14:left leg
        //15~17:right leg
        //18:rotation
        bool[][] isReversed;
        bool[] oneReversed;

        /// <summary> 时间轴过渡段 右键菜单 </summary>
        /// <param Name = "IsReversed">当前帧方向数据</param>
        /// <param Name = "OneReversed">全局帧方向数据</param>
        /// <param Name = "ClickMark">帧位置</param>
        public TimeAxis_ContextMenu(bool[][] IsReversed, bool[] OneReversed, long ClickMark)
        {
            isReversed = IsReversed;
            oneReversed = OneReversed;
            #region 初始化
            Style = FindResource("CustomContextMenu") as Style;
            this.Items.Add(new MenuItem()
            {
                Header = FindResource("Direction_ReserveAll"),
                Style = FindResource("CustomMenuItem") as Style
            });
            this.Items.Add(new Separator() { Style = FindResource("CustomSeparator") as Style });

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("Head"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "HeadX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[0],Tag = "0"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "HeadY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[1],Tag = "1"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "HeadZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[2],Tag = "2"}
                }
            });

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("Body"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "BodyX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[3],Tag = "3"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "BodyY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[4],Tag = "4"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "BodyZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[5],Tag = "5"}
                }
            });

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("LeftArm"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftArmX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[6],Tag = "6"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftArmY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[7],Tag = "7"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftArmZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[8],Tag = "8"}
                }
            });

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("RightArm"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightArmX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[9],Tag = "9"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightArmY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[10],Tag = "10"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightArmZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[11],Tag = "11"}
                }
            });

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("LeftLeg"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftLegX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[12],Tag = "12"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftLegY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[13],Tag = "13"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "LeftLegZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[14],Tag = "14"}
                }
            }); ;

            this.Items.Add(new MenuItem()
            {
                Style = FindResource("CustomMenuItem") as Style,
                Header = FindResource("RightLeg"),
                Items ={
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightLegX", Content = "X" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[15],Tag = "15"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightLegY", Content = "Y" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[16],Tag = "16"},
                    new TimeAxis_ContextMenu_CheckBox(IsReversed,ClickMark)
                    { Name = "RightLegZ", Content = "Z" ,Style = FindResource("CustomCheckBox") as Style,IsChecked = OneReversed[17],Tag = "17"}
                }
            });
        }

        #endregion

    }
}
