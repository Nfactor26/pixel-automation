using Caliburn.Micro;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;

namespace Pixel.Automation.PrefabExplorer.ViewModels
{
    /// <summary>
    /// View Model used for editing the details of a <see cref="TestCase"/>
    /// </summary>
    public class EditPrefabViewModel : SmartScreen
    {
        private readonly ILogger logger = Log.ForContext<EditPrefabViewModel>();
        private readonly Prefab prefab;
        private readonly IEnumerable<string> existingPrefabs;
        private readonly IArgumentTypeBrowserFactory typeBrowserFactory;
        private readonly IWindowManager windowManager;

        /// <summary>
        /// A clone of TestCaseViewModel where edits are made.
        /// Actual copy will be replaced with edited copy on user confirmation after edit is done.
        /// </summary>
        public Prefab CopyOfPrefab { get; }

        /// <summary>
        /// DisplayName for the TestCase
        /// </summary>
        public string PrefabDisplayName
        {
            get => CopyOfPrefab.DisplayName;
            set
            {
                CopyOfPrefab.DisplayName = value;
                ValidateProperty(nameof(PrefabDisplayName));
                NotifyOfPropertyChange(() => PrefabDisplayName);
            }
        }
      
        /// <summary>
        /// Friendly name for the <see cref="Prefab.SupportedShape"> of a Prefab
        /// </summary>
        public string SupportedShape
        {
            get => CopyOfPrefab.SupportedShape;
            set
            {
                CopyOfPrefab.SupportedShape = value;
                ValidateProperty(nameof(SupportedShape));
                NotifyOfPropertyChange(() => SupportedShape);
            }
        }

        /// <summary>
        /// Description for the TestCase
        /// </summary>
        public string PrefabDescrpition
        {
            get => CopyOfPrefab.Description;
            set
            {
                CopyOfPrefab.Description = value;
                NotifyOfPropertyChange(() => PrefabDescrpition);
            }
        }     
        

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <param name="existingTestCases"></param>
        public EditPrefabViewModel(Prefab prefab, IEnumerable<string> existingPrefabs, IArgumentTypeBrowserFactory typeBrowserFactory, IWindowManager windowManager)
        {
            this.prefab = prefab;
            this.existingPrefabs = existingPrefabs;
            this.typeBrowserFactory = typeBrowserFactory;
            this.windowManager = windowManager;
            if(prefab.Clone() is Prefab copy)
            {
                this.CopyOfPrefab = copy;
                return;
            }
            throw new InvalidOperationException("Clone is not a Prefab");
        }

        public async Task PickModelAsync()
        {
            var typeBrowser = typeBrowserFactory.CreateArgumentTypeBrowser(true);
            var result = await windowManager.ShowDialogAsync(typeBrowser);
            if(result.HasValue && result.Value && typeBrowser.SelectedType is not null)
            {
                CopyOfPrefab.SupportedShape = typeBrowser.SelectedType.FullName;
            }
        }

        /// <summary>
        /// Apply the edits done of temporary copy to the actual instance of TestCase
        /// and close the screen.
        /// </summary>
        /// <returns></returns>
        public async Task Save()
        {
            try
            {
                if (Validate())
                {
                    this.prefab.DisplayName = CopyOfPrefab.DisplayName;
                    this.prefab.Description = CopyOfPrefab.Description;   
                    this.prefab.SupportedShape = CopyOfPrefab.SupportedShape;
                    await this.TryCloseAsync(true);
                    logger.Information("Edit Prefab changes applied.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);                
            }
        }

        /// <summary>
        /// Close the screen without applying any edits done.
        /// </summary>
        /// <returns></returns>
        public async Task Cancel()
        {
            await this.TryCloseAsync(false);
            logger.Information("Edit Prefab changes were cancelled.");
        }

        #region Validation

        /// <inheritdoc/>
        public override bool ShowModelErrors => HasErrors;

        /// <inheritdoc/>
        public override void DismissModelErrors()
        {           
            NotifyOfPropertyChange(() => ShowModelErrors);
        }
       
        private bool Validate()
        {
            ValidateProperty(nameof(PrefabDisplayName));
            ValidateProperty(nameof(SupportedShape));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(PrefabDisplayName):
                    ValidateRequiredProperty(nameof(PrefabDisplayName), PrefabDisplayName);
                    if (this.existingPrefabs.Any(a => a.Equals(PrefabDisplayName)))
                    {
                        AddOrAppendErrors(nameof(PrefabDisplayName), "Name must be unique.");
                    }
                    break;
                case nameof(SupportedShape):
                    ValidateRequiredProperty(nameof(SupportedShape), SupportedShape);
                    break;
            }          
        }

        #endregion Validation
    }
}
