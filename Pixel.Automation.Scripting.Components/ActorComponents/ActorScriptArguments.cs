using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Scripting.Components
{
    public class ActorScriptArguments<T> : ScriptArguments<T> where T : new()
    { 
        /// <summary>
        /// Target application to which the Script Actor component belongs
        /// </summary>
        public IApplication Application { get; internal set; }

        /// <summary>
        /// Parent Control Entity of Script Actor component if any
        /// </summary>
        public Entity ControlEntity { get; internal set; }
        
        /// <summary>
        /// EntityManager for locating any additional components
        /// </summary>
        public EntityManager EntityManager { get; internal set; }
       
        public ActorScriptArguments(EntityManager entityManager, IApplication application, Entity controlEntity, T dataModel) : base(dataModel)
        {
            this.EntityManager = entityManager;
            this.Application = application;
            this.ControlEntity = controlEntity;
            
        }    

    }
}
