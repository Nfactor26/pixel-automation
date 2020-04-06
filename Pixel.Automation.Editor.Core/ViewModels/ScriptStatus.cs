namespace Pixel.Automation.Editor.Core.ViewModels
{
    public class ScriptStatus
    {
        public string ScriptName { get; set; }

        public bool IsValid { get; private set; }

        public string Diagnostics { get; private set; }

        public void UpdateStatus(bool isValid, string errors)
        {
            this.IsValid = isValid;
            this.Diagnostics = errors;
        }
    }
}
