using Caliburn.Micro;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using System;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class ParameterDescription : PropertyChangedBase
    {
        bool isRequired;
        public bool IsRequired
        {
            get => isRequired;
            set
            {
                isRequired = value;
                NotifyOfPropertyChange();
            }
        }

        ParameterUsage usage;
        public ParameterUsage Usage
        {
            get => usage;
            set
            {
                usage = value;
                NotifyOfPropertyChange();
            }
        }

        public string PropertyName { get; private set; }

        public string DisplayType { get; private set; }

        public string NameSpace { get; private set; }

        public string Assembly { get; private set; }

        public Type PropertyType { get; private set; }

        public ParameterDescription(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            DisplayType = propertyType.GetDisplayName();
            NameSpace = propertyType.Namespace;
            Assembly = propertyType.Assembly.GetAssemblyName();
        }

    }
}
