namespace Pixel.Automation.Core.Arguments
{
    public class ScriptArguments<T> where T : new()
    {
        public T DataModel { get; }     

        public ScriptArguments(T dataModel) 
        {
            this.DataModel = dataModel;                     
        }
    }
}
