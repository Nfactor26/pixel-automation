namespace Pixel.Persistence.Core.Request;

public class GetHandlersRequest : PagedDataRequest
{
    public string HandlerFilter { get; set; }
}
