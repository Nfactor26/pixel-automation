using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Editor.Controls.Prefabs
{
    /// <summary>
    /// Interaction logic for InputMappingButton.xaml
    /// </summary>
    public partial class InputMappingButton : MappingButton
    {
        public InputMappingButton()
        {
            InitializeComponent();
        }

        protected override IPrefabArgumentMapper GetArgumentMapper()
        {
            return new PrefabInputMapper();
        }       
    }
}
