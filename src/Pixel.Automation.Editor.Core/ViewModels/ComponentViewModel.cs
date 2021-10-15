using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Editor.Core.ViewModels
{
    public class ComponentViewModel : PropertyChangedBase
    {       
        public int ProcessOrder
        {
            get => Model.ProcessOrder;
        }

        [System.ComponentModel.Browsable(false)]
        public IComponent Model { get; protected set; }

        protected EntityComponentViewModel parent;
        public EntityComponentViewModel Parent
        {
            get => this.parent;
            set
            {
                this.parent = value;
                this.Model.Parent = this.parent?.Model as Entity;
            }
        }
   
        protected ComponentViewModel()
        {

        }

        public ComponentViewModel(IComponent model, EntityComponentViewModel parent)
        {
            Model = Guard.Argument(model).NotNull().Value;
            Parent = Guard.Argument(parent).NotNull();
        }

    }  
}
