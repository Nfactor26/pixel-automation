using Caliburn.Micro;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Pixel.Automation.Editor.TypeBrowser
{
    public class ArgumentTypeBrowserViewModel : Screen, IArgumentTypeBrowser
    {
        IArgumentTypeProvider argumentTypeProvider;

        /// <summary>
        /// Collection of Available TypeDefinition
        /// </summary>
        public BindableCollection<TypeDefinition> AvailableTypes { get; } = new BindableCollection<TypeDefinition>();

        /// <summary>
        /// Collectoin of common TypeDefinition like string, int, List<T>, etc.
        /// </summary>
        public BindableCollection<TypeDefinition> CommonTypes { get; } = new BindableCollection<TypeDefinition>();

        /// <summary>
        /// Collection of  ArgumentTypePicker for each  Open Type argument for the currently selected type
        /// </summary>
        public BindableCollection<ArgumentTypePicker> SelectedTypeGenericParameters { get; private set; } = new BindableCollection<ArgumentTypePicker>();


        TypeDefinition selectedType;
        /// <summary>
        /// Selected Type 
        /// </summary>
        public TypeDefinition SelectedType
        {
            get => selectedType;
            set
            {
                if (value != null)  // we are using this value later. on closing window, this becomes null otherwise
                {
                    selectedType = value;                 
                    SelectedTypeGenericParameters.Clear();
                    if (selectedType.IsGenericType)
                    {                        
                        for (int i = 0; i < selectedType.ActualType.GetGenericArguments().Count(); i++)
                        {
                            SelectedTypeGenericParameters.Add(new ArgumentTypePicker(this.argumentTypeProvider));
                        }
                    }
                    NotifyOfPropertyChange(() => SelectedType);
                }

            }
        }

        /// <summary>
        /// Underlying Type for the selected TypeDefinition
        /// </summary>
        public Type ActualType { get; private set; }


        string filterText = string.Empty;
        /// <summary>
        /// Filter text condition for visible collection of TypeDefintion on UI
        /// </summary>
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                var view = CollectionViewSource.GetDefaultView(AvailableTypes);
                view.Refresh();
                NotifyOfPropertyChange(() => FilterText);
            }
        }

        bool isBrowseMode = true;
        /// <summary>
        /// Indicates whether the TypeDefinition collection visible on UI is enabled/disabled
        /// </summary>
        public bool IsBrowseMode
        {
            get => this.isBrowseMode;
            set
            {
                this.isBrowseMode = value;
                NotifyOfPropertyChange(() => IsBrowseMode);
            }
        }

        bool showAll;
        /// <summary>
        /// Toggle between common types and all available types for selection on UI
        /// </summary>
        public bool ShowAll
        {
            get => this.showAll;
            set
            {
                this.showAll = value;
                switch (value)
                {
                    case true:
                        ShowAllAssemblies();
                        break;
                    case false:
                        ShowCommonTypes();
                        break;
                }
            }
        }

        public ArgumentTypeBrowserViewModel(IArgumentTypeProvider argumentTypeProvider)
        {
            this.argumentTypeProvider = argumentTypeProvider;
            this.DisplayName = "Browse for .NET types";
            ShowCommonTypes();
            AddGroupDefinition();
        }


        public ArgumentTypeBrowserViewModel(IArgumentTypeProvider argumentTypeProvider,
            TypeDefinition selectedType) : this(argumentTypeProvider)
        {
            this.SelectedType = selectedType;
            this.IsBrowseMode = false;
        }

        private void AddGroupDefinition()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(AvailableTypes);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("AssemblyName"));
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("NameSpace"));
            groupedItems.SortDescriptions.Add(new SortDescription("AssemblyName", ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription("NameSpace", ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as TypeDefinition).FullName.ToLower().Contains(this.filterText.ToLower());
            });
        }


        private void ShowCommonTypes()
        {
            this.AvailableTypes.Clear();
            this.AvailableTypes.AddRange(argumentTypeProvider.GetCustomDefinedTypes());
            this.AvailableTypes.AddRange(argumentTypeProvider.GetCommonTypes());
        }

        private void ShowAllAssemblies()
        {
            this.AvailableTypes.Clear();
            this.AvailableTypes.AddRange(argumentTypeProvider.GetCustomDefinedTypes());
            this.AvailableTypes.AddRange(argumentTypeProvider.GetAllKnownTypes());
        }


        public override async Task TryCloseAsync(bool? dialogResult = null)
        {
            AvailableTypes.Clear();
            await base.TryCloseAsync(dialogResult);
        }

        public TypeDefinition GetCreatedType()
        {
            return new TypeDefinition(this.ActualType);
        }


        public Argument CreateInArgumentForSelectedType()
        {
            Type generic = typeof(InArgument<>);
            Type[] typeArgs = { ActualType };
            Type typeToConstruct = generic.MakeGenericType(typeArgs);
            Argument createdInstance = (Argument)Activator.CreateInstance(typeToConstruct);
            return createdInstance;
        }

        public Argument CreateOutArgumentForSelectedType()
        {
            Type generic = typeof(OutArgument<>);
            Type[] typeArgs = { ActualType };
            Type typeToConstruct = generic.MakeGenericType(typeArgs);
            Argument createdInstance = (Argument)Activator.CreateInstance(typeToConstruct);
            return createdInstance;
        }

        private bool TryCreateDesiredType(TypeDefinition selectedTypeDefinition)
        {
            try
            {
                if (selectedTypeDefinition.IsGenericType && selectedTypeDefinition.ActualType.ContainsGenericParameters)
                {
                    Type generic = selectedTypeDefinition.ActualType;
                    Type[] typeArgs = SelectedTypeGenericParameters.Select(p => p.ActualType).ToArray();
                    Type constructedType = generic.MakeGenericType(typeArgs);
                    ActualType = constructedType;
                }
                else
                {
                    ActualType = SelectedType.ActualType;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async void Ok()
        {
            if (TryCreateDesiredType(this.selectedType))
            {
                await this.TryCloseAsync(true);
            }
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }

        [Conditional("DEBUG")]
        void CheckIfAssemblyHasAnyIssues(Assembly assembly)
        {
            if (assembly.IsDynamic)
                return;
            if (string.IsNullOrEmpty(assembly.FullName))
                Debug.WriteLine(false, "Assembly name can't be null or empty");
            var groupedTypes = assembly.GetExportedTypes().Where(t => (!t.IsAbstract && t.IsClass) || t.IsValueType || t.IsEnum).GroupBy(t => t.Namespace);
            foreach (var group in groupedTypes)
            {
                if (string.IsNullOrEmpty(group.Key))
                    Debug.WriteLine(false, "Namespace can't be null or empty");
            }

        }

    }

}
