namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Defines a contract for intializers that can be used to initialize a component after it is created.
    /// </summary>
    public interface IComponentInitializer
    {
        void IntializeComponent(IComponent component, IEntityManager entityManager);
    }
}
