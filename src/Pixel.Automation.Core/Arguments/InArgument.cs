﻿using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{

    public interface IDefaultValueProvider<out T>
    {
        T GetDefaultValue();
    }

    /// <summary>
    /// Use InArgument<T> when you need to retrieve a value from globals object or script variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [Serializable]
    public class InArgument<T> : Argument, IDefaultValueProvider<T>
    {

        T defaultValue = default(T);
        [DataMember(IsRequired = false, Order = 30)]
        /// <summary>
        /// Default value of the argument
        /// </summary>
        public T DefaultValue
        {
            get
            {
                //If the type is changed, make sure defaultValue is not null . This can work only if type has default constructor
                if (this.Mode == ArgumentMode.Default && this.defaultValue == null)
                {
                    Type type = typeof(T);

                    if (type == typeof(string))
                    {
                        this.defaultValue = (T)((object)(string.Empty));
                    }

                    if ((!type.IsValueType) && (type.GetConstructor(Type.EmptyTypes) != null))
                    {                      
                        this.defaultValue = (T)Activator.CreateInstance(type);                       
                    }
                }
                return (this.Mode == ArgumentMode.Default) ? this.defaultValue : default;
            }
            set
            {
                defaultValue = value;
                OnPropertyChanged();
            }
        }      

        /// <summary>
        /// Default constructor
        /// </summary>
        public InArgument()
        {
           
        }

        /// <summary>
        /// Get the default value of the argument
        /// </summary>
        /// <returns></returns>
        public T GetDefaultValue()
        {
            return DefaultValue;
        }

        /// <inheritdoc/>
        public override Type GetArgumentType()
        {
            return typeof(T);
        }


        /// <inheritdoc/>
        public override bool IsConfigured()
        {
           return base.IsConfigured() || (this.Mode == ArgumentMode.Default && this.DefaultValue != null);
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            InArgument<T> clone = new InArgument<T>()
            {
                Mode = this.Mode,
                AllowedModes = this.AllowedModes,
                DefaultValue = this.DefaultValue,
                PropertyPath = this.PropertyPath,               
                CanChangeType = this.CanChangeType,
                ScriptFile = this.ScriptFile
            };
            return clone;
        }
    }
 
}

