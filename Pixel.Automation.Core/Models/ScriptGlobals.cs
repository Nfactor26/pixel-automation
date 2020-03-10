namespace Pixel.Automation.Core.Models
{
    public abstract class ScriptGlobals
    {
        public EntityManager EntityManager { get; set; }

        public ScriptGlobals(EntityManager entityManager)
        {
            this.EntityManager = entityManager;           
        }

        public abstract object GetArgumentsObject();

    }

    public class ScriptGlobals<T> : ScriptGlobals where T : class
    {
        public T Arguments { get; set; }

        public ScriptGlobals(EntityManager entityManager, T arguments) : base(entityManager)
        {
            this.Arguments = arguments;
        }

        public override object GetArgumentsObject()
        {
            return Arguments;
        }
    }
}
