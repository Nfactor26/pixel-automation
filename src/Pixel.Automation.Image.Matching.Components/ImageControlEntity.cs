using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Image.Matching.Components
{
    public enum ImageSearchScope
    {
        Desktop,
        Application,
        Custom
    }

    [DataContract]
    [Serializable]    
    public class ImageControlEntity : ControlEntity
    {
        [DataMember]      
        [Browsable(false)]
        public override Argument SearchRoot
        {
            get => base.SearchRoot;
            set => base.SearchRoot = value;
        }

        private ImageSearchScope imageSearchScope = ImageSearchScope.Application;
        [DataMember]
        [Display(Name = "Search scope", GroupName = "Search Strategy", Order = 5)]
        [Description("Indicates how the search scope i.e. area on screen where search will be performed")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public ImageSearchScope ImageSearchScope
        {
            get
            {
                switch (this.imageSearchScope)
                {
                    case ImageSearchScope.Desktop:
                        this.SetDispalyAttribute(nameof(TargetWindow), false);
                        this.SetDispalyAttribute(nameof(AreaOnScreen), false);
                        break;
                    case ImageSearchScope.Application:
                        this.SetDispalyAttribute(nameof(TargetWindow), true);
                        this.SetDispalyAttribute(nameof(AreaOnScreen), false);
                        break;
                    case ImageSearchScope.Custom:
                        this.SetDispalyAttribute(nameof(TargetWindow), false);
                        this.SetDispalyAttribute(nameof(AreaOnScreen), true);
                        break;
                }
                return this.imageSearchScope;
            }
            set
            {
                this.imageSearchScope = value;              
                OnPropertyChanged();
            }
        }       
                

        /// <summary>
        /// Optional. When Search scope is Application, by default image lookup is within the bounding box of parent application window.
        /// However, if search area needs to be restrained to another child window of the owner application or even another application window,
        /// target window can be specified  as ApplicationWindow. ChildWindow can be searched using FindChildWindowActorComponent. 
        /// </summary>
        [DataMember]
        [Display(Name = "Target window", GroupName = "Search Strategy", Order = 10)]    
        [Description("Target window within which image lookup will be restricted")]     
        public InArgument<ApplicationWindow> TargetWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        /// <summary>
        /// A custom bounding box can be provided when search scope is set to custom. Image lookup is constrained within this bounding box.
        /// </summary>
        [DataMember]
        [Display(Name = "Area on screen", GroupName = "Search Strategy", Order = 15)]
        [Description("Target window within which image lookup will be restricted")]    
        public InArgument<BoundingBox> AreaOnScreen { get; set; } = new InArgument<BoundingBox>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<BoundingBox>() { CanChangeMode = false, CanChangeType = false };
            }
        }
    
        public override async Task<UIControl> GetControl()
        {
            BoundingBox regionOfInterest = await GetRegionOfInterest();
            ImageControlLocatorComponent controlLocator;
            if (this.EntityManager.TryGetOwnerApplication(this, out IApplication application))
            {
                controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as ImageControlLocatorComponent;
            }
            else
            {
                controlLocator = new ImageControlLocatorComponent();
            }
            UIControl targetControl = default;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    targetControl = await controlLocator.FindControlAsync(this.ControlDetails, new ImageUIControl(this.ControlDetails, regionOfInterest));
                    break;
                case LookupMode.FindAll:
                    var descendantControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, new ImageUIControl(this.ControlDetails, regionOfInterest));
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            targetControl = await GetElementAtIndex(descendantControls);
                            break;
                        case FilterMode.Custom:
                            targetControl = GetElementMatchingCriteria(descendantControls);
                            break;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
           
            return targetControl;
        }


        public override async Task<IEnumerable<UIControl>> GetAllControls()
        {
            BoundingBox regionOfInterest = await GetRegionOfInterest();
            ImageControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as ImageControlLocatorComponent;
            var foundControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, new ImageUIControl(this.ControlDetails, regionOfInterest));          
            return foundControls;
        }      

        /// <summary>
        /// Get bounding box of the application if SearchWithinApplication is true.
        /// Control locator treats null value as search on entire desktop
        /// </summary>
        /// <returns></returns>
        private async  Task<BoundingBox> GetRegionOfInterest()
        {
            switch(this.ImageSearchScope)
            {
                case ImageSearchScope.Desktop:
                    //For now, ImageControlLocator will capture entire desktop when we don't pass any BoundingBox as search scope parameter.
                    //TODO : Find how can we get screen resolution without relying on System.Windows.Forms.Screen.PrimaryScreen.Bounds which ScreenCapture
                    //is using.
                    break;
                case ImageSearchScope.Application:
                    var windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
                    if (TargetWindow.IsConfigured())
                    {
                        var targetWindow = await ArgumentProcessor.GetValueAsync<ApplicationWindow>(this.TargetWindow);
                        return targetWindow.WindowBounds;
                    }
                    else
                    {
                        if(this.EntityManager.TryGetOwnerApplication<IApplication>(this, out IApplication parentApplication))
                        {
                            var appRectangle = windowManager.GetWindowSize(parentApplication.Hwnd);
                            return appRectangle;
                        }
                        throw new ConfigurationException($"Search scope is Application. However, {this} doesn't have a Application Context.");
                    }                   
                case ImageSearchScope.Custom:
                    if(AreaOnScreen.IsConfigured())
                    {
                        return await ArgumentProcessor.GetValueAsync<BoundingBox>(this.AreaOnScreen);
                    }
                    throw new ConfigurationException("Search scope is configured as Custom. However, required input AreaOnScreen is not configured or incorrectly configured");                   
            }
            return null;
        }

        public override void ResolveDependencies()
        {
            if (this.EntityManager.TryGetOwnerApplication<IApplication>(this, out IApplication parentApplication))
            {
                var applicationPoolEntity = this.EntityManager.RootEntity.GetComponentsByTag("ApplicationPoolEntity", SearchScope.Children).FirstOrDefault() as Entity;
                var applicationsInPool = applicationPoolEntity.GetComponentsOfType<IApplicationEntity>(SearchScope.Descendants);
                if (applicationsInPool != null)
                {
                    var targetApp = applicationsInPool.FirstOrDefault(a => a.ApplicationId.Equals(parentApplication.ApplicationId)) as Entity;
                    if (!targetApp?.GetComponentsOfType<ImageControlLocatorComponent>().Any() ?? false)
                    {
                        targetApp.AddComponent(new ImageControlLocatorComponent());
                    }
                }
            }
        }
    }
   
}
