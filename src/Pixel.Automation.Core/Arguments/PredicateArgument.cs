using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// Predicate argument is a script based argument that evaluates to a boolean value.
    /// This is used for decision making by some components.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class PredicateArgument<T> : Argument
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PredicateArgument()
        {
            this.Mode = ArgumentMode.Scripted;
            this.CanChangeType = true;
            this.CanChangeMode = false;
        }

        /// <inheritdoc/>
        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            PredicateArgument<T> clone = new PredicateArgument<T>()
            {
                Mode = this.Mode,            
                CanChangeMode = this.CanChangeMode,
                CanChangeType = this.CanChangeType,
                ScriptFile = this.ScriptFile
            };
            return clone;
        }
    }
}
