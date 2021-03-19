using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Linq;

namespace Pixel.Automation.Editor.TypeBrowser
{
    public class ArgumentTypePicker : NotifyPropertyChanged
    {
        IArgumentTypeProvider argumentTypeProvider;

        bool isBrowsingForArguments = false;

        object selectedType;
        public object SelectedType
        {
            get => selectedType;
            set
            {
                if (value != null && !isBrowsingForArguments)
                {
                    isBrowsingForArguments = true;
                    if (value is TypeDefinition selectedTypeDefintion)
                    {
                        if (selectedTypeDefintion.IsGenericType)
                        {
                            ArgumentTypeBrowserViewModel argumentTypeBrowser = new ArgumentTypeBrowserViewModel(argumentTypeProvider, selectedTypeDefintion);
                            BrowserAndPickArgument(argumentTypeBrowser);
                        }
                        else
                        {
                            ActualType = selectedTypeDefintion.ActualType;
                            selectedType = value;
                        }
                    }
                    else if (value is string)
                    {
                        ArgumentTypeBrowserViewModel argumentTypeBrowser = new ArgumentTypeBrowserViewModel(argumentTypeProvider);
                        BrowserAndPickArgument(argumentTypeBrowser);
                    }
                    isBrowsingForArguments = false;
                    OnPropertyChanged();
                }

            }
        }

        private async void BrowserAndPickArgument(ArgumentTypeBrowserViewModel argumentTypeBrowser)
        {
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            var result = await windowManager.ShowDialogAsync(argumentTypeBrowser);
            if (result.HasValue && result.Value)
            {
                ActualType = argumentTypeBrowser.ActualType;
                if (!this.CommonTypes.Any(t => (t is TypeDefinition) && (t as TypeDefinition).ActualType.Equals(ActualType)))
                {
                    var createdTypeDefintion = new TypeDefinition(ActualType);
                    this.CommonTypes.Add(createdTypeDefintion);
                    selectedType = createdTypeDefintion;
                }
                else
                {
                    //TODO : without modifying commontypes , ui won't udpate .. check what's the problem
                    this.CommonTypes.Clear();
                    this.CommonTypes.AddRange(argumentTypeProvider.GetCommonTypes());
                    this.CommonTypes.Add("Browser for more...");
                    selectedType = this.CommonTypes.FirstOrDefault(t => (t as TypeDefinition)?.ActualType.Equals(ActualType) ?? false);
                }
            }
            else
            {
                selectedType = null;
                ActualType = null;
            }
        }

        public BindableCollection<object> CommonTypes { get; }

        public Type ActualType { get; private set; }

        public ArgumentTypePicker(IArgumentTypeProvider argumentTypeProvider)
        {
            this.argumentTypeProvider = argumentTypeProvider;
            this.CommonTypes = new BindableCollection<object>(argumentTypeProvider.GetCommonTypes());
            this.CommonTypes.Add("Browser for more...");
        }

    }
}

