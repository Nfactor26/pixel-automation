namespace Pixel.Scripting.Editor.Core.Models.FileOperations
{
    public abstract class FileOperationResponse
    {
        public FileOperationResponse(string fileName, FileModificationType type)
        {
            FileName = fileName;
            ModificationType = type;
        }

        public string FileName { get; }

        public FileModificationType ModificationType { get; }
    }
}
