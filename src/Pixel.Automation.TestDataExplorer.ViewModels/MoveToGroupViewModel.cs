using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestDataExplorer.ViewModels;

/// <summary>
/// Allow users to pick a new group name for data source
/// </summary>
public class MoveToGroupViewModel : SmartScreen
{
    private readonly ILogger logger = Log.ForContext<MoveToGroupViewModel>();

    private readonly IReferenceManager referenceManager;
    private readonly INotificationManager notificationManager;
    private readonly TestDataSource dataSource;
    private readonly string currentGroup;

    /// <summary>
    /// Available screen names
    /// </summary>
    public BindableCollection<string> Groups { get; private set; } = new();
      
    private string selectedGroup;
    /// <summary>
    /// Selected group name
    /// </summary>
    public string SelectedGroup
    {
        get => this.selectedGroup;
        set
        {
            this.selectedGroup = value;
            ValidateGroupName(selectedGroup);
            NotifyOfPropertyChange(nameof(SelectedGroup));
            NotifyOfPropertyChange(nameof(CanMoveToGroup));
        }
    }

    /// <summary>
    /// Name of the test data source which needs to be moved
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Check if the data souce can be moved to selected group
    /// </summary>
    public bool CanMoveToGroup
    {
        get
        {
            return !this.HasErrors && !string.IsNullOrEmpty(this.SelectedGroup);
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="groups"></param>
    /// <param name="currentGroup"></param>
    public MoveToGroupViewModel(TestDataSource testDataSource, IEnumerable<string> groups, string currentGroup,
        IReferenceManager referenceManager, INotificationManager notificationManaager)
    {
        this.DisplayName = "Move To Screen";
        Guard.Argument(groups, nameof(groups)).NotNull();       
        this.dataSource = Guard.Argument(testDataSource, nameof(testDataSource)).NotNull();
        this.currentGroup = Guard.Argument(currentGroup, nameof(currentGroup)).NotNull().NotWhiteSpace();      
        this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManaager, nameof(notificationManaager)).NotNull().Value;
        this.Groups.AddRange(groups.Except(new[] { currentGroup }));
        this.SelectedGroup = this.Groups.FirstOrDefault();
        this.Name = this.dataSource.Name;
    }

    /// <summary>
    /// Check if new group name is valid
    /// </summary>
    /// <param name="groupName"></param>
    private void ValidateGroupName(string groupName)
    {
        ClearErrors(nameof(SelectedGroup));
        ValidateRequiredProperty(nameof(SelectedGroup), groupName);
    }

    /// <summary>
    /// Close the dialog with true result
    /// </summary>
    /// <returns></returns>
    public async Task MoveToGroup()
    {
        try
        {
            if (this.CanMoveToGroup)
            {
                await this.referenceManager.MoveTestDataSourceToGroupAsync(this.dataSource.DataSourceId, this.currentGroup, this.SelectedGroup);
                await this.TryCloseAsync(true);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error while moving data source : '{0}' to another group", this.dataSource.Name);         
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    /// <summary>
    /// Close the dialog with false result
    /// </summary>
    public async void Cancel()
    {
        await this.TryCloseAsync(false);
    }
}
