namespace Pixel.Persistence.Core.Request;

public class AddProjectFileRequest
{
    public string ProjectId { get; set; }

    public string ProjectVersion { get; set; }

    public string Tag { get; set; }

    public string FileName { get; set; }

    public string FilePath { get; set; }

}
