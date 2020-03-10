using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Arguments
{
    public enum ArgumentMode
    {
        Default,
        DataBound,
        Scripted        
    }

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
                OnPropertyChanged("Mode");
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
                OnPropertyChanged("PropertyPath");
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
                OnPropertyChanged("ScriptFile");
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
