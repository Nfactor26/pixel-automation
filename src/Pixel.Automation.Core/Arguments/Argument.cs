using Pixel.Automation.Core.Extensions;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    /// <summary>
    /// ArgumentMode specifies how does a argument get or set value from a globals object / script variable
    /// </summary>
    [Flags]
    public enum ArgumentMode
    {
        None = 0,
        Default = 1, //Argument has a default value and doesn't use a globals object / script variable. 
        DataBound = 2, // Argument is bound to one of the properties on globals object / script variable or directly to a script variable
        Scripted  = 4 // A custom script is provided which will be executed to get or set value       
    }

    /// <summary>
    /// Argument can be used to get value from a globals object or script variable or to modify their state.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class Argument : NotifyPropertyChanged , ICloneable
    {
        //Note : Always set AllowedModes first before setting the Mode as value to be set on Mode is guarded by AllowedModes
        [DataMember(Order = 10)]
        [Browsable(false)]
        public ArgumentMode AllowedModes { get; set; } = ArgumentMode.Default | ArgumentMode.DataBound | ArgumentMode.Scripted;


        ArgumentMode mode = ArgumentMode.Default;
        /// <summary>
        /// Indicates the mode of operation of Argument i.e. if data binding is used / scripting is used / or Default value is provided (incase of InArgument)
        /// </summary>
        [DataMember(Order = 20)]
        [Browsable(false)]
        public ArgumentMode Mode
        {
            get => mode;
            set
            {
                if(this.AllowedModes.HasFlag(value))
                {
                    mode = value;
                    OnPropertyChanged();
                }              
            }
        }
       
        string propertyPath;
        /// <summary>
        /// Identifies the property on the dataModel class whose value will be feteched or set 
        /// depending on the direction of Argument
        /// </summary>
        [DataMember(Order = 40)]
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
        [DataMember(Order = 50)]
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

        [DataMember(Order = 60)]
        [Browsable(false)]
        /// <summary>
        /// Indicates whether it is possible to change type for this argument
        /// </summary>
        public bool CanChangeType { get; set; } = false;

        [Browsable(false)]
        /// <summary>
        /// Indicates whether it is possible to change the mode for this argument
        /// </summary>
        public bool CanChangeMode
        {
            get => (this.AllowedModes & (this.AllowedModes - 1)) != 0;
        }

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
