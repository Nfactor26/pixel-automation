namespace Pixel.Automation.Core.Arguments
{
    public class SetValueScriptArguments<T,U> : ScriptArguments<T> where T : new()
    {
        public U ExtractedValue { get; private set; }

        public SetValueScriptArguments(T dataModel, U argumentValue) : base(dataModel)
        {
            this.ExtractedValue = argumentValue;
        }
    }
}
