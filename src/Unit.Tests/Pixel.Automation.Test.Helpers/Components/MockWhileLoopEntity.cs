using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Test.Helpers
{
    [Builder(typeof(MockBuilder))]
    public class MockWhileLoopEntity : Entity, ILoop, IScopedEntity
    {
        public MockWhileLoopEntity() : base()
        {

        }

        public MockWhileLoopEntity(string name, string tag) : base(name, tag)
        {

        }

        public bool ExitCriteriaSatisfied { get; set; }

        public IEnumerable<string> GetPropertiesOfType(Type propertyType)
        {
            throw new NotImplementedException();
        }

        public string GetScopedArgumentName()
        {
            throw new NotImplementedException();
        }

        public object GetScopedTypeInstance()
        {
            throw new NotImplementedException();
        }
    }

    public class MockBuilder
    {

    }
}
