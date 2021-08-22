

namespace Pixel.Automation.Editor.Core
{
    /// <summary>
    /// Anchorable screens are docked screens along the sides of host window
    /// </summary>
    public abstract class Anchorable : ScreenBase, IAnchorable
    { 
        /// <inheritdoc/>
        public override bool IsActive => true;
        
        /// <inheritdoc/>
        public virtual PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        /// <inheritdoc/>
        public virtual double PreferredWidth
        {
            get
            {
                return 250;
            }
        }

        /// <inheritdoc/>
        public virtual double PreferredHeight
        {
            get
            {
                return 250;
            }
        }

        /// <summary>
        /// Guard method to check if screen can be closed.
        /// Always returns true.
        /// </summary>
        /// <returns></returns>
        public override bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// Close screen will only set IsVisible to false and will close the instance
        /// </summary>
        public override void CloseScreen()
        {
            this.IsVisible = false;
        }
    }
}
