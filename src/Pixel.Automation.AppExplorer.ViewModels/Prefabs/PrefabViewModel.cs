using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefabs;

/// <summary>
/// View Model for <see cref="TestCase"/>
/// </summary>
public class PrefabViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// Underlying Model for the View
    /// </summary>
    public Prefab Prefab { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="testCase"></param>
    public PrefabViewModel(Prefab prefab)
    {
        this.Prefab = prefab;
    }

    /// <summary>
    /// Identifier of the TestCase
    /// </summary>
    public string PrefabId
    {
        get => Prefab.PrefabId;
    }

    /// <summary>
    /// Identifier of the TestCase
    /// </summary>
    public string ApplicationId
    {
        get => Prefab.ApplicationId;        
    }

    /// <summary>
    /// DisplayName for the TestCase
    /// </summary>
    public string DisplayName
    {
        get => Prefab.DisplayName;
        set
        {
            Prefab.DisplayName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Version of the control
    /// </summary>
    public Version Version
    {
        get => this.Prefab.Version;
    }

    /// <summary>
    /// Group assigned to control
    /// </summary>
    public string GroupName
    {
        get => this.Prefab.GroupName;
        set
        {
            this.Prefab.GroupName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Description for the TestCase
    /// </summary>
    public string Description
    {
        get => Prefab.Description;
        set => Prefab.Description = value;
    }

    /// <summary>
    /// TestCaseEntity
    /// </summary>
    public Entity WorkflowEntity

    {
        get => Prefab.WorkFlowEntity;
        set => Prefab.WorkFlowEntity = value;
    }

    /// <summary>
    /// Initialization ScriptFile for the TestCase
    /// </summary>
    public string ScriptFile
    {
        get => Prefab.ScriptFile;
        set => Prefab.ScriptFile = value;
    }

    bool isSelected;
    /// <summary>
    /// Indicates if the TestCase is currently selected in TestExplorer
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            OnPropertyChanged();
        }
    }

    bool isOpenForEdit;
    /// <summary>
    /// Indicates if the TestCase is open for edit
    /// </summary>
    public bool IsOpenForEdit
    {
        get => isOpenForEdit;
        set
        {
            isOpenForEdit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanOpenForEdit));
        }
    }

    /// <summary>
    /// Guard method for determining whether the TestCase can be opened for edit
    /// </summary>
    public bool CanOpenForEdit
    {
        get => !isOpenForEdit;
    }

    bool isVisible = true;
    /// <summary>
    /// Indicates whether the TestCase is visible in TestExplorer.
    /// This is controlled by the filter applied on TestExplorer
    /// </summary>
    public bool IsVisible
    {
        get => isVisible;
        private set
        {
            isVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates if there is a pending change
    /// </summary>
    public bool IsDirty { get; set; } = false;

    /// <summary>
    /// Replace the underlying Prefab model with a new instance.
    /// This is required when a new copy of Prefab might be available but we want to reuse the view model.
    /// </summary>
    /// <param name="prefab"></param>
    public void WithPrefab(Prefab prefab)
    {
        Guard.Argument(prefab, nameof(TestCase)).NotNull();
        this.Prefab = prefab;
    }

    /// <summary>
    /// Associate usage of control to the test case by storing the identifier of control
    /// </summary>
    /// <param name="prefabId"></param>
    public bool AddControlUsage(string controlId)
    {
        bool wasUsagedAddedd = false;
        if (this.Prefab.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
        {
            var entry = this.Prefab.ControlsUsed.First(a => a.ControlId.Equals(controlId));
            entry.Count++;
        }
        else
        {
            this.Prefab.ControlsUsed.Add(new ControlUsage() { ControlId = controlId, Count = 1 });
            wasUsagedAddedd = true;
        }
        this.IsDirty = true;
        return wasUsagedAddedd;
    }

    /// <summary>
    /// Remove usage of control from the test case
    /// </summary>
    /// <param name="controlId"></param>
    public bool RemoveControlUsage(string controlId)
    {
        bool wasUsagedRemoved = false;
        if (this.Prefab.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
        {
            var entry = this.Prefab.ControlsUsed.First(a => a.ControlId.Equals(controlId));
            if (entry.Count > 1)
            {
                entry.Count--;
            }
            else
            {
                this.Prefab.ControlsUsed.Remove(entry);
                wasUsagedRemoved = true;
            }
            this.IsDirty = true;
        }
        return wasUsagedRemoved;
    }

    /// <summary>
    /// Determine whether this TestCase should be visible on the TestExplorer
    /// </summary>
    /// <param name="filterText"></param>
    public void UpdateVisibility(string filterText)
    {
        if (string.IsNullOrEmpty(filterText))
        {
            IsVisible = true;
            return;
        }

        string[] query = filterText.Split(new char[] { ':' });
        foreach (var part in query)
        {
            if (string.IsNullOrEmpty(part))
            {
                IsVisible = true;
                return;
            }
        }
        switch (query.Length)
        {
            case 1:
                IsVisible = this.DisplayName.ToLower().Contains(filterText.ToLower());
                break;
            case 2: // we have a key value pair query
                switch (query[0].ToLower())
                {
                    case "name":
                        IsVisible = this.DisplayName.ToLower().Contains(query[1]);
                        break;
                    case "control":
                        IsVisible = this.Prefab.ControlsUsed.Any(p => p.ControlId.Equals(query[1]));
                        break;
                    default:
                        IsVisible = false;
                        break;
                }
                break;
        }
    }
}

