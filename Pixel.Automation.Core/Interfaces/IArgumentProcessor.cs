using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Contract for getting and setting value from and to arguments respectively.
    /// </summary>
    public interface IArgumentProcessor
    {
        /// <summary>
        /// Set the globals object for the IArgumentProcessor
        /// </summary>      
        /// <param name="dataModel"></param>
        void SetGlobals(object globals);

        /// <summary>
        /// Get the value of argument
        /// </summary>
        /// <typeparam name="T">Type of Argument</typeparam>
        /// <param name="argument">Argument<typeparamref name="T"/></param>
        /// <returns></returns>
        T GetValue<T>(Argument argument);

        /// <summary>
        /// Set the value of argument
        /// </summary>
        /// <typeparam name="T">Type of Argument</typeparam>
        /// <param name="argument">Argument<typeparamref name="T"/></param>
        /// <param name="value">value to be set</param>
        void SetValue<T>(Argument argument, T value);
    }
}
