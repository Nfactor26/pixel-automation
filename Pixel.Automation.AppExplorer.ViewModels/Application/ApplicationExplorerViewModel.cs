using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    public class ApplicationExplorerViewModel : Conductor<IScreen>.Collection.OneActive, IToolBox, IDisposable
    {
        private readonly string applicationsRepository = "ApplicationsRepository";
        private readonly string controlsDirectory = "Controls";
        private readonly string prefabsDirectory = "Prefabs";

        private readonly IEventAggregator eventAggregator;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;

        public ControlExplorerViewModel ControlExplorer { get; private set; }

        public PrefabExplorerViewModel PrefabExplorer { get; private set; }

        public BindableCollection<ApplicationDescription> Applications { get; set; } = new BindableCollection<ApplicationDescription>();

        public BindableCollection<KnownApplication> KnownApplications { get; set; } = new BindableCollection<KnownApplication>();

        private ApplicationDescription selectedApplication;
        public ApplicationDescription SelectedApplication
        {
            get => selectedApplication;
            set
            {
                selectedApplication = value;
                NotifyOfPropertyChange(() => SelectedApplication);
                CanEdit = false;
                if (value != null)
                {
                    //Notification for property grid to display selected application details
                    this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value.ApplicationDetails));
                }

            }
        }      

        public ApplicationExplorerViewModel(IEventAggregator eventAggregator, ISerializer serializer, ITypeProvider typeProvider, ControlExplorerViewModel controlExplorer, PrefabExplorerViewModel prefabExplorer)
        {
            this.DisplayName = "Application Repository";
            this.eventAggregator = eventAggregator;
            this.serializer = serializer;
            this.typeProvider = typeProvider;
            this.ControlExplorer = controlExplorer;
            this.PrefabExplorer = prefabExplorer;
            this.ControlExplorer.ControlCreated += OnControlCreated;
            this.ControlExplorer.ControlDeleted += OnControlDeleted;
            this.PrefabExplorer.PrefabCreated += OnPrefabCreated;
            this.PrefabExplorer.PrefabDeleted += OnPrefabDeleted;
            CreateCollectionView();
            InitializeKnownApplications();
            LoadApplications();
        }      

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(Applications);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ApplicationDescription.ApplicationName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ApplicationDescription.ApplicationName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as ApplicationDescription).ApplicationName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        private void OnControlCreated(object sender, ControlDescription e)
        {
            if (this.selectedApplication != null)
            {
                this.selectedApplication.AddControl(e);
                SaveApplication(this.selectedApplication);
            }
            Debug.Assert(this.selectedApplication != null, "SelectedApplication is null");
        }

        private void OnControlDeleted(object sender, ControlDescription e)
        {
            if (this.selectedApplication != null)
            {
                this.selectedApplication.DeleteControl(e);
                SaveApplication(this.selectedApplication);
            }
            Debug.Assert(this.selectedApplication != null, "SelectedApplication is null");
        }

        private void OnPrefabCreated(object sender, PrefabDescription e)
        {
            if (this.selectedApplication != null)
            {
                this.selectedApplication.AddPrefab(e);               
                SaveApplication(this.selectedApplication);
            }
            Debug.Assert(this.selectedApplication != null, "SelectedApplication is null");
        }

        private void OnPrefabDeleted(object sender, PrefabDescription e)
        {
            if (this.selectedApplication != null)
            {
                this.selectedApplication.DeletePrefab(e);  
                SaveApplication(this.selectedApplication);
            }
            Debug.Assert(this.selectedApplication != null, "SelectedApplication is null");
        }

        public void ShowInExplorer()
        {

        }


        #region Filter 


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
                var view = CollectionViewSource.GetDefaultView(Applications);
                view.Refresh();
                NotifyOfPropertyChange(() => Applications);

            }
        }

        #endregion Filter

        #region Manage Application

        public bool IsApplicationOpen { get; set; }

        public void OpenApplication(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();

            IsApplicationOpen = true;
            ControlExplorer.SetActiveApplication(application);
            PrefabExplorer.SetActiveApplication(application);
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(application.ApplicationName, application.ApplicationId));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
        }
       
        public void GoBack()
        {
            IsApplicationOpen = false;
            //if (this.selectedApplication.IsDirty)
            //{
            //    SaveApplication(this.selectedApplication);
            //}
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(string.Empty, string.Empty));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
        }

        public void AddApplication(KnownApplication knownApplication)
        {
            Guard.Argument(knownApplication).NotNull();

            IApplication application = (IApplication)Activator.CreateInstance(knownApplication.UnderlyingApplicationType);

            ApplicationDescription newApplication = new ApplicationDescription(application);
            if (string.IsNullOrEmpty(newApplication.ApplicationName))
            {
                newApplication.ApplicationName = $"{this.Applications.Count() + 1}";
            }
            Directory.CreateDirectory(Path.Combine(applicationsRepository, newApplication.ApplicationId));
            Directory.CreateDirectory(Path.Combine(applicationsRepository, newApplication.ApplicationId, controlsDirectory));
            Directory.CreateDirectory(Path.Combine(applicationsRepository, newApplication.ApplicationId, prefabsDirectory));

            this.Applications.Add(newApplication);
            this.SelectedApplication = newApplication;
            SaveApplication(newApplication);
            NotifyOfPropertyChange(() => Applications);
            Log.Information($"New application of type {application.ToString()} has been added to the application repository");
        }

        public void DeleteApplication(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();

            string applicationFolder = GetApplicationDirectory(application);
            if (Directory.Exists(applicationFolder))
            {
                Directory.Delete(applicationFolder, true);
            }
            Applications.Remove(application);
            Log.Information($"Application with name : {application.ApplicationName} has been deleted from applicaton repository");
        }

        /// <summary>
        /// Save the ApplicationDetails of ApplicationToolBoxItem to File
        /// </summary>
        /// <param name="application"></param>
        public void SaveApplication(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();

            string appDirectory = GetApplicationDirectory(application);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            string appFile = GetApplicationFile(application);
            if (File.Exists(appFile))
            {
                File.Delete(appFile);
            }

            serializer.Serialize(appFile, application, typeProvider.KnownTypes["Default"]);
            //application.IsDirty = false;
        }


        private string GetApplicationDirectory(ApplicationDescription application)
        {
            return Path.Combine(applicationsRepository, application.ApplicationId);
        }

        private string GetApplicationFile(ApplicationDescription application)
        {
            return Path.Combine(applicationsRepository, application.ApplicationId, $"{application.ApplicationId}.app");
        }

        private void LoadApplications()
        {
            foreach (var app in Directory.GetDirectories(applicationsRepository))
            {
                string appFile = Directory.GetFiles(Path.Combine(this.applicationsRepository, new DirectoryInfo(app).Name), "*.app", SearchOption.TopDirectoryOnly).FirstOrDefault();
                ApplicationDescription application = serializer.Deserialize<ApplicationDescription>(appFile);
                Applications.Add(application);
            }
            Applications.OrderBy(a => a.ApplicationName);
        }

        private void InitializeKnownApplications()
        {
            var knownApps = this.typeProvider.GetAllTypes().Where(t => t.GetInterface(nameof(IApplication)) != null);
            foreach(var knownApp in knownApps)
            {
                string displayName = TypeDescriptor.GetAttributes(knownApp).OfType<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName ?? knownApp.Name;
                string description = TypeDescriptor.GetAttributes(knownApp).OfType<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? knownApp.Name;
                KnownApplication knownApplication = new KnownApplication() { DisplayName = displayName, Description = description, UnderlyingApplicationType = knownApp };
                this.KnownApplications.Add(knownApplication);
            }
        }

        #endregion Manage Application

        #region Edit Application       

        bool canEdit;
        public bool CanEdit
        {
            get
            {
                return canEdit;
            }
            set
            {
                canEdit = value;
                NotifyOfPropertyChange(() => CanEdit);
            }
        }

        public void ToggleRename(ApplicationDescription targetItem)
        {
            if (selectedApplication == targetItem)
                CanEdit = !CanEdit;
        }

        public void RenameApplication(ActionExecutionContext context, ApplicationDescription application)
        {
            try
            {
                KeyEventArgs keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    if (newName != application.ApplicationName)
                    {
                        application.ApplicationName = newName;
                        application.ApplicationDetails.ApplicationName = newName;
                        SaveApplication(application);
                        CanEdit = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                CanEdit = false;
            }
        }

        #endregion Edit Application

        #region IToolBox

        bool isActiveItem;
        public bool IsActiveItem
        {
            get => isActiveItem;
            set
            {
                isActiveItem = value;
                NotifyOfPropertyChange(() => IsActiveItem);
            }

        }

        public PaneLocation PreferredLocation
        {
            get
            {
                return PaneLocation.Bottom;
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public double PreferredWidth
        {
            get
            {
                return 250;
            }
        }

        public double PreferredHeight
        {
            get
            {
                return 280;
            }
        }

        protected ICommand closeCommand;
        public virtual ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand<bool>(p => IsVisible = false, p => true)); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        #endregion IToolBox        

        #region IDisposable

        public void Dispose()
        {
            this.ControlExplorer.ControlCreated -= OnControlCreated;
            this.ControlExplorer.ControlDeleted -= OnControlDeleted;
            this.PrefabExplorer.PrefabCreated -= OnPrefabCreated;
            this.PrefabExplorer.PrefabDeleted -= OnPrefabDeleted;
        }

        #endregion IDisposable
    }
}
