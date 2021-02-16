using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// Use OutArgument<T> when you need to modify some value on a globals object or script variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class OutArgument<T> : Argument
    {
        public OutArgument()
        {
            this.Mode = ArgumentMode.DataBound;
        }

        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        public override bool Equals(object obj)
        {
            if (obj is OutArgument<T> other)
            {
                if (other.ArgumentType.Equals(this.ArgumentType))
                {
                    switch (other.Mode)
                    {
                        case ArgumentMode.DataBound:
                            if (this.Mode == ArgumentMode.DataBound && this.PropertyPath.Equals(other.PropertyPath))
                                return true;
                            break;
                        case ArgumentMode.Default:                         
                            return false; //OutArgument doesn't support Default mode
                        case ArgumentMode.Scripted:
                            return false;
                    }
                }
            }
            return false;
        }
     
        public override object Clone()
        {
            OutArgument<T> clone = new OutArgument<T>()
            {
                Mode = this.Mode,
                PropertyPath = this.PropertyPath,              
                CanChangeMode = this.CanChangeMode,
                CanChangeType = this.CanChangeType
            };
            return clone;
        }
    }
}
