namespace Pixel.Scripting.Editor.Core.Models.CodeActions
{
    public class EditorCodeAction
    {
        public EditorCodeAction(string identifier, string name)
        {
            Identifier = identifier;
            Name = name;
        }

        public string Identifier { get; }
        public string Name { get; }
    }
}
