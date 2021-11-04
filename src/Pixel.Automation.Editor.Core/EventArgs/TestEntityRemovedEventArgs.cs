using Dawn;
using Pixel.Automation.Core;
using System;

namespace Pixel.Automation.Editor.Core
{
    /// <summary>
    /// Raised when an entity is removed from designer by an external module e.g. closing a test case or fixture
    /// from test explorer which was previously opened for edit. The editor will listen for this notification
    /// and if it happens to be the ActiveItem , editor will update the ActiveItem to Root.
    /// </summary>
    public class TestEntityRemovedEventArgs : EventArgs
    {
        public Entity RemovedEntity { get; private set; }   

        public TestEntityRemovedEventArgs(Entity removedEntity)
        {
            this.RemovedEntity = Guard.Argument(removedEntity).NotNull();
        }
    }
}
