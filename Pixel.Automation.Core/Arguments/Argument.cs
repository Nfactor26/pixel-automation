using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// ArgumentMode specifies how does a argument get or set value from a globals object / script variable
    /// </summary>
    public enum ArgumentMode
    {
        Default, //Argument has a default value and doesn't use a globals object / script variable. 
        DataBound, // Argument is bound to one of the properties on globals object / script variable or directly to a script variable
        Scripted  // A custom script is provided which will be executed to get or set value       
    }

    /// <summary>
    /// Argument can be used to get value from a globals object or script variable or to modify their state.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class Argument : NotifyPropertyChanged , ICloneable
    {
        ArgumentMode mode;
        /// <summary>
        /// Indicates the mode of operation of Argument i.e. if data binding is used / scripting is used / or Default value is provided (incase of InArgument)
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public ArgumentMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                OnPropertyChanged();
            }
        }

        string propertyPath;
        /// <summary>
        /// Identifies the property on the dataModel class whose value will be feteched or set 
        /// depending on the direction of Argument
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public string PropertyPath
        {
            get => propertyPath;
            set
            {
                propertyPath = value;
                OnPropertyChanged();
            }
        }

        protected string scriptFile;
        /// <summary>
        /// Relative path of the .csx file 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public string ScriptFile
        {
            get => scriptFile;
            set
            {
                scriptFile = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [Browsable(false)]
        public bool CanChangeType { get; set; } = false;

        [DataMember]
        [Browsable(false)]
        public bool CanChangeMode { get; set; } = true;

        public string ArgumentType
        {
            get
            {
                return GetDisplayName(GetArgumentType());
            }
        }

        protected string GetDisplayName(Type type)
        {
            switch (type.IsGenericType)
            {
                case true:
                    if (type.ContainsGenericParameters)
                        return type.Name.Split('`')[0];
                    else
                        return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                case false:
                    return type.Name;
            }
            return type.Name;
        }

        public abstract Type GetArgumentType();

        public virtual bool IsConfigured()
        {
            return (this.Mode == ArgumentMode.DataBound && !string.IsNullOrEmpty(PropertyPath)) || (this.Mode == ArgumentMode.Scripted && !string.IsNullOrEmpty(ScriptFile));
        }           

        public abstract object Clone();
    }
}
