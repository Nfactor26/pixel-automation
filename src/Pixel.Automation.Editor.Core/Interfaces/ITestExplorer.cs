using System;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface ITestExplorer : IDisposable
    {
        /// <summary>
        /// Sets the TestRepositoryManager instance belonging to an Automation process.
        /// </summary>
        /// <param name="instance"></param>
        void SetActiveInstance(ITestRepositoryManager instance);

        /// <summary>
        /// Clear the TestRepositoryManager instance belong to an Automation process.
        /// For ex : If two automation process are open, first automation process on loosing focus will clear active instance while
        /// next automation process that gets activated will set active instance
        /// </summary>
        void ClearActiveInstance();

    }
}
