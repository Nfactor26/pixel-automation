using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// Not actually an argument. Just a place holder to defined filter/predicate scripts
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class PredicateArgument<T> : Argument
    {
        public PredicateArgument()
        {
            this.Mode = ArgumentMode.Scripted;
            this.CanChangeType = true;
            this.CanChangeMode = false;
        }

        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        public override bool Equals(object obj)
        {           
            return false;
        }
       
        public override object Clone()
        {
            PredicateArgument<T> clone = new PredicateArgument<T>()
            {
                Mode = this.Mode,            
                CanChangeMode = this.CanChangeMode,
                CanChangeType = this.CanChangeType
            };
            return clone;
        }
    }
}
