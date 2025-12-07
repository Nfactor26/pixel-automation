using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components;

[DataContract]
[Serializable]
public abstract class ControlEntity : Entity, IControlEntity
{
    [DataMember(Order = 500)]
    [Browsable(false)]
    public string ApplicationId { get; set; }

    [DataMember(Order = 510)]
    [Browsable(false)]
    public string ControlId { get; set; }

    [DataMember(Order = 210)]
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
    [DataMember(Order = 220)]
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
    [DataMember(Order = 230)]
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

    [DataMember(Order = 240)]
    [Display(Name = "Index", GroupName = "Search Strategy", Order = 40)]
    [Description("Bind to current Iteration when used inside loop")]
    public Argument Index { get; set; } = new InArgument<int>() { DefaultValue = 0, CanChangeType = false, Mode = ArgumentMode.Default };

    private Argument filter;
    [DataMember(Order = 250)]
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

    private Argument configureScript = new FuncArgument<Func<IControlIdentity, Task>>()
    {
        CanChangeType = false,
        AllowedModes = ArgumentMode.Scripted,
        Mode = ArgumentMode.Scripted
    };

    [DataMember(Order = 255)]
    [Display(Name = "Configure Script", GroupName = "Customize", Order = 45)]
    [Description("Script that executes before look happens. Example usage : Use it to modify control identity details")]
    public virtual Argument ConfigureScript
    {
        get => configureScript;
        set
        {   
            configureScript = value;
            OnPropertyChanged();
        }
    }

    [DataMember(Order = 260)]
    [Display(Name = "Enable Caching", GroupName = "Caching", Order = 50,
        Description = "Subsequent requests for target control from child components will return cached value if caching is enabled")]
    public bool CacheControl { get; set; } = false;

    private ControlDescription controlDescription;
    [Browsable(false)]
    public ControlDescription ControlDescription
    {
        get
        {
            if (controlDescription is null)
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

    /// <summary>
    /// Initialize the Filter Argument.
    /// </summary>
    protected virtual void InitializeFilter()
    {
        if (this.Filter == null)
        {
            this.Filter = new FuncArgument<Func<IComponent, UIControl, Task<bool>>>() { CanChangeType = false };
        }
    }

    public ControlEntity(string name = "Control Entity", string tag = "ControlEntity") : base(name, tag)
    {

    }
    
    private IControlLoader controlLoader;

    protected virtual ControlDescription LoadControlDescription()
    {
        if (this.controlLoader == null)
        {
            this.controlLoader = this.EntityManager.GetServiceOfType<IControlLoader>();
        }
        return this.controlLoader.GetControl(this.ApplicationId, this.ControlId);
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

    protected T GetElementMatchingCriteria<T>(IEnumerable<T> foundControls) where T : UIControl
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

    protected async Task<bool> ApplyPredicate<T>(string predicateScriptFile, T targetElement) where T : UIControl
    {
        IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
        var fn = await scriptEngine.CreateDelegateAsync<Func<IComponent, UIControl, Task<bool>>>(predicateScriptFile);
        bool isMatch = await fn(this, targetElement);
        return isMatch;
    }

    /// <summary>
    /// This method modifies the ControlDetails by executing the ConfigureScript if configured. Use the returned ControlDetails for further processing.
    /// Trying to get ControlDetails directly will give the original details without any modification.
    /// </summary>
    /// <returns></returns>
    protected async Task ExecuteConfigureScriptAsync()
    {
        if (this.ConfigureScript.IsConfigured())
        {
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var fn = await scriptEngine.CreateDelegateAsync<Func<IControlIdentity, Task>>(this.ConfigureScript.ScriptFile);           
            await fn(this.ControlDetails);          
        }     
    }

    public override Task BeforeProcessAsync()
    {
       this.controlDescription = LoadControlDescription();
       return base.BeforeProcessAsync();
    }

    public override Task OnCompletionAsync()
    {
        this.controlDescription = null;
        return base.OnCompletionAsync();
    }
}
