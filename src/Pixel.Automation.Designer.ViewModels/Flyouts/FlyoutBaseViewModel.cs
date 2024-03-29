﻿using Caliburn.Micro;
using MahApps.Metro.Controls;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Designer.ViewModels.Flyouts
{
    public abstract class FlyoutBaseViewModel : PropertyChangedBase, IFlyOut
    {
        private string header;
        private bool isOpen;
        private Position position;
        private FlyoutTheme theme = FlyoutTheme.Dark;

        public string Header
        {
            get { return this.header; }
            set
            {
                if (value == this.header)
                {
                    return;
                }
                this.header = value;
                this.NotifyOfPropertyChange(() => this.Header);
            }
        }

        public bool IsOpen
        {
            get { return this.isOpen; }
            set
            {
                if (value.Equals(this.isOpen))
                {
                    return;
                }
                this.isOpen = value;
                this.NotifyOfPropertyChange(() => this.IsOpen);
            }
        }

        public Position Position
        {
            get { return this.position; }
            set
            {
                if (value == this.position)
                {
                    return;
                }
                this.position = value;
                this.NotifyOfPropertyChange(() => this.Position);
            }
        }

        public FlyoutTheme Theme
        {
            get { return this.theme; }
            set
            {
                if (value == this.theme)
                {
                    return;
                }
                this.theme = value;
                this.NotifyOfPropertyChange(() => this.Theme);
            }
        }
    }
}