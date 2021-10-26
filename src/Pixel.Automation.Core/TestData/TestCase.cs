using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{

    [DataContract]
    [Serializable]
    [FileDescription("test")]
    public class TestCase : ICloneable
    {
        /// <summary>
        /// Identifier of the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Identified o the fixture to which test case belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string FixtureId { get; set; }

        /// <summary>
        /// Display name for the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Order within a fixture in which test case should be executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public int Order { get; set; }

        /// <summary>
        /// Indicates whether a test case is muted.
        /// Muted test cases are ignored during execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public bool IsMuted { get; set; }

        /// <summary>
        /// Controls the delay for pre and post run of actors.
        /// </summary>
        [DataMember(IsRequired = true, Order = 60)]
        public int DelayFactor { get; set; } = 3;

        /// <summary>
        /// Priority for the test case. This can be used for filtering test cases during execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 70)]
        public Priority Priority { get; set; }

        /// <summary>
        /// Script file for the test case. This script file can be used for any temporary variables required 
        /// during test execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 80)]
        public string ScriptFile { get; set; }

        /// <summary>
        /// Identifier for the Test data souce for the test case
        /// </summary>
        [DataMember(IsRequired = false, Order = 90)]
        public string TestDataId { get; set; }

        /// <summary>
        /// Description for the test case
        /// </summary>
        [DataMember(IsRequired = false, Order = 100)]
        public string Description { get; set; }

        /// <summary>
        /// User defined key value pair that can be associated with test case.
        /// This can be used for filtering test cases during execution..
        /// </summary>
        [DataMember(IsRequired = true, Order = 110)]
        public TagCollection Tags { get; private set; } = new TagCollection();
       
        /// <summary>
        /// Store a reference to any prefab that has been used in this test case.
        /// </summary>
        [DataMember(IsRequired = true, Order = 120)]
        public PrefabReferences PrefabReferences { get; set; } = new PrefabReferences();

        /// <summary>
        /// Root entity for the test case
        /// </summary>
        public Entity TestCaseEntity { get; set; }

        ///</inheritdoc>
        public object Clone()
        {
            TestCase copy = new TestCase()
            {
                DisplayName = this.DisplayName,
                Description = this.Description,
                Tags = new TagCollection(this.Tags.Tags),
                IsMuted = this.IsMuted,
                Order = this.Order,
                DelayFactor = this.DelayFactor,
                Priority = this.Priority,
                PrefabReferences = PrefabReferences
            };
            return copy;
        }

        ///</inheritdoc>
        public override string ToString()
        {
            return $"{Id}";
        }
    }

}
