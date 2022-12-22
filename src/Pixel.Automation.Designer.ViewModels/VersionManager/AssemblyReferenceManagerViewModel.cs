using Dawn;
using ICSharpCode.AvalonEdit;
using Notifications.Wpf.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels.VersionManager;

/// <summary>
/// AssemblyReferenceManager is used to manage the assembly references and imports used for code editor, script editor and script engine.
/// </summary>
public class AssemblyReferenceManagerViewModel : SmartScreen, IVersionManager
{
    private readonly ILogger logger = Log.ForContext<AssemblyReferenceManagerViewModel>();
    private readonly IFileSystem fileSystem;
    private readonly ISerializer serializer;
    private readonly IReferenceManager referenceManager;
    private readonly INotificationManager notificationManager;

    public TextEditor JsonEditor { get; set; } = new TextEditor()
    {
        ShowLineNumbers = true,
        Margin = new Thickness(5),
        FontSize = 16,
        FontFamily = new ("Consolas"),
        HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
        VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
        Options = new TextEditorOptions
        {
            HighlightCurrentLine = true
        }
    };

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <param name="serializer"></param>
    public AssemblyReferenceManagerViewModel(IFileSystem fileSystem, ISerializer serializer,
        IReferenceManager referenceManager, INotificationManager notificationManager)
    {
        this.DisplayName = "Manager Assembly References";
        this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
        this.JsonEditor.Text = this.serializer.Serialize(this.referenceManager.GetEditorReferences());
    }

    /// <summary>
    /// Save all the changes done on screen.
    /// </summary>
    public async Task SaveAsync()
    {
        string jsonText = this.JsonEditor.Text;
        if(!string.IsNullOrEmpty(jsonText))
        {
            try
            {
                ClearErrors("");
                var editorReferences = this.serializer.DeserializeContent<EditorReferences>(jsonText);
                await this.referenceManager.SetEditorReferencesAsync(editorReferences);              
                logger.Information("{0} was updated", fileSystem.ReferencesFile);
                await this.TryCloseAsync(true);
            }
            catch(Exception ex)
            {
                AddOrAppendErrors("", ex.Message);
                logger.Error(ex, "There was an error while trying to save changes to assembly references");
                await notificationManager.ShowErrorNotificationAsync(ex.Message);
            }
        }        
    }

    /// <summary>
    /// Close the screen without saving changes.
    /// </summary>
    /// <returns></returns>
    public async Task CloseAsync()
    {
        await this.TryCloseAsync(false);
    }

}
