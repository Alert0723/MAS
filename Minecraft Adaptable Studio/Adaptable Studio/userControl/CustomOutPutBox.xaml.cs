using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Adaptable_Studio.userControl
{
    /// <summary> CustomOutPutBox.xaml 的交互逻辑 </summary>
    public partial class CustomOutPutBox : UserControl
    {
        public CustomOutPutBox()
        {
            InitializeComponent();
        }

        bool spreadstate = false;

        #region Sidebutton
        private void SideButtonPressedEvent(object sender, MouseButtonEventArgs e)
        {
            spreadstate = !spreadstate;

            VisualStateManager.GoToState(this, "SideButtonPressed", true);
            if (spreadstate == false)
            {
                VisualStateManager.GoToState(this, "OutPutButtonUnspread", true);
                VisualStateManager.GoToState(this, "RetrievalButtonUnspread", true);
                OutPutButton.IsHitTestVisible = false;
                RetrievalButton.IsHitTestVisible = false;
            }
            if (spreadstate == true)
            {
                VisualStateManager.GoToState(this, "OutPutButtonNormal", true);
                VisualStateManager.GoToState(this, "RetrievalButtonNormal", true);
                OutPutButton.IsHitTestVisible = true;
                RetrievalButton.IsHitTestVisible = true;
            }
        }
        private void SideButtonMouseOverEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "SideButtonMouseOver", true);
        }

        private void SideButtonOverPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "SideButtonMouseOver", true);
        }
        #endregion

        #region OutPutButton
        private void OutPutButtonMouseOverEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "OutPutButtonMouseOver", true);
        }

        private void OutPutButtonPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "OutPutButtonPressed", true);
        }

        private void OutPutButtonOverPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "OutPutButtonMouseOver", true);
        }

        private void OutPutButtonMouseLeaveEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "OutPutButtonNormal", true);
        }
        #endregion

        #region RetrievalButton
        private void RetrievalButtonPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "RetrievalButtonPressed", true);
        }

        private void RetrievalButtonMouseOverEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "RetrievalButtonMouseOver", true);
        }

        private void RetrievalButtonMouseLeaveEvent(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "RetrievalButtonNormal", true);
        }

        private void RetrievalButtonOverPressedEvent(object sender, MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "RetrievalButtonMouseOver", true);
        }
        #endregion

        private void CloseSideButtonEvent(object sender, MouseEventArgs e)
        {
            spreadstate = false;
            OutPutButton.IsHitTestVisible = false;
            RetrievalButton.IsHitTestVisible = false;
            VisualStateManager.GoToState(this, "OutPutButtonUnspread", true);
            VisualStateManager.GoToState(this, "RetrievalButtonUnspread", true);
            VisualStateManager.GoToState(this, "SideButtonNormal", true);
        }
        
        public bool OutPut { get; set; }
        public bool Retrieval { get; set; }

        private void OutPut_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            OutPut = true;
        }

        private void Retrieval_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Retrieval = true;
        }
    }
}
