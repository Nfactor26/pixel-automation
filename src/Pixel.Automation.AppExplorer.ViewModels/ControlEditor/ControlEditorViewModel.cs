using Dawn;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public class ControlEditorViewModel : ControlEditorBaseViewModel, IControlEditor
    {

        IControlIdentity rootControl;
        IControlIdentity leafControl;

        public ObservableCollection<IControlIdentity> ControlHierarchy { get; } = new ObservableCollection<IControlIdentity>();
        
        IControlIdentity selectedControl;
        public IControlIdentity SelectedControl
        {
            get => selectedControl;
            set
            {
                selectedControl = value;
                if (selectedControl == leafControl)
                {
                    ShowOffSetPointer();
                    IsOffsetPoiniterVisible = Visibility.Visible;
                }
                else
                {
                    IsOffsetPoiniterVisible = Visibility.Collapsed;
                }
                NotifyOfPropertyChange(() => SelectedControl);

            }

        }

        public string controlImage;
        public string ControlImage
        {
            get
            {
                return this.controlImage;
            }
            set
            {
                this.controlImage = value;
            }
        }

        ImageSource imageSource;
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
            set
            {
                this.imageSource = value;
                NotifyOfPropertyChange(() => ImageSource);
            }
        }
      
        public ControlEditorViewModel()
        {
            this.DisplayName = "Control Editor";
        }

        protected override void OnViewReady(object view)
        {
            this.view = view as FrameworkElement;
            base.OnViewReady(view);
            this.SelectedControl = leafControl;
        }

        public void Initialize(ControlDescription targetControl)
        {
            Guard.Argument(targetControl).NotNull();

            this.rootControl = targetControl.ControlDetails;
            RefreshHierarchy();
            this.ControlImage = targetControl.ControlImage;
            if (!string.IsNullOrEmpty(this.controlImage))
            {               
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.UriSource = new Uri(this.ControlImage, UriKind.Relative);
                source.EndInit();
                this.ImageSource = source;
            }
        }

        protected override (double width, double height) GetImageDimension()
        {
            return (ImageSource.Width, ImageSource.Height);
        }

        protected override System.Drawing.Point GetOffSet()
        {
            return new System.Drawing.Point((int)this.rootControl.XOffSet, (int)this.rootControl.YOffSet);
        }

        protected override void SetOffSet(System.Drawing.Point offSet)
        {
            this.rootControl.XOffSet = offSet.X;
            this.rootControl.YOffSet = offSet.Y;
        }

        protected override Pivots GetPivotPoint()
        {
            return this.rootControl.PivotPoint;
        }

        protected override void SetPivotPoint(Pivots pivotPoint)
        {
            this.rootControl.PivotPoint = pivotPoint;
        }

        #region Control Hierarchy

        public void RemoveFromControlHierarchy(IControlIdentity controlToRemove)
        {
            Guard.Argument(controlToRemove).NotNull();

            IControlIdentity root = this.rootControl;
            if (root == controlToRemove)
            {
               if(root.Next != null)
                {
                    this.rootControl = root.Next;                   
                }
                return;
            }

            IControlIdentity current = root;
            while (current.Next != controlToRemove)
            {
                current = current.Next;
            }

            current.Next = current.Next?.Next;
            RefreshHierarchy();
                      

        }

        /// <summary>
        /// Create a copy of controldentity and set it as next control in the LinkedList making any necessary adjustments
        /// </summary>
        /// <param name="controlIdentity"></param>
        public void InsertIntoControlHierarchy(IControlIdentity controlIdentity)
        {
            Guard.Argument(controlIdentity).NotNull();
            IControlIdentity current = controlIdentity;
            IControlIdentity copyOfSelectedControl = controlIdentity.Clone() as IControlIdentity;
            if (current.Next != null)
            {
                IControlIdentity next = current.Next;
                current.Next = copyOfSelectedControl;
                copyOfSelectedControl.Next = next;
            }
            else
            {
                current.Next = copyOfSelectedControl;
            }
            RefreshHierarchy();

        }

        private void RefreshHierarchy()
        {
            this.ControlHierarchy.Clear();
            var current = this.rootControl;
            while (current != null)
            {
                this.ControlHierarchy.Add(current);
                if (current.Next == null)
                {
                    leafControl = current;
                }
                current = current.Next;
            }

        }

        #endregion Control Hierarchy
            
        public async void Save()
        {
            CleanUp();
            await this.TryCloseAsync(true);
        }

        public async void Cancel()
        {
            CleanUp();
            await this.TryCloseAsync(false);
        }

        private void CleanUp()
        {
            this.ControlHierarchy.Clear();
            this.view = null;
            this.rootControl = null;
            this.selectedControl = null;
            this.imageSource = null;
        }     
    }
}
