using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
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
        /// <summary>
        /// Control file inside the repository 
        /// </summary>
        [DataMember]
        [Category("File Details")]
        [Browsable(false)]
        public string ControlFile { get; set; } = string.Empty;

        private IControlIdentity controlDetails;
        [Browsable(false)]
        public IControlIdentity ControlDetails
        {
            get
            {
                if (controlDetails == null)
                {
                    controlDetails = GetControlDetails();
                }
                return controlDetails;
            }
        }

        [DataMember]
        [DisplayName("Search Root")]
        [Category("Control Details")]
        [Browsable(true)]
        public Argument SearchRoot { get; set; } = new InArgument<UIControl>()
        {
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

        [DataMember]
        [Browsable(false)]
        [Display(Name = "Filter Script", GroupName = "Search Strategy", Order = 40)]
        [Description("When using FindAll LookupMode, provide a script to Filter the result")]
        public virtual Argument Filter { get; set; }


        protected abstract void InitializeFilter();

        public ControlEntity(string name = "Control Entity", string tag = "ControlEntity") : base(name, tag)
        {

        }

        protected IControlIdentity GetControlDetails()
        {
            if (File.Exists(this.ControlFile))
            {
                ISerializer serializer = this.EntityManager.GetServiceOfType<ISerializer>();
                var controlDescription = serializer.Deserialize<ControlDescription>(this.ControlFile);
                var controlDetails = controlDescription.ControlDetails;
                (controlDetails as Component).Parent = this;
                (controlDetails as Component).EntityManager = this.EntityManager;
                return controlDetails;
            }

            throw new FileNotFoundException($"Control file : {this.ControlFile} could not be found");

        }

        public override IEnumerable<IComponent> GetNextComponentToProcess()
        {
            foreach (var component in base.GetNextComponentToProcess())
            {
                yield return component;
            }
        }

        public abstract T GetTargetControl<T>();

        public abstract UIControl GetControl();

        public abstract IEnumerable<UIControl> GetAllControls();

        public override bool ValidateComponent()
        {
            base.ValidateComponent();
            if (this.ControlDetails.ControlType.Equals(Core.Enums.ControlType.Relative) && !(this.Parent is ControlEntity))
            {
                IsValid = false;
            }
            return IsValid;
        }

        protected T GetElementAtIndex<T>(IEnumerable<T> foundControls)
        {
            int index = ArgumentProcessor.GetValue<int>(this.Index);
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

            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
            var fn = await scriptEngine.CreateDelegateAsync<Func<IComponent, T, bool>>(predicateScriptFile);
            bool isMatch = fn(this, targetElement);
            return isMatch;
        }

    }
}
