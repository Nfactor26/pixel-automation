using Caliburn.Micro;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Image.Matching.Components;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.Editor.Image.Capture
{
    public class ImageCaptureViewModel : Screen
    {

        IImageControlIdentity controlIdentity;
        public IImageControlIdentity ControlIdentity
        {
            get
            {
                return controlIdentity;
            }
        }

        public double ScreenWidth
        {
            get => SystemParameters.PrimaryScreenWidth;
        }

        public double ScreenHeight
        {
            get => SystemParameters.PrimaryScreenHeight;
        }

        BoundingBox templateBoundingBox;
        public BoundingBox TemplateBoundingBox
        {
            get => templateBoundingBox;
            set
            {
                templateBoundingBox = value;
                NotifyOfPropertyChange(() => TemplateBoundingBox);
            }
        }    

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

        BitmapSource imageSource;
        public BitmapSource ImageSource
        {    
            get => imageSource;
        }
           

        public ImageCaptureViewModel()
        {        
           
        }


        public IImageControlIdentity GetCapturedImageControl()
        {
            return this.controlIdentity;
        }       

        public void Initialize(Bitmap screenShot)
        {
            this.controlIdentity = new ImageControlIdentity()
            {
                Name = "1",               
                PivotPoint = Pivots.Center
            };
            this.TemplateBoundingBox = new BoundingBox()
            {
                X = 200,
                Y = 200,
                Width = 200,
                Height = 200
            };
            this.offset = new System.Drawing.Point((int)controlIdentity.XOffSet, (int)controlIdentity.YOffSet);

            UpdatePivotPoint();
            absOffset = new System.Drawing.Point(pivotPoint.X + offset.X - 16, pivotPoint.Y + offset.Y - 16);           
          
            var hBitmap = screenShot.GetHbitmap();
            imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            return;
        }

        FrameworkElement view;
        protected override void OnViewReady(object view)
        {
            this.view = view as FrameworkElement;
            base.OnViewReady(view);
        }    

       
        public void UpdateBoundingBox(object sender)
        {
            var container = sender as FrameworkElement;         

            //Log.Debug($"Update Bounding Box for : {container.Name}");

            switch (container.Name)
            {
                case "Template":
                    var templateArea = container;
                    this.TemplateBoundingBox = new BoundingBox(Convert.ToInt32(templateArea.GetValue(Canvas.LeftProperty)), Convert.ToInt32(templateArea.GetValue(Canvas.TopProperty)),
                    Convert.ToInt32(templateArea.GetValue(Canvas.WidthProperty)), Convert.ToInt32(templateArea.GetValue(Canvas.HeightProperty)));
                    controlIdentity.BoundingBox = this.templateBoundingBox;
                    UpdatePivotPoint();
                    absOffset.X = pivotPoint.X + offset.X - 16;
                    absOffset.Y = pivotPoint.Y + offset.Y - 16;
                    NotifyOfPropertyChange("Offset");                    
                    break;               
                case "OffsetPointer":
                    var offSetArea = container;
                    var currentOffset = new System.Drawing.Point(Convert.ToInt32(offSetArea.GetValue(Canvas.LeftProperty)), Convert.ToInt32(offSetArea.GetValue(Canvas.TopProperty)));
                    this.offset.X = currentOffset.X - this.pivotPoint.X + 16;
                    this.offset.Y = currentOffset.Y - this.pivotPoint.Y +  16;
                    controlIdentity.XOffSet = this.offset.X;
                    controlIdentity.YOffSet = this.offset.Y;
                    absOffset.X = pivotPoint.X + offset.X - 16;
                    absOffset.Y = pivotPoint.Y + offset.Y - 16;
                    NotifyOfPropertyChange("Offset");
                    break;
            }           
       
        }

        private void UpdatePivotPoint()
        {
            switch (controlIdentity.PivotPoint)
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
            switch (name)
            {
                case "Center":
                    controlIdentity.PivotPoint = Pivots.Center;
                    break;
                case "TopLeft":
                    controlIdentity.PivotPoint = Pivots.TopLeft;
                    break;
                case "TopRight":
                    controlIdentity.PivotPoint = Pivots.TopRight;
                    break;
                case "BottomLeft":
                    controlIdentity.PivotPoint = Pivots.BottomLeft;
                    break;
                case "BottomRight":
                    controlIdentity.PivotPoint = Pivots.BottomRight;
                    break;

            }

            UpdatePivotPoint();

            absOffset.X = pivotPoint.X + offset.X - 16;
            absOffset.Y = pivotPoint.Y + offset.Y - 16;

            FrameworkElement offsetPointer = view.FindName("OffsetPointer") as FrameworkElement;
            offsetPointer.SetValue(Canvas.LeftProperty, (double)this.Offset.X);
            offsetPointer.SetValue(Canvas.TopProperty, (double)this.Offset.Y);

        
        }

        public async Task Save()
        {
            this.controlIdentity.BoundingBox = this.TemplateBoundingBox;
            await this.TryCloseAsync(true);
        }

        public async Task Exit()
        {           
            await this.TryCloseAsync(false);
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
