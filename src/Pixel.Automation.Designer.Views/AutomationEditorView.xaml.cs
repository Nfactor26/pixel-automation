using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// Interaction logic for AutomationEditorView.xaml
    /// </summary>
    public partial class AutomationEditorView : UserControl , INotifyPropertyChanged
    {
        private double zoomFactor = 0.10;

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

        public AutomationEditorView()
        {
            InitializeComponent();
            WorkFlowRoot.RequestBringIntoView += WorkFlowRoot_RequestBringIntoView;           
        }      

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                double zoomFactor = e.Delta > 0 ? .15 : -.15;
                this.ScaleX += zoomFactor;
                this.ScaleY += zoomFactor;
                e.Handled = true;
            }

            this.TransformY += e.Delta/5;          
        }

        private void WorkFlowRoot_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
        
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            this.ScaleX += zoomFactor;
            this.ScaleY += zoomFactor;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            this.ScaleX -= zoomFactor;
            this.ScaleY -= zoomFactor;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        bool isDragging = false;
        Point lastPos;
        Point currPos;
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(isPanModeEnabled)
            {
                isDragging = true;
                lastPos = e.GetPosition(DesignerRoot);
                DesignerRoot.CaptureMouse();
                this.Cursor = Cursors.ScrollAll;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPanModeEnabled && isDragging)
            {
                currPos = e.GetPosition(DesignerRoot);
                var delta = currPos - lastPos;
                this.TransformX += delta.X;
                this.TransformY += delta.Y;
                lastPos = currPos;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanModeEnabled)
            {
                isDragging = false;
                lastPos = default(Point);
                currPos = default(Point);
                DesignerRoot.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
            
        }

        bool isPanModeEnabled;
        private void TogglePanMode(object sender, RoutedEventArgs e)
        {
            this.isPanModeEnabled = !this.isPanModeEnabled;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            this.TransformX = 0;
            this.TransformY = 0;
            this.ScaleX = 1;
            this.ScaleY = 1;
        }      
    }
}
