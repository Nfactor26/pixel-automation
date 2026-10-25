using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefabs;


public class PrefabExplorerViewModel : Screen, IApplicationAware,
    IHandle<OpenPrefabRequestNotification>, IHandle<ClosePrefabRequestNotification>, IHandle<SavePrefabRequestNotification>,
    IHandle<ControlAddedEventArgs>
{
    #region Data members

    private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
    
    private readonly IApplicationDataManager applicationDataManager;   
    private readonly IEventAggregator eventAggregator;
    private readonly IWindowManager windowManager;
    private readonly INotificationManager notificationManager;   
    private readonly IPrefabDataManager prefabManager;
    private readonly ICodeEditorFactory codeEditorFactory;
    private readonly IReferenceManagerFactory referenceManagerFactory;
    private readonly IReferenceProviderFactory referenceProviderFactory;
    private readonly IVersionManagerFactory versionManagerFactory;
    private readonly ApplicationSettings applicationSettings;

    private ApplicationDescriptionViewModel activeApplication;

    public ApplicationDescriptionViewModel ActiveApplication
    {
        get => this.activeApplication;
        set
        {
            this.activeApplication = value;
            NotifyOfPropertyChange();
        }
    }

    public ApplicationScreenCollection ScreenCollection { get; private set; }

    /// <summary>
    /// A collection of TestFixtures belonging to the Automation Project
    /// </summary>
    public BindableCollection<PrefabViewModel> Prefabs { get; set; } = new BindableCollection<PrefabViewModel>();

    private PrefabViewModel selectedPrefab;
    /// <summary>
    /// Selected TestDataSource on the view
    /// </summary>
    public PrefabViewModel SelectedPrefab
    {
        get => this.selectedPrefab;
        set
        {
            this.selectedPrefab = value;
            NotifyOfPropertyChange(() => SelectedPrefab);
            this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value, true));
        }
    }   

    #endregion Data members

    #region Constructor

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="projectManager"></param>   
    /// <param name="entityManager"></param>
    /// <param name="windowManager"></param>
    /// <param name="applicationSettings"></param>
    public PrefabExplorerViewModel(IEventAggregator eventAggregator, IPrefabDataManager prefabManager, IApplicationDataManager applicationDataManager,
        ICodeEditorFactory codeEditorFactory, IReferenceManagerFactory referenceManagerFactory, IReferenceProviderFactory referenceProviderFactory,
        IVersionManagerFactory versionManagerFactory,
        IWindowManager windowManager, INotificationManager notificationManager, ApplicationSettings applicationSettings)
    {
        this.DisplayName = "Prefab Explorer";
        this.prefabManager = Guard.Argument(prefabManager, nameof(prefabManager)).NotNull().Value;
        this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        this.codeEditorFactory = Guard.Argument(codeEditorFactory, nameof(codeEditorFactory)).NotNull().Value;
        this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
        this.referenceProviderFactory = Guard.Argument(referenceProviderFactory, nameof(referenceProviderFactory)).NotNull().Value;
        this.versionManagerFactory = Guard.Argument(versionManagerFactory, nameof(versionManagerFactory)).NotNull().Value;
        this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
        this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings));
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;            
        this.eventAggregator.SubscribeOnPublishedThread(this);        

        CreateCollectionView();

        ConfigureCodeEditor();

    }

    void ConfigureCodeEditor()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ConfigureCodeEditor), ActivityKind.Internal))
        {
            logger.Debug($"Trying to configure code editor for Prefab Explorer");
            this.codeEditorFactory.Initialize(Environment.CurrentDirectory, applicationSettings.DefaultEditorReferences.Union(applicationSettings.DefaultCodeReferences), Enumerable.Empty<string>());
            logger.Debug($"Configure code editor for Prefab Explorer completed.");
        }
    }

    #endregion

    #region Filter

    /// <summary>
    /// Controls visibility of test fixtures and test cases displayed on view
    /// </summary>
    string filterText = string.Empty;
    public string FilterText
    {
        get
        {
            return filterText;
        }
        set
        {
            filterText = value;
            var prefabView = CollectionViewSource.GetDefaultView(Prefabs);
            prefabView.Refresh();
            NotifyOfPropertyChange(() => FilterText);
        }
    }


    /// <summary>
    /// Setup the collection view with grouping and sorting
    /// </summary>
    private void CreateCollectionView()
    {
        var prefabGroupedItems = CollectionViewSource.GetDefaultView(Prefabs);
        prefabGroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
        prefabGroupedItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
        prefabGroupedItems.Filter = new Predicate<object>((a) =>
        {
            if (a is PrefabViewModel prefab)
            {
                return prefab.DisplayName.ToLower().Contains(this.filterText.ToLower());
            }
            return true;
        });
    }

    #endregion

    #region Application Aware

    /// <summary>
    /// Set the application for which control collection should be loaded and displayed
    /// </summary>
    /// <param name="applicationDescriptionViewModel"></param>
    public void SetActiveApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
    {
        if (this.ScreenCollection != null)
        {
            this.ScreenCollection.ScreenChanged -= OnScreenChanged;
        }
        this.ActiveApplication = applicationDescriptionViewModel;
        this.ScreenCollection = applicationDescriptionViewModel?.ScreenCollection;
        this.Prefabs.Clear();
        if (this.ScreenCollection != null)
        {
            this.ScreenCollection.ScreenChanged += OnScreenChanged;
            OnScreenChanged(this, this.ScreenCollection.SelectedScreen);
        }
        NotifyOfPropertyChange(nameof(this.ScreenCollection));
    }

    private void OnScreenChanged(object sender, ApplicationScreen selectedScreen)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OnScreenChanged), ActivityKind.Internal))
        {
            try
            {
                this.Prefabs.Clear();
                activity?.SetTag("SelectedScreen", selectedScreen?.ScreenName ?? string.Empty);
                if (selectedScreen is not null)
                {
                    var prefabsForSelectedScreen = LoadPrefabDetails(this.ActiveApplication, selectedScreen);
                    this.Prefabs.AddRange(prefabsForSelectedScreen);
                }
                else
                {
                    foreach (var screen in this.activeApplication.Screens)
                    {
                        var prefabsForScreen = LoadPrefabDetails(this.ActiveApplication, screen);
                        this.Prefabs.AddRange(prefabsForScreen);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured while loading controls for screen : '{0}'", selectedScreen);
                _ = notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    private IEnumerable<PrefabViewModel> LoadPrefabDetails(ApplicationDescriptionViewModel applicationDescriptionViewModel, ApplicationScreen selectedScreen)
    {
        List<PrefabViewModel> prefabsList = new();
        if (applicationDescriptionViewModel.ContainsScreen(selectedScreen.ScreenName))
        {
            var prefabs = this.prefabManager.GetPrefabsForScreen(applicationDescriptionViewModel.Model, selectedScreen.ScreenName).ToList();
            foreach (var prefab in prefabs)
            {
                if (prefab.IsDeleted)
                {
                    continue;
                }
                var prefabViewModel = new PrefabViewModel(prefab);
                applicationDescriptionViewModel.AddPrefab(prefabViewModel, selectedScreen.ScreenName);
                prefabsList.Add(prefabViewModel);

                if(this.openForEdit.ContainsKey(prefab.PrefabId))
                {
                    prefabViewModel.IsOpenForEdit = true;
                }
            }

        }
        return prefabsList;
    }

    /// <summary>
    /// Move a Prefab from one screen to another
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public async Task MoveToScreen(PrefabViewModel prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(MoveToScreen), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("PrefabName", prefab.DisplayName);
                var moveToScreenViewModel = new MoveToScreenViewModel(prefab.DisplayName, this.ScreenCollection.Screens.Select(s => s.ScreenName), this.ScreenCollection.SelectedScreen.ScreenName);
                var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                if (result.GetValueOrDefault())
                {
                    await this.prefabManager.MovePrefabToScreen(prefab.Prefab, this.ActiveApplication[moveToScreenViewModel.SelectedScreen].ScreenId);
                    this.ActiveApplication.MovePrefabToScreen(prefab, this.ScreenCollection.SelectedScreen.ScreenName, moveToScreenViewModel.SelectedScreen);
                    this.Prefabs.Remove(prefab);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    logger.Information("Moved Prefab : {0} from screen {1} to {2} for application {3}", prefab.DisplayName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
                    await notificationManager.ShowSuccessNotificationAsync($"Prefab : '{prefab.DisplayName}' was moved to screen : '{moveToScreenViewModel.SelectedScreen}'");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while moving control : '{0}' to another screen", prefab?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    #endregion 

    #region Prefab   

    /// <summary>
    /// Add a new Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    public async Task AddPrefabAsync()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddPrefabAsync), ActivityKind.Internal))
        {
            try
            {              
                Prefab prefab = new Prefab()
                {
                    ApplicationId = this.ActiveApplication.ApplicationId                               
                };
                prefab.AvailableVersions.Add(new VersionInfo("1.0"));                               
              
                var prefabEditor = new EditPrefabViewModel(prefab, this.Prefabs.Select(s => s.DisplayName), windowManager);
                bool? result = await this.windowManager.ShowDialogAsync(prefabEditor);
                if (result.HasValue && result.Value)
                {                   
                    PrefabViewModel prefabVM = new PrefabViewModel(prefab);                   
                    await this.prefabManager.AddPrefabToScreenAsync(prefab, new PrefabEntity(), this.ActiveApplication.ScreenCollection.SelectedScreen.ScreenName);
                    this.Prefabs.Add(prefabVM);

                    this.ActiveApplication.AddPrefab(prefabVM, this.ScreenCollection.SelectedScreen.ScreenName);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);

                    activity?.SetTag("Prefab", prefab.DisplayName);
                    logger.Information("Created new Prefab {0}", prefab.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to add new prefab");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

    }

    /// <summary>
    /// Edit the details of a Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    public async Task EditPrefabAsync(PrefabViewModel prefabVM)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditPrefabAsync), ActivityKind.Internal))
        {
            try
            {
                Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
                activity?.SetTag("Prefab", prefabVM.DisplayName);
                await RefreshPrefabAsync(prefabVM);             
                var prefabEditor = new EditPrefabViewModel(prefabVM.Prefab, this.Prefabs.Except([prefabVM]).Select(s => s.DisplayName), windowManager);
                bool? result = await this.windowManager.ShowDialogAsync(prefabEditor);
                if (result.HasValue && result.Value)
                {                  
                    await this.prefabManager.UpdatePrefabAsync(prefabVM.Prefab);
                    prefabVM.Refresh();
                    prefabVM.IsDirty = false;
                    logger.Information("Prefab : '{0}' was edited.", prefabVM.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to edit Prefab : '{0}'", prefabVM?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Create a copy of Prefab
    /// </summary>
    /// <param name="controlToRename"></param>
    public async Task CreateRevisionAsync(PrefabViewModel prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateRevisionAsync), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("Prefab", prefab.DisplayName);

                var versionToCreate = new VersionInfo(new Version(prefab.Prefab.LatestActiveVersion.Version.Major + 1, 0));
                var updated = await this.prefabManager.AddPrefabVersionAsync(prefab.Prefab, versionToCreate, prefab.Prefab.LatestActiveVersion);               
                logger.Information("Created a new revision for Prefab : {0}", updated.DisplayName);
                await notificationManager.ShowSuccessNotificationAsync($"Create version '{updated.LatestActiveVersion}' of Prefab: '{updated.DisplayName}'");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while creating revision of Prefab : {0}", prefab?.DisplayName ?? "NULL");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Edit the initialization script for a Prefab
    /// </summary>
    /// <param name="fixtureVM"></param>
    /// <returns></returns>
    public async Task EditPrefabScriptAsync(PrefabViewModel prefabVM)
    {
        try
        {          
            if (prefabVM.IsOpenForEdit)
            {
                var openInstance = this.openForEdit[prefabVM.PrefabId];              
                var prefabEntityManager = openInstance.PrefabEntity.EntityManager;
                var fileSystem = this.prefabManager.GetPrefabFileSystem(prefabVM.Prefab, prefabVM.Prefab.LatestActiveVersion);
                IScriptEditorFactory scriptEditorFactory = prefabEntityManager.GetServiceOfType<IScriptEditorFactory>();
                using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                {
                    scriptEditorScreen.OpenDocument(fileSystem.ScriptFile, prefabVM.PrefabId, string.Empty);
                    var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                    if (result.HasValue && result.Value)
                    {
                        await this.prefabManager.AddOrUpdateDataFileAsync(openInstance.Prefab, openInstance.Version, fileSystem.ScriptFile, openInstance.PrefabId);
                        logger.Information("Script file : '{0}' was edited for Prefab : '{1}'.", fileSystem.ScriptFile, prefabVM.DisplayName);
                        
                        var prefabScriptEngine = prefabEntityManager.GetScriptEngine();
                        prefabScriptEngine.ClearState();
                        await prefabScriptEngine.ExecuteFileAsync(fileSystem.ScriptFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to edit script file for Prefab : '{0}'", prefabVM?.DisplayName);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    /// <summary>
    /// Edit the initialization script for a Prefab
    /// </summary>
    /// <param name="fixtureVM"></param>
    /// <returns></returns>
    public async Task EditPrefabDataModelScriptAsync(PrefabViewModel prefabVM)
    {
        try
        {
            if (!prefabVM.IsOpenForEdit)
            {
                //TODO : Ask user the version to edit or source it from open project
                var fileSystem = this.prefabManager.GetPrefabFileSystem(prefabVM.Prefab, prefabVM.Prefab.LatestActiveVersion); 
                codeEditorFactory.AddProject(prefabVM.PrefabId, $"{Constants.NamespacePrefix}.{prefabVM.DisplayName.Replace(' ', '.')}", []);
                using (ICodeEditorScreen codeEditorScreen = codeEditorFactory.CreateCodeEditor())
                {
                    codeEditorScreen.OpenDocument(fileSystem.DataModelFile, prefabVM.PrefabId, string.Empty);
                    var result = await this.windowManager.ShowDialogAsync(codeEditorScreen);
                    if (result.HasValue && result.Value)
                    {
                        await this.prefabManager.AddOrUpdateDataFileAsync(prefabVM.Prefab, prefabVM.Prefab.LatestActiveVersion, fileSystem.DataModelFile, prefabVM.PrefabId);
                        logger.Information("Data Model file : '{0}' was edited for Prefab : '{1}'.", fileSystem.DataModelFile, prefabVM.DisplayName);                       
                    }                   
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to edit Data Model file for Prefab : '{0}'", prefabVM?.DisplayName);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    /// <summary>
    /// Delete an existing Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    public async void DeletePrefab(PrefabViewModel prefabVM)
    {
        Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
        if (prefabVM.IsOpenForEdit)
        {
            MessageBox.Show("Prefab is open for edit. An open Prefab can't be deleted.", "Delete Prefab", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeletePrefab), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("Prefab", prefabVM.DisplayName);

                PrefabViewModel prefabToDelete = this.Prefabs.FirstOrDefault(c => c.PrefabId.Equals(prefabVM.PrefabId));
                if (prefabToDelete is not null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        await this.prefabManager.DeletePrefabAsync(prefabVM.Prefab);   
                        this.Prefabs.Remove(prefabToDelete);
                        logger.Information("Deleted Prefab : '{0}'", prefabVM.DisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to delete Test Case : '{0}'", prefabVM?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Open a Prefab to be edited on automation editor
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    async Task<PrefabInstance> OpenPrefabAsync(Prefab prefab, VersionInfo version, PrefabHostEntity prefabHost)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenPrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(prefab, nameof(prefab)).NotNull();
            activity?.SetTag("Prefab", prefab.DisplayName);
           
            await this.prefabManager.DownloadPrefabDataAsync(prefab, version);

            var fileSystem = this.prefabManager.GetPrefabFileSystem(prefab, version);
            var referenceProvider = this.referenceProviderFactory.CreateReferenceProvider(fileSystem);
            var workflowEntity = fileSystem.GetPrefabEntity();

            var referenceManager = this.referenceManagerFactory.CreateReferenceManager(prefab.PrefabId, version.ToString(), fileSystem);

            if (await TryOpenForEdit())
            {
                SetupScriptEditor();             
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }

            return new PrefabInstance()
            {
                Prefab = prefab,
                PrefabEntity = workflowEntity,
                Version = version,
                ReferenceManager = referenceManager
            };

            async Task<bool> TryOpenForEdit()
            {               
                EntityManager prefabEntityManager = new EntityManager(prefabHost.EntityManager);
                prefabEntityManager.SetCurrentFileSystem(fileSystem);
                prefabEntityManager.SetRefereceProvider(referenceProvider);
                prefabEntityManager.SetIdentifier($"Prefab - {prefab.DisplayName}");
                workflowEntity.EntityManager = prefabEntityManager;
                                
                var componentViewBuilder = prefabHost.EntityManager.GetServiceOfType<IComponentViewBuilder>();
                componentViewBuilder.OpenPrefab(prefab, workflowEntity);

                //This data model is only used at design time so that visibility is limited to members from this model
                var dataModelInstance = CompileDataModel();              
                prefabEntityManager.Arguments = dataModelInstance;
                //Copy the values from the arguments of testcase entity manager to prefab entity manager. This will allow to execute components of prefab at design time one at a time.
                //This will also throw an exception if types are not compatible
                TypeExtensions.CopyProperties(prefabHost.EntityManager.Arguments, prefabEntityManager.Arguments);

                var scriptEngine = prefabEntityManager.GetScriptEngine();
                await scriptEngine.ExecuteFileAsync(fileSystem.ScriptFile);

                return true;

            }

            void SetupScriptEditor()
            {
                var prefabEntityManager = workflowEntity.EntityManager;
                IScriptEditorFactory editorFactory = prefabEntityManager.GetServiceOfType<IScriptEditorFactory>();

                //Add the test script project and script file to script editor
                editorFactory.AddProject(prefab.PrefabId, new string[] { }, prefabEntityManager.Arguments.GetType());
                string scriptFileContent = File.ReadAllText(fileSystem.ScriptFile);
                editorFactory.AddDocument(fileSystem.ScriptFile, prefab.PrefabId, scriptFileContent);
            }


            object CompileDataModel()
            {
                string tempDirectory = fileSystem.TempDirectory;
                this.codeEditorFactory.AddProject(prefab.PrefabId, $"{Constants.NamespacePrefix}.{prefab.DisplayName.Replace(' ', '.')}", []);
                this.codeEditorFactory.AddDocument(Path.GetFileName(fileSystem.DataModelFile), prefab.PrefabId, File.ReadAllText(fileSystem.DataModelFile));
                using (var compilationResult = this.codeEditorFactory.CompileProject(prefab.PrefabId, GetNewDataModelAssemblyName()))
                {
                    compilationResult.SaveAssemblyToDisk(tempDirectory);
                    string assemblyLocation = Path.Combine(tempDirectory, compilationResult.OutputAssemblyName);
                    Assembly assembly = Assembly.LoadFrom(assemblyLocation);
                    logger.Information($"Data model assembly compiled and assembly loaded from {assemblyLocation}");
                    Type typeofDataModel = assembly.GetTypes().FirstOrDefault();
                    if (typeofDataModel != null)
                    {
                        return Activator.CreateInstance(typeofDataModel);
                    }
                    logger.Warning("No data model found for Prefab : {0}", prefab.DisplayName);
                    return new object();
                }
            }

            string GetNewDataModelAssemblyName()
            {
                string tempDirectory = fileSystem.TempDirectory;
                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                }

                foreach (var file in Directory.GetFiles(tempDirectory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {

                    }
                }

                int compilationIteration = 0;    
                       
                var assemblyFiles = Directory.GetFiles(tempDirectory, "*.dll").Select(f => new FileInfo(Path.Combine(tempDirectory, f)));
                if (assemblyFiles.Any())
                {
                    var mostRecentAssembly = assemblyFiles.OrderBy(a => a.CreationTime).Last();
                    string assemblyName = Path.GetFileNameWithoutExtension(mostRecentAssembly.Name);
                    if (int.TryParse(assemblyName.Split('_', StringSplitOptions.RemoveEmptyEntries).Last(), out int lastIteration))
                    {
                        compilationIteration = lastIteration;
                    }
                }

                //There are some scenarios where the data model assembly with same name is already loaded in domain
                compilationIteration++;
                string dataModelAssemblyName = $"{Constants.NamespacePrefix}.{prefab.DisplayName.Replace(' ', '.')}";
                string dataModelAssemblyNameWithIteration = $"{dataModelAssemblyName}_{compilationIteration}";
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                while (loadedAssemblies.Any(a => a.GetName().Name.Equals(dataModelAssemblyNameWithIteration)))
                {
                    compilationIteration++;
                    dataModelAssemblyNameWithIteration = $"{dataModelAssemblyName}_{compilationIteration}";
                }

                return dataModelAssemblyNameWithIteration;
            }
        }
    }

    /// <summary>
    /// Close a Prefab which was previously opened to be edited on automation editor
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="autoSave"></param>
    /// <returns></returns>
    async Task ClosePrefabAsync(Prefab prefab, VersionInfo version, Entity prefabEntity, bool autoSave)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ClosePrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(prefab, nameof(prefab)).NotNull();
            activity?.SetTag("Prefab", prefab.DisplayName);
            if (autoSave)
            {
                await SavePrefabAsync(prefab, version, prefabEntity);
            }
            NotifyOfPropertyChange(nameof(CanSaveAll));

            CleanUpScriptEditor();
            RemoveFromEditor();

            logger.Information("Prefab : '{0}' was closed", prefab.DisplayName);

            void RemoveFromEditor()
            {                
                prefabEntity.EntityManager.Dispose();

                var componentViewBuilder = prefabEntity.Parent.EntityManager.GetServiceOfType<IComponentViewBuilder>();              
                componentViewBuilder.ClosePrefab(prefab, prefabEntity);
            }

            void CleanUpScriptEditor()
            {
                //Remove the script editor project that was added while opening the test case
                var entityManager = prefabEntity.EntityManager;
                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.RemoveProject(prefab.PrefabId);
                prefabEntity.DisposeEditors();
            }
        }
    }

    /// <inheritdoc/>      
    public async Task ManageControlReferencesAsync(PrefabViewModel prefabVM)
    {
        try
        {
            if (prefabVM.IsOpenForEdit)
            {
                var openInstance = this.openForEdit[prefabVM.PrefabId];
                var versionManager = this.versionManagerFactory.CreateControlReferenceManager(openInstance.ReferenceManager);
                await this.windowManager.ShowDialogAsync(versionManager);
            }
            
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error on control references screen");
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    /// <summary>
    /// Retrieve the latest version of test case and update the underlying model on view model to use it
    /// </summary>
    /// <param name="testCaseVM"></param>
    /// <returns></returns>
    async Task RefreshPrefabAsync(PrefabViewModel prefabVM)
    {
        var latestVersion = await this.prefabManager.GetPrefabByIdAsync(prefabVM.ApplicationId, prefabVM.PrefabId, prefabVM.Prefab.LatestActiveVersion.ToString());
        if (latestVersion != null)
        {
            prefabVM.WithPrefab(latestVersion);
        }
    }

    /// <summary>
    /// Save data files belonging to the TestFixture
    /// </summary>
    /// <param name="fixtureVM"></param>
    /// <returns></returns>
    async Task SavePrefabAsync(Prefab prefab, VersionInfo version, Entity prefabEntity)
    {
        await this.prefabManager.SavePrefabEntityAsync(prefab, version, prefabEntity);
    }
  
    #endregion Test Case

    #region save all

    public bool CanSaveAll
    {
        get => this.Prefabs.Any(a => a.IsOpenForEdit);
    }

    /// <summary>
    /// Save all open prefabs.     
    /// </summary>
    public async Task SaveAll()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SaveAll), ActivityKind.Internal))
        {
            foreach (var prefab in this.openForEdit.Values)
            {
                try
                {
                    await SavePrefabAsync(prefab.Prefab, prefab.Version, prefab.PrefabEntity);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to save prefab : {0}.", prefab.Prefab.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }
    }

    #endregion save all

    #region life cycle

    /// <summary>
    /// Called when the view is deactivated. close will be true when view should be closed as well.
    /// </summary>
    /// <param name="close"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (close)
        {
            this.Prefabs.Clear();            
        }
        return base.OnDeactivateAsync(close, cancellationToken);

    }

    private readonly Dictionary<string, PrefabInstance> openForEdit = new ();

    public async Task HandleAsync(OpenPrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(OpenPrefabRequestNotification.PrefabId)).NotNull().NotEmpty();

            var hostEntityManager = message.PrefabHost.EntityManager;
            var referenceManager = hostEntityManager.GetServiceOfType<IReferenceManager>();
            var versionInUse = referenceManager.GetPrefabReferences().GetPrefabReference(message.PrefabId).Version;
            var targetPrefab = await this.prefabManager.GetPrefabByIdAsync(message.PrefabHost.ApplicationId, message.PrefabId, versionInUse.ToString());
            if (targetPrefab is not null)
            {
                if (openForEdit.ContainsKey(targetPrefab.PrefabId))
                {
                    await notificationManager.ShowWarningNotificationAsync($"{targetPrefab.DisplayName} is already open for edit.");
                    logger.Information("Prefab : '{0}' is already open for edit", targetPrefab.DisplayName);
                    return;
                }
                message.PrefabHost.IsInDesignMode = true;
               
                var openPrefabInstance = await OpenPrefabAsync(targetPrefab, new VersionInfo(versionInUse), message.PrefabHost);
                openForEdit[targetPrefab.PrefabId] = openPrefabInstance;

                MarkOpenForEdit(message);
            }
        }
        catch (Exception ex)
        {
            message.PrefabHost.IsInDesignMode = false;
            logger.Error(ex, "Error while trying to open prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }

        void MarkOpenForEdit(OpenPrefabRequestNotification message)
        {
            var prefabVM = this.Prefabs.FirstOrDefault(p => p.PrefabId.Equals(message.PrefabId));
            if (prefabVM is not null)
            {
                prefabVM.IsOpenForEdit = true;
            }
        }
    }

    public async Task HandleAsync(ClosePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(message.PrefabId)).NotNull().NotEmpty();
            if(this.openForEdit.ContainsKey(message.PrefabId))
            {
                var prefab = openForEdit[message.PrefabId];
                await ClosePrefabAsync(prefab.Prefab, prefab.Version, prefab.PrefabEntity, autoSave: message.SaveChanges);
            }
            openForEdit.Remove(message.PrefabId);
            message.PrefabHost.IsInDesignMode = false;
            MarkClosedForEdit(message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to close test case with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }

        void MarkClosedForEdit(ClosePrefabRequestNotification message)
        {
            var prefabVM = this.Prefabs.FirstOrDefault(p => p.PrefabId.Equals(message.PrefabId));
            if (prefabVM is not null)
            {
                prefabVM.IsOpenForEdit = false;
            }
        }
    }

    public async Task HandleAsync(SavePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(SavePrefabRequestNotification.PrefabId)).NotNull().NotEmpty();

            if (this.openForEdit.ContainsKey(message.PrefabId))
            {
                var prefab = openForEdit[message.PrefabId];
                await SavePrefabAsync(prefab.Prefab, prefab.Version, prefab.PrefabEntity);
            }           
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to save prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    public async Task HandleAsync(ControlAddedEventArgs message, CancellationToken cancellationToken)
    {
        try
        {

            if (!string.IsNullOrEmpty(message.AddedToPrefab) && this.openForEdit.ContainsKey(message.AddedToPrefab))
            {
                var referenceManager = this.openForEdit[message.AddedToPrefab].ReferenceManager;
                await referenceManager.AddControlReferenceAsync(new ControlReference()
                {
                    ApplicationId = message.Control.ApplicationId,
                    ControlId = message.Control.ControlId,
                    Version = message.Control.Version
                });
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to add control reference to prefab");
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    #endregion
}