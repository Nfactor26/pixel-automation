namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
    public interface ICodeActionRequest
    {       
        int Line { get; }      
        int Column { get; }
        string Buffer { get; }
        string FileName { get; }
        Range Selection { get; }
    }
}
