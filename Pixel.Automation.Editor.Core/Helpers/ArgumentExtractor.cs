using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Helpers
{
   
    public class ArgumentExtractor : IArgumentExtractor
    {

        public IEnumerable<Argument> ExtractArguments(Entity entity)
        {
            List<Argument> extractedArguments = new List<Argument>();

            var allComponents = entity.GetAllComponents();          
            foreach (var component in allComponents)
            {
                if (component is GroupEntity groupEntity && (groupEntity.GroupActor != null))
                {
                    extractedArguments.AddRange(AddArgumentsForComponent(groupEntity.GroupActor));
                    continue;
                }
                extractedArguments.AddRange(AddArgumentsForComponent(component));
            }

            return extractedArguments;
           
        }

        public IEnumerable<Argument> ExtractArguments(IComponent component)
        {
            List<Argument> extractedArguments = new List<Argument>();           
            extractedArguments.AddRange(AddArgumentsForComponent(component));
            return extractedArguments;

        }

        IEnumerable<Argument> AddArgumentsForComponent(IComponent component)
        {
            foreach (var argument in GetArguments(component))
            {
                if (argument != null)
                {
                    yield return argument;
                }
            }
        }

        IEnumerable<Argument> GetArguments(IComponent component)
        {
            var componentProperties = component.GetType().GetProperties();
            foreach (var property in componentProperties)
            {
                if (property.PropertyType.Equals(typeof(Argument)) || property.PropertyType.IsSubclassOf(typeof(Argument)))
                {
                    yield return property.GetValue(component) as Argument;
                }
            }
        }
    }
}
