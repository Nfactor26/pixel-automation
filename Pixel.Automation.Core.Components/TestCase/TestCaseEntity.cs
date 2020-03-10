using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("TestCase", "Test Entities", iconSource: null, description: "An Entity that contains set of components that represent a test case ", tags: new string[] { "Test" })]

    public class TestCaseEntity : Entity//, IScopedEntity
    {

        [DataMember(IsRequired = true)]
        [Description("Test data collection for test case. Test will be repeated for each test data in this collection.")]       
        [Display(Name = "Data Source", Order = 20, GroupName = "Test Data")]
        public Argument DataSource { get; set; } = new InArgument<IEnumerable<object>>() { CanChangeType = true, Mode = ArgumentMode.DataBound, CanChangeMode = true };

        [DataMember(IsRequired = true)]
        [Description("Test data for current iteration")]        
        [Display(Name = "Current", Order = 20, GroupName = "Test Data")]
        public Argument Current { get; set; } = new InArgument<object> { CanChangeType = true, Mode = ArgumentMode.DataBound, CanChangeMode = true };

        public TestCaseEntity() : base("Test Case","TestCase")
        {
            
        }

        #region IScopedEntity

        private Dictionary<string, IEnumerable<string>> argumentPropertiesInfo = new Dictionary<string, IEnumerable<string>>();

        private Type localScopeType = typeof(object);

        public IEnumerable<string> GetPropertiesOfType(Type propertyType)
        {

            if(this.Current.GetArgumentType() != localScopeType)
            {
                localScopeType = this.Current.GetArgumentType();  
                if(localScopeType.Assembly == typeof(int).Assembly)
                {
                    this.argumentPropertiesInfo.Add(localScopeType.Name, new []{ Current.PropertyPath });
                }
                else
                {
                    UpdateArgumentPropertiesInfo();
                }
            }

            if (this.argumentPropertiesInfo.ContainsKey(propertyType.Name))
            {
                return this.argumentPropertiesInfo[propertyType.Name] ?? Enumerable.Empty<string>();
            }
            return Enumerable.Empty<string>();


            void UpdateArgumentPropertiesInfo()
            {
                this.argumentPropertiesInfo.Clear();

                if (this.Current.GetArgumentType() != null)
                {
                    var propertiesGroupedByType = this.Current.GetArgumentType().GetProperties().GroupBy(p => p.PropertyType);
                    foreach (var propertyGroup in propertiesGroupedByType)
                    {
                        this.argumentPropertiesInfo.Add(propertyGroup.Key.Name, propertyGroup.Select(p => p.Name));
                    }
                }
            }
        }

        #endregion IScopedEntity

        public override void ResolveDependencies()
        {          
            if(this.components.Count==0)
            {
                this.AddComponent(new SetUpEntity());
                this.AddComponent(new TestSequenceEntity());
                this.AddComponent(new TearDownEntity());
            }  
           
        }
    }
}
