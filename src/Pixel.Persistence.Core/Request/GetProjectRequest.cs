namespace Pixel.Persistence.Core.Request
{
    public class GetProjectRequest : PagedDataRequest
    {
        public string ProjectFilter { get; set; }
    }

}
