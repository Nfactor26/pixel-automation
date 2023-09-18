using Microsoft.AspNetCore.Components;

namespace Pixel.Automation.Web.Portal.Components.Tests;

public partial class ImageDialog : ComponentBase
{  
    [Parameter] public byte[] imageBytes { get; set; }
}