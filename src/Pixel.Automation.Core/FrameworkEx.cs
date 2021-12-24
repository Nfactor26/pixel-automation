using System.Threading.Tasks;

namespace Pixel.Automation.Core
{
   public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
}
