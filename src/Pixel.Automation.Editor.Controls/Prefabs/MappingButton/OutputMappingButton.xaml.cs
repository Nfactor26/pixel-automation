using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Editor.Controls.Prefabs
{
    /// <summary>
    /// Interaction logic for OutputMappingButton.xaml
    /// </summary>
    public partial class OutputMappingButton : MappingButton
    {        
        public OutputMappingButton()
        {
            InitializeComponent();            
        }

        protected override IPrefabArgumentMapper GetArgumentMapper()
        {
            return new PrefabOutputMapper();
        }       
    }
}

