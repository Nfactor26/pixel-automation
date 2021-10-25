using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Enums;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public abstract class ControlEditorBaseViewModel : Screen
    {
        protected FrameworkElement view;

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

        protected Rectangle templateBoundingBox;

        protected Visibility isOffsetPointerVisible = Visibility.Collapsed;
        public Visibility IsOffsetPoiniterVisible
        {
            get => isOffsetPointerVisible;
            set
            {
                isOffsetPointerVisible = value;
                NotifyOfPropertyChange(() => IsOffsetPoiniterVisible);
            }
        }

        public ControlEditorBaseViewModel()
        {
            
        }

        protected abstract (double width, double height) GetImageDimension();

        protected abstract System.Drawing.Point GetOffSet();

        protected abstract void SetOffSet(System.Drawing.Point offSet);

        protected abstract Pivots GetPivotPoint();

        protected abstract void SetPivotPoint(Pivots pivotPoint);
       
        #region Offset Editor

        protected void ShowOffSetPointer()
        {
            FrameworkElement canvas = this.view.FindName("DesignerCanvas") as FrameworkElement;
            FrameworkElement imageHolder = this.view.FindName("ControlImage") as FrameworkElement;

            (double width, double height) = GetImageDimension();
            this.templateBoundingBox = new Rectangle(Convert.ToInt32((canvas.ActualWidth - width) / 2), Convert.ToInt32((canvas.ActualHeight - height) / 2), Convert.ToInt32(width), Convert.ToInt32(height));

            this.offset = GetOffSet();
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

            switch (GetPivotPoint())
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
                    SetPivotPoint(Pivots.Center);
                    break;
                case "TopLeft":
                    SetPivotPoint(Pivots.TopLeft);
                    break;
                case "TopRight":
                    SetPivotPoint(Pivots.TopRight);
                    break;
                case "BottomLeft":
                    SetPivotPoint(Pivots.BottomLeft);
                    break;
                case "BottomRight":
                    SetPivotPoint(Pivots.BottomRight);
                    break;

            }
            offset.X = 0;
            offset.Y = 0;

            UpdatePivotPoint();

            absOffset.X = pivotPoint.X + offset.X - 16;
            absOffset.Y = pivotPoint.Y + offset.Y - 16;

            SetOffSet(offset);          

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

            SetOffSet(offset);

            absOffset.X = pivotPoint.X + offset.X - 16;
            absOffset.Y = pivotPoint.Y + offset.Y - 16;
            NotifyOfPropertyChange(() => Offset);

            FrameworkElement offsetPointer = view.FindName("OffsetPointer") as FrameworkElement;
            offsetPointer.SetValue(Canvas.LeftProperty, (double)this.Offset.X);
            offsetPointer.SetValue(Canvas.TopProperty, (double)this.Offset.Y);
        }

        #endregion Offset Editor

    }
}
