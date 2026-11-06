using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces;

/// <summary>
/// Contract for managing image
/// </summary>
public interface IImageManager
{
    /// <summary>
    /// Save PNG image bytes asynchronously at a specified location
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <param name="saveLocation"></param>
    /// <returns></returns>
    public Task SaveAsAsync(byte[] imageBytes, string saveLocation);
}
