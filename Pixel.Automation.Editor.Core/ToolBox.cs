

namespace Pixel.Automation.Editor.Core
{
    public abstract class ToolBox : ScreenBase, IToolBox
    {
        protected bool isActiveItem;
        public virtual bool IsActiveItem
        {
            get => isActiveItem;
            set
            {
                isActiveItem = value;
                NotifyOfPropertyChange(() => IsActiveItem);
            }

        }
        public virtual PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public virtual double PreferredWidth
        {
            get
            {
                return 250;
            }
        }

        public virtual double PreferredHeight
        {
            get
            {
                return 250;
            }
        }

        public override bool CanClose()
        {
            return true;
        }

        public override void CloseScreen()
        {
            this.IsVisible = false;
        }
    }
}
