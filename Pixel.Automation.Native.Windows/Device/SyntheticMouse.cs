using InputSimulatorStandard;
using Pixel.Automation.Core.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using MouseButton = Pixel.Automation.Core.Devices.MouseButton;
using System.Windows.Forms;

namespace Pixel.Automation.Native.Windows.Device
{
    public class SyntheticMouse : ISyntheticMouse
    {
        private readonly object _lock = new object();
        private const int screenScale = 65535;

        //TODO : Resolutions are hard coded. How to get it without Screen from System.Windows.Forms?
        private int resX = Screen.PrimaryScreen.Bounds.Width;
        private int resY = Screen.PrimaryScreen.Bounds.Height;

        private readonly IMouseSimulator mouseSimulator;
        
        public SyntheticMouse()
        {
            this.mouseSimulator = new MouseSimulator();
        }

        public bool IsCriticalResource => true;

        public int MouseWheelClickSize
        {
            get => this.mouseSimulator.MouseWheelClickSize;
            set => this.mouseSimulator.MouseWheelClickSize = value;
        }

        public ISyntheticMouse ButtonDown(MouseButton mouseButton)
        {
            lock(_lock)
            {
                switch (mouseButton)
                {
                    case MouseButton.LeftButton:
                        this.mouseSimulator.LeftButtonDown();
                        break;
                    case MouseButton.RightButton:
                        this.mouseSimulator.RightButtonDown();
                        break;
                    case MouseButton.MiddleButton:
                        this.mouseSimulator.MiddleButtonDown();
                        break;
                    default:
                        throw new ArgumentException($"{mouseButton} for ButtonDown is not supported");
                }
                return this;
            }          
        }

        public ISyntheticMouse ButtonUp(MouseButton mouseButton)
        {
            lock(_lock)
            {
                switch (mouseButton)
                {
                    case MouseButton.LeftButton:
                        this.mouseSimulator.LeftButtonUp();
                        break;
                    case MouseButton.RightButton:
                        this.mouseSimulator.RightButtonUp();
                        break;
                    case MouseButton.MiddleButton:
                        this.mouseSimulator.MiddleButtonUp();
                        break;
                    default:
                        throw new ArgumentException($"{mouseButton} for ButtonUp is not supported");
                }
                return this;
            }           
        }

        public ISyntheticMouse Click(MouseButton mouseButton)
        {
            lock(_lock)
            {
                switch (mouseButton)
                {
                    case MouseButton.LeftButton:
                        this.mouseSimulator.LeftButtonClick();
                        break;
                    case MouseButton.RightButton:
                        this.mouseSimulator.RightButtonClick();
                        break;
                    case MouseButton.MiddleButton:
                        this.mouseSimulator.MiddleButtonClick();
                        break;
                    default:
                        throw new ArgumentException($"{mouseButton} for Click is not supported");
                }
                return this;
            }            
        }

        public ISyntheticMouse DoubleClick(MouseButton mouseButton)
        {
            lock(_lock)
            {
                switch (mouseButton)
                {
                    case MouseButton.LeftButton:
                        this.mouseSimulator.LeftButtonDoubleClick();
                        break;
                    case MouseButton.RightButton:
                        this.mouseSimulator.RightButtonDoubleClick();
                        break;
                    case MouseButton.MiddleButton:
                        this.mouseSimulator.MiddleButtonDoubleClick();
                        break;
                    default:
                        throw new ArgumentException($"{mouseButton} for DoubleClick is not supported");
                }
                return this;
            }
           
        }       

        public ISyntheticMouse MoveMouseBy(int xCoordinate, int yCoordinate, SmoothMode smoothMode)
        {
            lock(_lock)
            {
                var cursorPos = GetCursorPosition();
                ScreenCoordinate finalStop = new ScreenCoordinate()
                {
                    XCoordinate = cursorPos.X + xCoordinate,
                    YCoordinate = cursorPos.Y + yCoordinate
                };

                foreach (var stop in GetNextStop(finalStop, smoothMode))
                {
                    int nextX = (stop.XCoordinate * screenScale) / (resX);
                    int nextY = (stop.YCoordinate * screenScale) / (resY);

                    this.mouseSimulator.MoveMouseTo(nextX, nextY);
                }            
                return this;
            }           
        }

        public ISyntheticMouse MoveMouseTo(ScreenCoordinate screenCoordinate, SmoothMode smoothMode)
        {
            lock(_lock)
            {
                foreach(var stop in GetNextStop(screenCoordinate, smoothMode))
                {
                    int nextX = (stop.XCoordinate * screenScale) / (resX);
                    int nextY = (stop.YCoordinate * screenScale) / (resY);

                    this.mouseSimulator.MoveMouseTo(nextX, nextY);
                }           
                return this;
            }          
        }

        public ISyntheticMouse HorizontalScroll(int scrollAmountInClicks)
        {
            lock(_lock)
            {
                this.mouseSimulator.HorizontalScroll(scrollAmountInClicks);
                return this;
            }
          
        }

        public ISyntheticMouse VerticalScroll(int scrollAmountInClicks)
        {
            lock(_lock)
            {
                this.mouseSimulator.VerticalScroll(scrollAmountInClicks);
                return this;
            }           
        }

        public Point GetCursorPosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        protected IEnumerable<ScreenCoordinate> GetNextStop(ScreenCoordinate destination, SmoothMode smoothMode)
        {
            switch(smoothMode)
            {
                case SmoothMode.None:
                    yield return destination;
                    yield break;
                case SmoothMode.Interpolated:
                    {
                        Point cursorPosition = GetCursorPosition();
                        //TODO : Always using 25 stops seems like a bad idea. For larger movements, this should be more..Use better approach to dynamically calculate number of stops
                        int intervals = 1; 
                        while (intervals <= 25)
                        {
                            yield return new ScreenCoordinate()
                            {
                                XCoordinate = cursorPosition.X + (destination.XCoordinate - cursorPosition.X) * intervals / 25,
                                YCoordinate = cursorPosition.Y + (destination.YCoordinate - cursorPosition.Y) * intervals / 25
                            };
                            Thread.Sleep(20);
                            intervals++;
                        }
                    }
                    yield break;
            }           
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
        

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };
    }
}
