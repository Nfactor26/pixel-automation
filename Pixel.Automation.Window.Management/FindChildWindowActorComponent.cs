using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Find Child Window", "Window Managment", iconSource: null, description: "Find child window", tags: new string[] { "Find child window" })]
    public class FindChildWindowActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Window Title")]
        [Category("Input")]          
        public InArgument<string> Title { get; set; } = new InArgument<string>();

        [DataMember]
        [DisplayName("Match Criteria")]
        [Category("Input")]       
        public MatchType MatchType { get; set; } = MatchType.Equals;

        [DataMember]
        [DisplayName("Parent Window")]
        [Description("Parent Window whose child window needs to be located")]
        [Category("Input")]    
        public InArgument<ApplicationWindow> ParentWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        private LookupMode lookupMode = LookupMode.FindSingle;
        [DataMember]
        [Category("Search Mode")]
        [Description("Find single control or Find All control and apply index based or custom filter")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public LookupMode LookupMode
        {
            get
            {
                switch (lookupMode)
                {
                    case LookupMode.FindSingle:
                        this.SetBrowsableAttribute(nameof(FilterMode), false);
                        this.SetBrowsableAttribute(nameof(Index), false);
                        this.SetBrowsableAttribute(nameof(Filter), false);                   
                        break;
                    case LookupMode.FindAll:
                        this.SetBrowsableAttribute(nameof(FilterMode), true);                    
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
        [Browsable(false)]
        [DataMember]
        [Category("Search Mode")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public FilterMode FilterMode
        {
            get
            {
                switch (filterMode)
                {
                    case FilterMode.Index:
                        this.SetBrowsableAttribute(nameof(Filter), false);
                        this.SetBrowsableAttribute(nameof(Index), true);
                        break;
                    case FilterMode.Custom:
                        this.Filter = new PredicateArgument<ApplicationWindow>() { CanChangeMode = false, CanChangeType = false };
                        this.SetBrowsableAttribute(nameof(Filter), true);
                        this.SetBrowsableAttribute(nameof(Index), false);
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
        [Browsable(false)]
        [Category("Search Mode")]
        [Description("Bind to current Iteration when used inside loop")]
        public Argument Index { get; set; } = new InArgument<int>() { DefaultValue = 0, CanChangeType = false, Mode = ArgumentMode.Default };

        [DataMember]
        [Browsable(false)]
        [Category("Search Mode")]
        [Description("When using FindAll LookupMode, provide a script to Filter the result")]
        public virtual Argument Filter { get; set; }


        [DataMember]
        [DisplayName("Target Window")]
        [Description("Window matching the configured search critieria")]
        [Category("Output")]
        public Argument TargetWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };
      

        public FindChildWindowActorComponent() : base("Find Child Windows", " FindChildWindows")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
          
            string titleToMatch = argumentProcessor.GetValue<string>(this.Title) ?? string.Empty;
            ApplicationWindow parent = argumentProcessor.GetValue<ApplicationWindow>(this.ParentWindow);         
          
            var foundWindows = windowManager.FindAllChildWindows(parent, titleToMatch, this.MatchType, true);

            if(this.lookupMode == LookupMode.FindSingle)
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
                        bool found = false;
                        foreach (var window in foundWindows)
                        {
                            found = (bool)ApplyPredicate(this.Filter.ScriptFile, window)
                                .Result.ReturnValue;
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

        protected async Task<ScriptResult> ApplyPredicate(string predicateScriptFile, ApplicationWindow applicationWindow)
        {
            Type scriptDataType = typeof(PredicateScriptArgument<,>).MakeGenericType(this.EntityManager.Arguments.GetType(), typeof(ApplicationWindow));
            var scriptData = Activator.CreateInstance(scriptDataType, new[] { this.EntityManager.Arguments, applicationWindow });

            IScriptEngine scriptExecutor = this.EntityManager.GetServiceOfType<IScriptEngine>();
            ScriptResult result = await scriptExecutor.ExecuteFileAsync(predicateScriptFile, scriptData, null);
            result = await scriptExecutor.ExecuteScriptAsync("IsMatch(DataModel,Control)", scriptData, result.CurrentState);
            return result;
        }
    }
}
