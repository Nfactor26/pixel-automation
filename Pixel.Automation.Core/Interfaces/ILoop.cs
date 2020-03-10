using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ILoop
    {

        bool ExitCriteriaSatisfied
        {
            get;set;
        }     
        
        //OutArgument<int> CurrentIteration { get;}
       
    }
}
