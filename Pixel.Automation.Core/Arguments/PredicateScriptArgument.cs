namespace Pixel.Automation.Core.Arguments
{
    public class PredicateScriptArgument<T, U> : ScriptArguments<T> where T : new()
    {
        public U Control { get; private set; }

        public PredicateScriptArgument(T dataModel, U control) : base(dataModel)
        {
            this.Control = control;           
        }
    }
}
