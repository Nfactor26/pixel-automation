using Pixel.Automation.Core.Enums;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces;

/// <summary>
/// Contract for managing image
/// </summary>
public interface IImageManager
{
    /// <summary>
    /// Save image at a specified location in specified format
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <param name="saveLocation"></param>
    /// <param name="format"></param>
    public void SaveAs(byte[] imageBytes, string saveLocation, ImageFormat format);

    /// <summary>
    /// Save image asynchronously at a specified location in specified format
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <param name="saveLocation"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public Task SaveAsAsync(byte[] imageBytes, string saveLocation, ImageFormat format);
}
