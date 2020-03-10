using System.Collections.Generic;


namespace Pixel.Automation.Core.Models
{
    public class ScriptResult
    {      
        public static readonly ScriptResult Empty = new ScriptResult();

        public static readonly ScriptResult Incomplete = new ScriptResult { IsCompleteSubmission = false };

        public object CurrentState { get; private set; }

        public object ReturnValue { get; private set; }

        public bool IsCompleteSubmission { get; private set; }

        public ScriptResult()
        {
            // Explicit default ctor to use as mock return value.
            IsCompleteSubmission = true;
        }

        public ScriptResult(object returnValue = null, object currentState = null)
        {
            ReturnValue = returnValue;
            CurrentState = currentState;  
         
            IsCompleteSubmission = true;
        }

    }
}
