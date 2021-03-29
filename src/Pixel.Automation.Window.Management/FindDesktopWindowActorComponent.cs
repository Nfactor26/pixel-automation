﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Find Desktop Window", "Window Managment", iconSource: null, description: "Find desktop window", tags: new string[] { "Find desktop window" })]
    public class FindDesktopWindowsActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Window Title")]
        [Category("Input")]       
        public Argument Title { get; set; } = new InArgument<string>();

        [DataMember]
        [DisplayName("Match Criteria")]
        [Category("Input")]       
        public MatchType MatchType { get; set; } = MatchType.Equals;

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
                        this.Filter = new PredicateArgument<ApplicationWindow>() { CanChangeMode = false, CanChangeType = false };
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
        [Display(Name = "Filter Script", GroupName = "Search Strategy", Order = 40)]
        [Description("When using FindAll LookupMode, provide a script to Filter the result")]
        public virtual Argument Filter { get; set; }


        [DataMember]
        [DisplayName("Target Window")]
        [Description("Window matching the configured search critieria")]
        [Category("Output")]
        public Argument TargetWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        public FindDesktopWindowsActorComponent() : base("Find Desktop Windows", " FindDesktopWindows")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            string titleToMatch = argumentProcessor.GetValue<string>(this.Title) ?? string.Empty;
            var foundWindows = windowManager.FindAllDesktopWindows(titleToMatch, this.MatchType, true);

            if (this.lookupMode == LookupMode.FindSingle)
            {
                argumentProcessor.SetValue<ApplicationWindow>(this.TargetWindow, foundWindows.Single());
            }
            else
            {
                switch (this.filterMode)
                {
                    case FilterMode.Index:
                        int index = argumentProcessor.GetValue<int>(this.Index);
                        if (foundWindows.Count() > index)
                        {
                            var foundWindow = foundWindows.ElementAt(index);
                            argumentProcessor.SetValue<ApplicationWindow>(this.TargetWindow, foundWindow);
                        }
                        break;
                    case FilterMode.Custom:                      
                        foreach (var window in foundWindows)
                        {
                            var found = ApplyPredicate(this.Filter.ScriptFile, window).Result;
                            if (found)
                            {
                                argumentProcessor.SetValue<ApplicationWindow>(this.TargetWindow, window);
                                break;
                            }

                        }
                        break;
                }
            }
        }


        protected async Task<bool> ApplyPredicate(string predicateScriptFile, ApplicationWindow applicationWindow)
        {
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var fn = await scriptEngine.CreateDelegateAsync<Func<Core.Interfaces.IComponent, ApplicationWindow, bool>>(predicateScriptFile);

            bool isMatch = fn(this, applicationWindow);
            return isMatch;            
        }
    }
}