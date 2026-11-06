using Pixel.Automation.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Native.Linux;

/// <summary>
/// Implementation of <see cref="IImageManager"/> for Linux platform
/// </summary>
public class ImageManager : IImageManager
{
    /// </inheritdoc>   
    public async Task SaveAsAsync(byte[] imageBytes, string saveLocation)
    {
        await File.WriteAllBytesAsync(saveLocation, imageBytes);
    }
}
