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

/// <summary>
/// View model used to create a new group for test data source
/// </summary>
public class NewDataSourceGroupViewModel : SmartScreen
{
    private readonly ILogger logger = Log.ForContext<NewDataSourceGroupViewModel>();

    private readonly IReferenceManager referenceManager;
    private readonly INotificationManager notificationManager;

    private string groupName;
    /// <summary>
    /// Name of the group to be created
    /// </summary>
    public string GroupName
    {
        get => groupName;
        set
        {
            groupName = value;
            ValidateGroupName(value);
            NotifyOfPropertyChange(() => GroupName);
            NotifyOfPropertyChange(() => CanCreateGroup);
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="referenceManager"></param>
    public NewDataSourceGroupViewModel(IReferenceManager referenceManager, INotificationManager notificationManager)
    {
        this.DisplayName = "Create New Test Data Source Group";
        this.referenceManager = Guard.Argument(referenceManager).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager).NotNull().Value;
    }

    /// <summary>
    /// Create new group and close the view with true result
    /// </summary>
    /// <returns></returns>
    public async Task CreateNewGroup()
    {
        if (this.CanCreateGroup)
        {
            try
            {
                await this.referenceManager.AddTestDataSourceGroupAsync(this.GroupName);
                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while create data source group : '{0}'", this.GroupName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// True if the group caan be created
    /// </summary>
    public bool CanCreateGroup
    {
        get
        {
            return !this.HasErrors && !string.IsNullOrEmpty(this.GroupName) && IsNameAvailable(this.GroupName);
        }
    }

    private void ValidateGroupName(string groupName)
    {
        ClearErrors(nameof(GroupName));
        ValidateRequiredProperty(nameof(GroupName), groupName);
        ValidatePattern("[A-Za-z]{3,}", nameof(GroupName), groupName, "Name must contain only alphabets and should be atleast 3 characters in length.");
        if (!IsNameAvailable(groupName))
        {
            this.AddOrAppendErrors(nameof(GroupName), $"Group already exists with name {GroupName}");
        }
    }

    private bool IsNameAvailable(string groupName)
    {
        return !this.referenceManager.GetTestDataSourceGroups().Any(a => a.Equals(groupName));
    }

    /// <summary>
    /// Close the view with false result
    /// </summary>
    public async void Cancel()
    {
        await this.TryCloseAsync(false);
    }
}
