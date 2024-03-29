﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Loops;

[DataContract]
[Serializable]
[ToolBoxItem("For..Each Loop", "Loops", iconSource: null, description: "Contains a group of automation entity that will be prcossed in a for each loop", tags: new string[] { "foreach loop" })]
[NoDropTarget]
public class ForEachLoopEntity : LoopEntity , IScopedEntity
{
    private readonly ILogger logger = Log.ForContext<ForEachLoopEntity>();

    private Argument targetCollection = new InArgument<IEnumerable<object>>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted , Mode = ArgumentMode.DataBound, CanChangeType = true };
    [DataMember(Order = 200)]
    [Description("Target collection which needs to be looped over all its items")]
    [Display(Name = "Collection", GroupName = "Input", Order = 20)]
    public Argument TargetCollection
    {
        get => this.targetCollection;
        set
        {
            this.targetCollection = value;
            OnPropertyChanged();
        }
    }

    private Argument current = new OutArgument<object>() { Mode = ArgumentMode.DataBound, CanChangeType = true };
    [DataMember(Order = 210)]
    [Description("Current object being iterated")]
    [Display(Name = "Item", GroupName = "Input", Order = 10)]
    public Argument Current
    {
        get => this.current;
        set
        {
            this.current = value;
            OnPropertyChanged();
        }
    }


    public ForEachLoopEntity() : base("For..Each Loop", "ForEachLoopEntity")
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Interfaces.IComponent> GetNextComponentToProcess()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;       
        var targetEnumerable = this.TargetCollection.GetValue(argumentProcessor).Result;
        logger.Information(": Begin foreach loop");         
        int index = 0;
        foreach(var item in (IEnumerable)targetEnumerable)
        {
            if (!this.exitCriteriaSatisfied)
            {
                logger.Information("Running iteration : {0}", index);
              
                _ = this.Current.SetValue(argumentProcessor, item);

                var placeHolderEntity = this.GetFirstComponentOfType<PlaceHolderEntity>();
                var iterator = placeHolderEntity.GetNextComponentToProcess().GetEnumerator();
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }

                this.ResetDescendants();
            }
            index++;
        }
        logger.Information(": End foreach loop");
        this.ExitCriteriaSatisfied = true;
    }

    public override void ResolveDependencies()
    {
        if (this.Components.Count() > 0)
        {
            return;
        }

        PlaceHolderEntity statementsPlaceHolder = new PlaceHolderEntity("Statements");
        base.AddComponent(statementsPlaceHolder);

    }

    public override Entity AddComponent(Interfaces.IComponent component)
    {
        return this;
    }


    #region IScopedEntity

    private Dictionary<string, IEnumerable<string>> argumentPropertiesInfo = new Dictionary<string, IEnumerable<string>>();

    private Type localScopeType = typeof(object);

    public IEnumerable<string> GetPropertiesOfType(Type propertyType)
    {

        if (this.Current.GetArgumentType() != localScopeType)
        {
            localScopeType = this.Current.GetArgumentType();
            UpdateArgumentPropertiesInfo();
        }

        if (this.argumentPropertiesInfo.ContainsKey(propertyType.GetDisplayName()))
        {
            return this.argumentPropertiesInfo[propertyType.GetDisplayName()] ?? Enumerable.Empty<string>();
        }
        return Enumerable.Empty<string>();


        void UpdateArgumentPropertiesInfo()
        {
            this.argumentPropertiesInfo.Clear();

            if (this.Current.GetArgumentType() != null)
            {
                var propertiesGroupedByType = this.Current.GetArgumentType().GetProperties().GroupBy(p => p.PropertyType);
                foreach (var propertyGroup in propertiesGroupedByType)
                {
                    this.argumentPropertiesInfo.Add(propertyGroup.Key.GetDisplayName(), propertyGroup.Select(p => p.Name));
                }
            }
        }
    }

    public object GetScopedTypeInstance()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;         
        return this.Current.GetValue(argumentProcessor);
    }

    public string GetScopedArgumentName()
    {
        return Current?.PropertyPath;
    }

    #endregion IScopedEntity
}
