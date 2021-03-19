using System;
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

        T defaultValue;
        [DataMember(IsRequired = false)]
        public T DefaultValue
        {
            get
            {
                //If the type is changed, make sure defaultValue is not null . This can work only if type has default constructor
                if (this.defaultValue == null)
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
                return this.defaultValue;
            }
            set
            {
                defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }      

        public InArgument()
        {
            this.Mode = ArgumentMode.Default;
        }

        public T GetDefaultValue()
        {
            return DefaultValue;
        }

        public override Type GetArgumentType()
        {
            return typeof(T);
        }

        public override bool IsConfigured()
        {
            return base.IsConfigured() || (this.Mode == ArgumentMode.Default && this.DefaultValue != null);
        }

        public override bool Equals(object obj)
        {
            if (obj is InArgument<T> other)
            {
                if(other.ArgumentType.Equals(this.ArgumentType))
                {
                    switch(other.Mode)
                    {
                        case ArgumentMode.DataBound:
                            if (this.Mode == ArgumentMode.DataBound && this.PropertyPath.Equals(other.PropertyPath))
                                return true;
                            break;
                        case ArgumentMode.Default:
                            if (this.Mode == ArgumentMode.Default && this.defaultValue.Equals(other.defaultValue))
                                return true;
                            return false;                           
                        case ArgumentMode.Scripted:
                            return false;                            
                    }
                }
            }
            return false;
        }

        public override object Clone()
        {
            InArgument<T> clone = new InArgument<T>()
            {
               Mode = this.Mode,
               DefaultValue = this.DefaultValue,
               PropertyPath = this.PropertyPath,
               CanChangeMode = this.CanChangeMode,
               CanChangeType = this.CanChangeType
            };
            return clone;
        }

      
    }
 
}

