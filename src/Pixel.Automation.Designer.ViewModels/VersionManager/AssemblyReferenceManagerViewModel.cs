using Dawn;
using ICSharpCode.AvalonEdit;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Reference.Manager.Models;
using Serilog;
using System.IO;
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
    public AssemblyReferenceManagerViewModel(IFileSystem fileSystem, ISerializer serializer)
    {
        this.DisplayName = "Manager Assembly References";
        this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
        this.JsonEditor.Text = this.fileSystem.ReadAllText(fileSystem.AssemblyReferencesFile);
    }

    /// <summary>
    /// Save all the changes done on screen.
    /// </summary>
    public async void SaveAsync()
    {
        string jsonText = this.JsonEditor.Text;
        if(!string.IsNullOrEmpty(jsonText))
        {
            try
            {
                ClearErrors("");
                var referenceCollection = this.serializer.DeserializeContent<ReferenceCollection>(jsonText);
                this.fileSystem.SaveToFile<ReferenceCollection>(referenceCollection, Path.GetDirectoryName(fileSystem.AssemblyReferencesFile), Path.GetFileName(fileSystem.AssemblyReferencesFile));
                logger.Information("{0} was updated", fileSystem.AssemblyReferencesFile);
                await this.TryCloseAsync(true);
            }
            catch(Exception ex)
            {
                AddOrAppendErrors("", ex.Message);
                logger.Error(ex, ex.Message);
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
