namespace Pixel.Scripting.Editor.Core.Models.FileOperations
{
    public class OpenFileResponse : FileOperationResponse
    {
        public OpenFileResponse(string fileName) : base(fileName, FileModificationType.Opened)
        {
        }
    }
}
