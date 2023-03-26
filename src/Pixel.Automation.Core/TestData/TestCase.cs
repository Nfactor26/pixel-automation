using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Automation.Core.TestData
{

    [DataContract]
    [Serializable]
    [FileDescription("test")]
    public class TestCase : ICloneable
    {
        /// <summary>
        /// Identified o the fixture to which test case belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string FixtureId { get; set; }

        /// <summary>
        /// Identifier of the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string TestCaseId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Identifier for the Test data souce for the test case
        /// </summary>
        [DataMember(IsRequired = false, Order = 30)]
        public string TestDataId { get; set; }

        /// <summary>
        /// Display name for the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Order within a fixture in which test case should be executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 5)]
        public int Order { get; set; }

        /// <summary>
        /// Indicates whether a test case is muted.
        /// Muted test cases are ignored during execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 60)]
        public bool IsMuted { get; set; }

        /// <summary>
        /// Amount of delay in ms after each actor is executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 70)]
        public int PostDelay { get; set; } = 100;

        /// <summary>
        /// Scaling factor for post delay amount.       
        /// </summary>
        [DataMember(IsRequired = true, Order = 80)]
        public int DelayFactor { get; set; } = 3;            
     
        /// <summary>
        /// Priority for the test case. This can be used for filtering test cases during execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 90)]
        public Priority Priority { get; set; }

        /// <summary>
        /// Script file for the test case. This script file can be used for any temporary variables required 
        /// during test execution.
        /// </summary>
        [DataMember(IsRequired = true, Order = 100)]
        public string ScriptFile { get; set; }     

        /// <summary>
        /// Description for the test case
        /// </summary>
        [DataMember(IsRequired = false, Order = 110)]
        public string Description { get; set; }

        /// <summary>
        /// User defined key value pair that can be associated with test case.
        /// This can be used for filtering test cases during execution..
        /// </summary>
        [DataMember(IsRequired = true, Order = 120)]
        public TagCollection Tags { get; private set; } = new TagCollection();

        /// <summary>
        /// Collection of Identifiers of controls used by the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 130)]
        public List<ControlUsage> ControlsUsed { get; private set; } = new();

        /// <summary>
        /// Collection of Identifiers of Prefabs used by the test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 140)]
        public List<PrefabUsage> PrefabsUsed { get; private set; } = new();

        /// <summary>
        /// Indicates if the TestCase is deleted. Deleted test cases are not loaded in explorer.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1000)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Root entity for the test case
        /// </summary>
        [JsonIgnore]
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
                Priority = this.Priority
            };
            return copy;
        }

        ///</inheritdoc>
        public override string ToString()
        {
            return $"{TestCaseId}";
        }
    }

}
