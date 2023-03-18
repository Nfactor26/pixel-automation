using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for configuring the <see cref="PathSegment"/>
/// </summary>
public class PathSegmentsConfigurationViewModel : Screen, IConfigurationScreen
{
    /// <summary>
    /// Actor component for exeucting the rest request
    /// </summary>
    public Component OwnerComponent { get; set; }

    /// <summary>
    /// Collection of PathSegments visible on screen
    /// </summary>
    public BindableCollection<PathSegment> PathSegments { get; set; } = new();
    
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="pathSegments"></param>
    public PathSegmentsConfigurationViewModel(Component ownerComponent, List<PathSegment> pathSegments)
    {
        this.DisplayName = "Path Segments";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent));
        Guard.Argument(pathSegments, nameof(pathSegments)).NotNull();
        if(pathSegments.Any())
        {
            this.PathSegments.AddRange(pathSegments);
        }
        else
        {
            this.AddPathSegment();
        }
    }

    /// <summary>
    /// Add a new Path segment
    /// </summary>
    public void AddPathSegment()
    {
        this.PathSegments.Add(new PathSegment());
    }

    /// <summary>
    /// Delete path segment
    /// </summary>
    /// <param name="pathSegment"></param>
    public void DeletePathSegment(PathSegment pathSegment)
    {
        this.PathSegments.Remove(pathSegment);
    }

    /// </inheritdoc>
    public void ApplyChanges(RestApi.Shared.HttpRequest request)
    {
        request.PathSegments.Clear();
        if (this.PathSegments.Any())
        {
            request.PathSegments.AddRange(this.PathSegments);
        }
    }
}
