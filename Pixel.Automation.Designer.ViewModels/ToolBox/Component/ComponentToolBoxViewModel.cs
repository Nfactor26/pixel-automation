using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using ToolBoxItem = Pixel.Automation.Core.Attributes.ToolBoxItemAttribute;

namespace Pixel.Automation.Designer.ViewModels
{
    public class ComponentToolBoxViewModel : ToolBox, IComponentBox
    {
        ITypeProvider knownTypesProvider;

        string filterText=string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;

                var view = CollectionViewSource.GetDefaultView(Components);
                view.Refresh();
                //if (!string.IsNullOrEmpty(filterText))
                //{                
                //    //if(view.GroupDescriptions.Count>0)  
                //    //view.GroupDescriptions.RemoveAt(0);
                //    view.Refresh();
                //}
                //else
                //{
                //    view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                //}
                NotifyOfPropertyChange(() => FilterText);
               
            }
        }

        BindableCollection<ComponentToolBoxItem> components;
        public BindableCollection<ComponentToolBoxItem> Components
        {
            get
            {
                return this.components;
            }
        }

        Dictionary<string, List<ComponentToolBoxItem>> customComponents;

        public override PaneLocation PreferredLocation => PaneLocation.Left;

        public ComponentToolBoxViewModel(ITypeProvider knownTypesProvider)
        {
            this.knownTypesProvider = knownTypesProvider;

            this.DisplayName = "Components";
            this.components = new BindableCollection<ComponentToolBoxItem>();
            this.customComponents = new Dictionary<string, List<ComponentToolBoxItem>>();
            var groupedItems = CollectionViewSource.GetDefaultView(Components);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("SubCategory"));
            groupedItems.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription("SubCategory", ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
           
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                if(!string.IsNullOrEmpty(filterText))
                {
                    return (a as ComponentToolBoxItem).Name.ToLower().Contains(filterText.ToLower());
                }
                return true;
            });

            LoadToolBoxItems();
        }

        private void LoadToolBoxItems()
        {
            this.components.Clear();

            foreach (var item in knownTypesProvider.GetAllTypes())
            {
                if(TryCreateToolBoxItem(item,out ComponentToolBoxItem toolBoxItem))
                {
                    this.components.Add(toolBoxItem);
                }
            }
        }

        private bool TryCreateToolBoxItem(Type typeOfComponent,out ComponentToolBoxItem toolBoxItem)
        {
            if (typeOfComponent.IsPublic && !typeOfComponent.IsAbstract)
            {
                var toolBoxItemAttribute = typeOfComponent.GetCustomAttribute(typeof(ToolBoxItem)) as ToolBoxItem;
                if (toolBoxItemAttribute != null)
                {
                    ComponentToolBoxItem component = new ComponentToolBoxItem();
                    component.Name = toolBoxItemAttribute.Name;
                    component.IconSource = toolBoxItemAttribute.IconSource;
                    component.Description = toolBoxItemAttribute.Description;                   
                    component.Category = toolBoxItemAttribute.Category;
                    component.SubCategory = toolBoxItemAttribute.SubCategory;                  
                    component.Tags = toolBoxItemAttribute.Tags == null ? new List<string>() : toolBoxItemAttribute.Tags;
                    component.TypeOfComponent = typeOfComponent;                   
                    toolBoxItem = component;
                    return true;                    
                }              
            }

            toolBoxItem = null;
            return false;
        }

     
        public void AddCustomComponents(string owner, List<Type> components)
        {
            List<ComponentToolBoxItem> customToolBoxItems = new List<ComponentToolBoxItem>();
            components.ForEach(c =>
            {
                if (TryCreateToolBoxItem(c, out ComponentToolBoxItem toolBoxItem))
                {
                    customToolBoxItems.Add(toolBoxItem);
                }
            });
            this.customComponents.Add(owner, customToolBoxItems);                         
        }

        public void RemoveCustomComponents(string owner)
        {
            if(this.customComponents.ContainsKey(owner))
            {
                foreach(var toolBoxItem in this.customComponents[owner])
                {
                    this.components.Remove(toolBoxItem);
                }              
                this.customComponents.Remove(owner);
            }           

        }

        public void HideCustomComponents(string owner)
        {
            if (this.customComponents.ContainsKey(owner))
            {
                foreach (var toolBoxItem in this.customComponents[owner])
                {
                    this.components.Remove(toolBoxItem);
                }
            }
         
        }

        public void ShowCustomComponents(string owner)
        {
            if (this.customComponents.ContainsKey(owner))
            {
                foreach (var toolBoxItem in this.customComponents[owner])
                {
                    this.components.Add(toolBoxItem);
                }               
            }          
        }

    }
}
