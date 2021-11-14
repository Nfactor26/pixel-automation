namespace Pixel.Automation.Core.Interfaces;

/// <summary>
/// Interface for a component 
/// </summary>
public interface IComponent
{

    /// <summary>
    /// Parent Entity of the component
    /// </summary>
    Entity Parent
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the EntityManager for the component
    /// </summary>
    IEntityManager EntityManager
    {
        get;
        set;
    }


    /// <summary>
    /// Unique Id of the component
    /// </summary>
    string Id
    {
        get;
    }

    /// <summary>
    /// Name assigned to this component
    /// </summary>
    string Name
    {
        get;
        set;
    }

    /// <summary>
    /// A tag that can be used to identify this component
    /// </summary>
    string Tag
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates whether this component is enabled or not. A disabled component is not processed.
    /// </summary>
    bool IsEnabled
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates the order  amongst its sibling components at which this will be executed
    /// </summary>
    int ProcessOrder
    {
        get;
        set;
    }

    /// <summary>
    /// Validate if the component is configured properly.
    /// </summary>
    /// <returns></returns>
    bool ValidateComponent();

    /// <summary>
    /// Reset any changes made to component during automation so that it can be reused again 
    /// </summary>
    /// <returns></returns>
    void ResetComponent();

    /// <summary>
    /// Add any required components at appropriate places in the entity component hierarchy
    /// </summary>
    void ResolveDependencies();
}