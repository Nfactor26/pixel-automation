using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Native.Windows;

/// <summary>
/// Implementation of <see cref="IImageManager"/> for windows platform
/// </summary>
public class ImageManager : IImageManager
{  
    /// </inheritdoc>   
    public void SaveAs(byte[] imageBytes, string saveLocation, ImageFormat format)
    {
        using (var memoryStream = new MemoryStream(imageBytes))
        {
            using (var image = Image.Load(memoryStream))
            {
                switch (format)
                {
                    case ImageFormat.Png:
                        image.SaveAsPng(saveLocation);
                        break;
                    case ImageFormat.Jpeg:
                        image.SaveAsJpeg(saveLocation);
                        break;
                }
            }
        }
    }

    /// </inheritdoc>   
    public async Task SaveAsAsync(byte[] imageBytes, string saveLocation, ImageFormat format)
    {
        using (var memoryStream = new MemoryStream(imageBytes))
        {
            using (var image = Image.Load(memoryStream))
            {
                switch (format)
                {
                    case ImageFormat.Png:
                        await image.SaveAsPngAsync(saveLocation);
                        break;
                    case ImageFormat.Jpeg:
                        await image.SaveAsJpegAsync(saveLocation);
                        break;
                }
            }
        }
    }
}