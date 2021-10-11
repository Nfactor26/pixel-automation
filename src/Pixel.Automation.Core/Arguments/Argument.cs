using Pixel.Automation.Core.Extensions;
using System;
using System.ComponentModel;
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
        /// <summary>
        /// Indicates whether it is possible to change type for this argument
        /// </summary>
        public bool CanChangeType { get; set; } = false;

        [DataMember]
        [Browsable(false)]
        /// <summary>
        /// Indicates whether it is possible to change the mode for this argument
        /// </summary>
        public bool CanChangeMode { get; set; } = true;

        /// <summary>
        /// Display name of the argument type
        /// </summary>
        public string ArgumentType
        {
            get
            {
                return GetDisplayName(GetArgumentType());
            }
        }

        /// <summary>
        /// Get the display name of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected string GetDisplayName(Type type)
        {
            return type.GetDisplayName();
        }

        /// <summary>
        /// Get the <see cref="Type"/> of the argument
        /// </summary>
        /// <returns></returns>
        public abstract Type GetArgumentType();

        /// <summary>
        /// Indicates whether this argument is configured 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            return (this.Mode == ArgumentMode.DataBound && !string.IsNullOrEmpty(PropertyPath)) || (this.Mode == ArgumentMode.Scripted && !string.IsNullOrEmpty(ScriptFile));
        }           

        /// <inheritdoc/>
        public abstract object Clone();
    }
}
