namespace Pixel.Automation.Core
{
    public interface IScriptEngineFactory
    {
        /// <summary>
        /// Create script engine
        /// </summary>
        /// <param name="useCaching">indicates if script engine should support caching</param>
        /// <returns></returns>
        IScriptEngine CreateScriptEngine(bool useCaching);

        /// <summary>
        /// Create  script engine for use with interactive execution
        /// </summary>
        /// <returns></returns>
        IScriptEngine CreateInteractiveScriptEngine();
    }
}
