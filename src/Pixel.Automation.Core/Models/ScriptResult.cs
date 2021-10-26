namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Result of execution of a script file
    /// </summary>
    public class ScriptResult
    {      
        public static readonly ScriptResult Empty = new ScriptResult();

        public static readonly ScriptResult Incomplete = new ScriptResult { IsCompleteSubmission = false };

        /// <summary>
        /// Stores the current state when a script is executed so next executions can use this as a starting point
        /// </summary>
        public object CurrentState { get; private set; }

        /// <summary>
        /// Return value from execution of the script
        /// </summary>
        public object ReturnValue { get; private set; }

        /// <summary>
        /// Indicates if the script is a complete submission
        /// </summary>
        public bool IsCompleteSubmission { get; private set; }

        /// <summary>
        /// Explicit default ctor to use as mock return value.
        /// </summary>
        public ScriptResult()
        {           
            IsCompleteSubmission = true;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="returnValue"></param>
        /// <param name="currentState"></param>
        public ScriptResult(object returnValue = null, object currentState = null)
        {
            ReturnValue = returnValue;
            CurrentState = currentState;           
            IsCompleteSubmission = true;
        }

    }
}
