﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components
{
    [DataContract]
    [Serializable]
    public abstract class ControlEntity : Entity, IControlEntity
    {
        private string controlId;
        [Browsable(false)]
        public string ControlId
        {
            get => this.controlId;
        }

        /// <summary>
        /// Control file inside the repository 
        /// </summary>
        [DataMember]
        [Category("File Details")]
        [Browsable(false)]
        public string ControlFile { get; set; } = string.Empty;

        [DataMember]
        [DisplayName("Search Root")]
        [Category("Control Details")]
        [Browsable(true)]
        public virtual Argument SearchRoot { get; set; } = new InArgument<UIControl>()
        {
            AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted,
            Mode = ArgumentMode.DataBound,
            CanChangeType = false
        };

        private LookupMode lookupMode = LookupMode.FindSingle;
        [DataMember]
        [Display(Name = "Look Up Mode", GroupName = "Search Strategy", Order = 20)]
        [Description("Find single control or Find All control and apply index based or custom filter")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public LookupMode LookupMode
        {
            get
            {
                switch (lookupMode)
                {
                    case LookupMode.FindSingle:
                        this.SetDispalyAttribute(nameof(FilterMode), false);
                        this.SetDispalyAttribute(nameof(Index), false);
                        this.SetDispalyAttribute(nameof(Filter), false);
                        break;
                    case LookupMode.FindAll:
                        this.SetDispalyAttribute(nameof(FilterMode), true);
                        break;
                }
                return lookupMode;
            }
            set
            {
                lookupMode = value;
                OnPropertyChanged(nameof(LookupMode));
                OnPropertyChanged(nameof(FilterMode));
            }
        }

        private FilterMode filterMode = FilterMode.Index;
        [DataMember]
        [Display(Name = "Filter By", GroupName = "Search Strategy", Order = 30, AutoGenerateField = false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public FilterMode FilterMode
        {
            get
            {
                switch (filterMode)
                {
                    case FilterMode.Index:
                        this.SetDispalyAttribute(nameof(Filter), false);
                        this.SetDispalyAttribute(nameof(Index), true);
                        break;
                    case FilterMode.Custom:
                        InitializeFilter();
                        this.SetDispalyAttribute(nameof(Filter), true);
                        this.SetDispalyAttribute(nameof(Index), false);
                        break;
                }
                return filterMode;
            }
            set
            {
                filterMode = value;
                OnPropertyChanged(nameof(FilterMode));
            }
        }

        [DataMember]
        [Display(Name = "Index", GroupName = "Search Strategy", Order = 40)]
        [Description("Bind to current Iteration when used inside loop")]
        public Argument Index { get; set; } = new InArgument<int>() { DefaultValue = 0, CanChangeType = false, Mode = ArgumentMode.Default };

        private Argument filter;
        [DataMember]
        [Browsable(false)]
        [Display(Name = "Filter Script", GroupName = "Search Strategy", Order = 40)]
        [Description("When using FindAll LookupMode, provide a script to Filter the result")]
        public virtual Argument Filter
        {
            get => filter;
            set
            {
                filter = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [Display(Name = "Enable Caching", GroupName = "Caching", Order = 50,
            Description = "Subsequent requests for target control from child components will return cached value if caching is enabled")]
        public bool CacheControl { get; set; } = false;

        private ControlDescription controlDescription;
        [Browsable(false)]
        public ControlDescription ControlDescription
        {
            get
            {
                if(controlDescription == null)
                {
                    controlDescription = LoadControlDescription();
                }
                return controlDescription;
            }
        }

        [Browsable(false)]
        public IControlIdentity ControlDetails
        {
            get => ControlDescription.ControlDetails;
          
        }

        protected abstract void InitializeFilter();

        public ControlEntity(string name = "Control Entity", string tag = "ControlEntity") : base(name, tag)
        {

        }

        protected virtual ControlDescription LoadControlDescription()
        {
            if (File.Exists(this.ControlFile))
            {
                ISerializer serializer = this.EntityManager.GetServiceOfType<ISerializer>();
                var controlDescription = serializer.Deserialize<ControlDescription>(this.ControlFile);
                this.controlId = controlDescription.ControlId;
                return controlDescription;              
            }

            throw new FileNotFoundException("Control file : {ControlFile} could not be found", this.ControlFile);
        }

        public override IEnumerable<IComponent> GetNextComponentToProcess()
        {
            foreach (var component in base.GetNextComponentToProcess())
            {
                yield return component;
            }
        }

        public abstract Task<UIControl> GetControl();

        public abstract Task<IEnumerable<UIControl>> GetAllControls();

        public override bool ValidateComponent()
        {
            base.ValidateComponent();
            if (this.ControlDetails.LookupType.Equals(Core.Enums.LookupType.Relative) && !(this.Parent is ControlEntity))
            {
                IsValid = false;
            }
            return IsValid;
        }

        protected async Task<T> GetElementAtIndex<T>(IEnumerable<T> foundControls)
        {
            int index =  await ArgumentProcessor.GetValueAsync<int>(this.Index);
            if (foundControls.Count() > index)
            {
                var foundControl = foundControls.ElementAt(index);
                //HighlightElement(foundControl);
                return foundControl;
            }
            throw new IndexOutOfRangeException($"Found {foundControls.Count()} controls.Desired index : {index} is greater than number of found controls");
        }

        protected T GetElementMatchingCriteria<T>(IEnumerable<T> foundControls)
        {
            foreach (var foundControl in foundControls)
            {
                bool found = (bool)ApplyPredicate(this.Filter.ScriptFile, foundControl).Result;
                if (found)
                {
                    //HighlightElement(foundControl);
                    return foundControl;
                }

            }
            throw new Exception($"Found {foundControls.Count()} controls. All controls failed filter criteria");
        }

        protected async Task<bool> ApplyPredicate<T>(string predicateScriptFile, T targetElement)
        {

            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var fn = await scriptEngine.CreateDelegateAsync<Func<IComponent, T, bool>>(predicateScriptFile);
            bool isMatch = fn(this, targetElement);
            return isMatch;
        }

        public void Reload()
        {
            this.controlDescription = LoadControlDescription();
            OnPropertyChanged(nameof(ControlDescription));
            OnPropertyChanged(nameof(ControlDetails));
        }

    }
}
