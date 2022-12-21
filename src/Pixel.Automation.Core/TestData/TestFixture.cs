using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    [FileDescription("fixture")]
    public class TestFixture
    {
        /// <summary>
        /// Identifier of the fixture
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string FixtureId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name for the test fixture
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Order of execution of fixture amognst other fixtures
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public int Order { get; set; }

        /// <summary>
        /// Indicates if the fixture is muted. Test case belonging to a muted fixture are skipped.
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public bool IsMuted { get; set; }

        /// <summary>
        /// Initialization script file for the fixture
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public string ScriptFile { get; set; }

        /// <summary>
        /// Description of the fixture
        /// </summary>
        [DataMember(IsRequired = false, Order = 60)]
        public string Description { get; set; }

        /// <summary>
        /// Category of the fixutre. Category can be used to filter test fixture for execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 70)]
        public string Category { get; set; } = "Default";

        /// <summary>
        /// Controls the delay for pre and post run of actors.
        /// </summary>
        [DataMember(IsRequired = true, Order = 80)]
        public int DelayFactor { get; set; } = 3;

        /// <summary>
        /// User defined key value pair that can be associated with test fixture.
        /// This can be used for filtering test fixture during execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 90)]
        public TagCollection Tags { get; private set; } = new TagCollection();

        /// <summary>
        /// Collection of Identifiers of controls used by the test fixture
        /// </summary>
        [DataMember(IsRequired = true, Order = 120)]
        public List<ControlUsage> ControlsUsed { get; private set; } = new();

        /// <summary>
        /// Collection of Identifiers of Prefabs used by the test fixture
        /// </summary>
        [DataMember(IsRequired = true, Order = 130)]
        public List<PrefabUsage> PrefabsUsed { get; private set; } = new();

        /// <summary>
        /// Indicates if the fixture was deleted. Deleted fixtures are not loaded in explorer.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1000)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Collection of tests belonging to a fixture 
        /// </summary>
        [JsonIgnore]
        public List<TestCase> Tests { get; private set; } = new List<TestCase>();

        /// <summary>
        /// Test fixture entity
        /// </summary>
        [JsonIgnore]       
        public Entity TestFixtureEntity { get; set; }

        ///</inheritdoc>
        public object Clone()
        {
            TestFixture copy = new TestFixture()
            {               
                DisplayName = this.DisplayName,
                Order = this.Order,
                IsMuted = this.IsMuted,             
                Description = this.Description,
                Category = this.Category,
                DelayFactor = this.DelayFactor,
                Tags = this.Tags               
            };
            return copy;
        }
        
        ///</inheritdoc>
        public override string ToString()
        {
            return $"{FixtureId}";
        }
    }
}
