namespace Pixel.Persistence.Core.Request
{
    public class GetTemplatesRequest : PagedDataRequest
    {
        public string TemplateFilter { get; set; }
    }
}
