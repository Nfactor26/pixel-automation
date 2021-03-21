using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public class ControlEditorViewModel : Screen, IControlEditor
    {

        IControlIdentity rootControl;

        public ObservableCollection<IControlIdentity> ControlHierarchy { get; } = new ObservableCollection<IControlIdentity>();


        IControlIdentity leafControl;


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

        FrameworkElement view;

        System.Drawing.Point pivotPoint;
        System.Drawing.Point offset;
        System.Drawing.Point absOffset;
        public System.Drawing.Point Offset
        {
            get
            {
                return absOffset;
            }
        }

        Rectangle templateBoundingBox;

        Visibility isOffsetPointerVisible = Visibility.Collapsed;
        public Visibility IsOffsetPoiniterVisible
        {
            get => isOffsetPointerVisible;
            set
            {
                isOffsetPointerVisible = value;
                NotifyOfPropertyChange(() => IsOffsetPoiniterVisible);
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
        }

        public void Initialize(IControlIdentity rootControl)
        {
            Guard.Argument(rootControl).NotNull();

            this.rootControl = rootControl;
            RefreshHierarchy();
            this.ControlImage = rootControl.ControlImage;
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
                    this.rootControl.LookupType = LookupType.Relative;
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

        #region Offset Editor

        private void ShowOffSetPointer()
        {
            FrameworkElement canvas = this.view.FindName("DesignerCanvas") as FrameworkElement;
            FrameworkElement imageHolder = this.view.FindName("ControlImage") as FrameworkElement;

            this.templateBoundingBox = new Rectangle(Convert.ToInt32((canvas.ActualWidth - imageSource.Width) / 2), Convert.ToInt32((canvas.ActualHeight - imageSource.Height) / 2), Convert.ToInt32(imageSource.Width), Convert.ToInt32(imageSource.Height));

            this.offset = new System.Drawing.Point((int)leafControl.XOffSet, (int)leafControl.YOffSet);
            UpdatePivotPoint();
            this.absOffset = new System.Drawing.Point(pivotPoint.X + offset.X - 16, pivotPoint.Y + offset.Y - 16);
            NotifyOfPropertyChange(() => Offset);
        }

        private void UpdatePivotPoint()
        {
            //Note : Don't delete 

            //Use this if you want to allow offset within the bounds of control
            //switch (leafControl.PivotPoint)
            //{
            //    case TestSuite.Models.Pivots.Center:
            //        this.pivotPoint = new System.Drawing.Point((this.templateBoundingBox.X + this.templateBoundingBox.Width / 2), (this.templateBoundingBox.Y + this.templateBoundingBox.Height / 2));
            //        break;
            //    case TestSuite.Models.Pivots.TopLeft:
            //        this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X, this.templateBoundingBox.Y);
            //        break;
            //    case TestSuite.Models.Pivots.TopRight:
            //        this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X + this.templateBoundingBox.Width, this.templateBoundingBox.Y);
            //        break;
            //    case TestSuite.Models.Pivots.BottomLeft:
            //        this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X, this.templateBoundingBox.Y + this.templateBoundingBox.Height);
            //        break;
            //    case TestSuite.Models.Pivots.BottomRight:
            //        this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X + this.templateBoundingBox.Width, this.templateBoundingBox.Y + this.templateBoundingBox.Height);
            //        break;
            //}

            switch (leafControl.PivotPoint)
            {
                case Pivots.Center:
                    this.pivotPoint = new System.Drawing.Point((this.templateBoundingBox.X + this.templateBoundingBox.Width / 2), (this.templateBoundingBox.Y + this.templateBoundingBox.Height / 2));
                    break;
                case Pivots.TopLeft:
                    this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X, this.templateBoundingBox.Y);
                    break;
                case Pivots.TopRight:
                    this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X + this.templateBoundingBox.Width, this.templateBoundingBox.Y);
                    break;
                case Pivots.BottomLeft:
                    this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X, this.templateBoundingBox.Y + this.templateBoundingBox.Height);
                    break;
                case Pivots.BottomRight:
                    this.pivotPoint = new System.Drawing.Point(this.templateBoundingBox.X + this.templateBoundingBox.Width, this.templateBoundingBox.Y + this.templateBoundingBox.Height);
                    break;
            }

        }

        public void ChangePivotPoint(string name)
        {
            Guard.Argument(name).NotNull().NotEmpty();
            switch (name)
            {
                case "Center":
                    leafControl.PivotPoint = Pivots.Center;
                    break;
                case "TopLeft":
                    leafControl.PivotPoint = Pivots.TopLeft;
                    break;
                case "TopRight":
                    leafControl.PivotPoint = Pivots.TopRight;
                    break;
                case "BottomLeft":
                    leafControl.PivotPoint = Pivots.BottomLeft;
                    break;
                case "BottomRight":
                    leafControl.PivotPoint = Pivots.BottomRight;
                    break;

            }
            offset.X = 0;
            offset.Y = 0;

            UpdatePivotPoint();

            absOffset.X = pivotPoint.X + offset.X - 16;
            absOffset.Y = pivotPoint.Y + offset.Y - 16;


            leafControl.XOffSet = offset.X;
            leafControl.YOffSet = offset.Y;

            FrameworkElement offsetPointer = view.FindName("OffsetPointer") as FrameworkElement;
            offsetPointer.SetValue(Canvas.LeftProperty, (double)this.Offset.X);
            offsetPointer.SetValue(Canvas.TopProperty, (double)this.Offset.Y);


        }

        public void ChangeOffset(MouseEventArgs args, IInputElement sender)
        {
            System.Windows.Point mouseDownPoint = args.GetPosition(sender);

            //Note : Don't delete 

            //Use this if you want to allow offset within the bounds of control
            //offset.X = this.templateBoundingBox.X + (int)mouseDownPoint.X - pivotPoint.X;
            //offset.Y = this.templateBoundingBox.Y + (int)mouseDownPoint.Y - pivotPoint.Y;

            offset.X = (int)mouseDownPoint.X - pivotPoint.X;
            offset.Y = (int)mouseDownPoint.Y - pivotPoint.Y;


            leafControl.XOffSet = offset.X;
            leafControl.YOffSet = offset.Y;



            absOffset.X = pivotPoint.X + offset.X - 16;
            absOffset.Y = pivotPoint.Y + offset.Y - 16;
            NotifyOfPropertyChange(() => Offset);

            FrameworkElement offsetPointer = view.FindName("OffsetPointer") as FrameworkElement;
            offsetPointer.SetValue(Canvas.LeftProperty, (double)this.Offset.X);
            offsetPointer.SetValue(Canvas.TopProperty, (double)this.Offset.Y);
        }

        #endregion Offset Editor

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
