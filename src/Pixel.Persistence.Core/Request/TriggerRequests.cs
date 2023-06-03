using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Core.Request
{
    public record AddTriggerRequest(string TemplateId, SessionTrigger Trigger);

    public record RunTriggerRequest(string TemplateId, SessionTrigger Trigger);

    public record UpdateTriggerRequest(string TemplateId, SessionTrigger Original, SessionTrigger Updated);
}
