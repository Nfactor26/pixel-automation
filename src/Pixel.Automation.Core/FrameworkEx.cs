using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Core
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;
}
