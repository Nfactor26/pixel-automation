namespace Pixel.Automation.Core.Interfaces
{

    /// <summary>
    /// Defines a contract for builders that can be used to build a specific type of component.
    /// </summary>
    public interface IComponentBuillder
    {
        IComponent CreateComponent();
    }
}
