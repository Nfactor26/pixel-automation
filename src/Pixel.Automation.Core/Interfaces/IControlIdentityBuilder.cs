namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlIdentityBuilder
    {
        IControlIdentity CreateFromData<T>(T controlData);
    }
}
