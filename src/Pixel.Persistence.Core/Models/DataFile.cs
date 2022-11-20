namespace Pixel.Persistence.Core.Models
{
    public class DataFile
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string Version { get; set; }

        public string Type { get; set; }

        public byte[] Bytes { get; set; }
    } 

    public class ControlImageDataFile : DataFile
    {
        public string ControlId { get; set; }
    }

    public class ProjectDataFile : DataFile
    {
        public string ProjectId { get; set; }

        public string ProjectVersion { get; set; }

        public string Tag { get; set; }
    }
}
