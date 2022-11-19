namespace Pixel.Persistence.Core.Request;

public class DeleteProjectFileRequest
{
    public string ProjectId { get; set; }

    public string ProjectVersion { get; set; }

    public string FileName { get; set; }
}
