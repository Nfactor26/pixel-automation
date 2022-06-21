using Pixel.Automation.Core.Arguments;
using System.Threading.Tasks;

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
        void Initialize(IScriptEngine scriptEngine, object globals);

        /// <summary>
        /// Get the value of argument
        /// </summary>
        /// <typeparam name="T">Type of Argument</typeparam>
        /// <param name="argument">Argument<typeparamref name="T"/></param>
        /// <returns></returns>
        Task<T> GetValueAsync<T>(Argument argument);

        /// <summary>
        /// Set the value of argument
        /// </summary>
        /// <typeparam name="T">Type of Argument</typeparam>
        /// <param name="argument">Argument<typeparamref name="T"/></param>
        /// <param name="value">value to be set</param>
        Task SetValueAsync<T>(Argument argument, T value);
    }
}
