using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// Func argument is a script based argument that provides script based extension points
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class FuncArgument<T> : Argument
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FuncArgument()
        {
            this.AllowedModes = ArgumentMode.Scripted;
            this.Mode = ArgumentMode.Scripted;
            this.CanChangeType = false;
        }

        /// <inheritdoc/>
        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            FuncArgument<T> clone = new FuncArgument<T>()
            {
                Mode = this.Mode,
                AllowedModes = this.AllowedModes,
                CanChangeType = this.CanChangeType,
                ScriptFile = this.ScriptFile
            };
            return clone;
        }
    }
}
