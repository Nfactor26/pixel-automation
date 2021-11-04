using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pixel.Automation.Designer.Views
{
    public class EditorView : UserControl, INotifyPropertyChanged
    {
        private double zoomFactor = 0.10;
        private bool isPanModeEnabled = false;

        double scaleX = 1.0;
        public double ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {

                if (value <= 0.25)
                    value = 0.25;
                if (value >= 2.5)
                    value = 2.5;
                scaleX = value;
                OnPropertyChanged();
            }
        }

        double scaleY = 1.0;
        public double ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                if (value <= 0.25)
                    value = 0.25;
                if (value >= 2.5)
                    value = 2.5;
                scaleY = value;
                OnPropertyChanged();
            }
        }

        double trasnformX = 0.0;
        public double TransformX
        {
            get
            {
                return trasnformX;
            }
            set
            {
                trasnformX = value;
                OnPropertyChanged();
            }
        }

        double transformY = 0.0;
        public double TransformY
        {
            get
            {
                return transformY;
            }
            set
            {
                transformY = value;
                OnPropertyChanged();
            }
        }

        protected virtual UIElement GetDesignerHost()
        {
            throw new NotImplementedException();
        }

        protected void ZoomOnScroll(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                double zoomFactor = e.Delta > 0 ? .15 : -.15;
                this.ScaleX += zoomFactor;
                this.ScaleY += zoomFactor;
                e.Handled = true;
            }

            this.TransformY += e.Delta / 5;
        }

        protected void OnZoomInClicked(object sender, RoutedEventArgs e)
        {
            this.ScaleX += zoomFactor;
            this.ScaleY += zoomFactor;
        }

        protected void OnZommOutClicked(object sender, RoutedEventArgs e)
        {
            this.ScaleX -= zoomFactor;
            this.ScaleY -= zoomFactor;
        }

        bool isDragging = false;
        Point lastPos;
        Point currPos;
        protected void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isPanModeEnabled)
            {
                isDragging = true;
                lastPos = e.GetPosition(GetDesignerHost());
                GetDesignerHost().CaptureMouse();
                this.Cursor = Cursors.ScrollAll;
            }
        }

        protected void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPanModeEnabled && isDragging)
            {
                currPos = e.GetPosition(GetDesignerHost());
                var delta = currPos - lastPos;
                this.TransformX += delta.X;
                this.TransformY += delta.Y;
                lastPos = currPos;
            }
        }

        protected void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanModeEnabled)
            {
                isDragging = false;
                lastPos = default(Point);
                currPos = default(Point);
                GetDesignerHost().ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }

        }
        
        protected void OnTogglePanMode(object sender, RoutedEventArgs e)
        {
            this.isPanModeEnabled = !this.isPanModeEnabled;
        }

        protected void OnResetAllClicked(object sender, RoutedEventArgs e)
        {
            this.TransformX = 0;
            this.TransformY = 0;
            this.ScaleX = 1;
            this.ScaleY = 1;
        }

        protected void OnActiveItemChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TransformX = 0;
            this.TransformY = 0;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
