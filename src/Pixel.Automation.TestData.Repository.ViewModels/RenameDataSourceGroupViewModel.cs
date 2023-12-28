using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels;

public class RenameDataSourceGroupViewModel : SmartScreen
{
    private readonly ILogger logger = Log.ForContext<RenameDataSourceGroupViewModel>();

    private readonly IReferenceManager referenceManager;
    private readonly INotificationManager notificationManager;

    /// <summary>
    /// Current name of the group
    /// </summary>
    public string GroupName { get; private set; }

    private string newGroupName;
    /// <summary>
    /// New name of the group
    /// </summary>
    public string NewGroupName
    {
        get => newGroupName;
        set
        {
            newGroupName = value;
            ValidateGroupName(value);
            NotifyOfPropertyChange(() => NewGroupName);
            NotifyOfPropertyChange(() => CanRenameGroup);
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="referenceManager"></param>
    /// <param name="groupName"></param>
    public RenameDataSourceGroupViewModel(string groupName, IReferenceManager referenceManager, INotificationManager notificationManager)
    {
        this.DisplayName = "Rename Group";
        this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
        this.GroupName = Guard.Argument(groupName, nameof(groupName)).NotNull().NotEmpty();
        this.notificationManager = Guard.Argument(notificationManager).NotNull().Value;
    }

    /// <summary>
    /// Rename group to a new value and close the view with true result
    /// </summary>
    /// <returns></returns>
    public async Task RenameGroup()
    {
        if (this.CanRenameGroup)
        {
            try
            {
                await this.referenceManager.RenameTestDataSourceGroupAsync(this.GroupName, this.NewGroupName);
                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while renaming data source group : '{0}'", this.GroupName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Check if it is possible to rename group
    /// </summary>
    public bool CanRenameGroup
    {
        get
        {
            return !this.HasErrors && !string.IsNullOrEmpty(this.NewGroupName) && IsNameAvailable(this.NewGroupName);
        }
    }

    /// <summary>
    /// Validate screen name
    /// </summary>
    /// <param name="newGroupName"></param>
    private void ValidateGroupName(string newGroupName)
    {
        ClearErrors(nameof(NewGroupName));
        ValidateRequiredProperty(nameof(NewGroupName), newGroupName);
        ValidatePattern("[A-Za-z]{3,}", nameof(NewGroupName), newGroupName, "Name must contain only alphabets and should be atleast 3 characters in length.");
        if (!IsNameAvailable(newGroupName))
        {
            this.AddOrAppendErrors(nameof(NewGroupName), $"Group already exists with name {NewGroupName}");
        }
    }

    /// <summary>
    /// Check if new screen name is available
    /// </summary>
    /// <param name="screenName"></param>
    /// <returns></returns>
    private bool IsNameAvailable(string screenName)
    {
        return !this.referenceManager.GetTestDataSourceGroups().Any(a => a.Equals(screenName));
    }

    /// <summary>
    /// Close the view with false result
    /// </summary>
    public async void Cancel()
    {
        await this.TryCloseAsync(false);
    }
}
