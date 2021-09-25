﻿using System;
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
        /// <summary>
        /// Default constructor
        /// </summary>
        public OutArgument()
        {
            this.Mode = ArgumentMode.DataBound;
        }
       
        /// <inheritdoc/>
        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            OutArgument<T> clone = new OutArgument<T>()
            {
                Mode = this.Mode,
                PropertyPath = this.PropertyPath,              
                CanChangeMode = this.CanChangeMode,
                CanChangeType = this.CanChangeType,
                ScriptFile = this.ScriptFile
            };
            return clone;
        }
    }
}
