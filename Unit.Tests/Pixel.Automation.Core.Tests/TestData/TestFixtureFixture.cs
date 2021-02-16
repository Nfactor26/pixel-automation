using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixel.Automation.Core.Tests.TestData
{
    class TestFixtureFixture
    {
        [Test]
        public void ValidateThatTestFixtureCanBeInitialized()
        {
            var testFixture = new TestFixture();

            Assert.IsFalse(string.IsNullOrEmpty(testFixture.Id));
            Assert.IsNull(testFixture.DisplayName);
            Assert.AreEqual(0, testFixture.Order);
            Assert.IsFalse(testFixture.IsMuted);
            Assert.IsNull(testFixture.ScriptFile);
            Assert.IsNull(testFixture.Description);
            Assert.IsNotNull(testFixture.Tags);
            Assert.AreEqual("Default", testFixture.Group);
            Assert.IsNotNull(testFixture.Tests);
            Assert.IsNull(testFixture.TestFixtureEntity);
            Assert.AreEqual(testFixture.Id, testFixture.ToString());

            testFixture.DisplayName = "TestFixture";
            testFixture.Order = 10;
            testFixture.IsMuted = true;
            testFixture.ScriptFile = "TestFixture.csx";
            testFixture.Description = "Description";         
            testFixture.Tags.Add("Tag1");
            testFixture.Tests.Add(new TestCase());
            testFixture.TestFixtureEntity = new Entity();

       
            Assert.AreEqual("TestFixture", testFixture.DisplayName);
            Assert.AreEqual(10, testFixture.Order);
            Assert.IsTrue(testFixture.IsMuted);
            Assert.AreEqual("TestFixture.csx", testFixture.ScriptFile);
            Assert.AreEqual("Description", testFixture.Description);
            Assert.IsTrue(testFixture.Tags.Any());
            Assert.IsTrue(testFixture.Tests.Any());
            Assert.IsNotNull(testFixture.TestFixtureEntity);
        }

        [Test]
        public void ValidateThatTestFixtureCanBeCloned()
        {
            var testFixture = new TestFixture() { DisplayName = "TestCase", Order = 10, IsMuted = true, ScriptFile = "ScriptFile.csx", 
                Description = "Description", Group = "GroupOne", TestFixtureEntity = new Entity() };
            testFixture.Tags.Add("Tag1");
            testFixture.Tests.Add(new TestCase());
            var copyOftestFixture = testFixture.Clone() as TestFixture;

            Assert.AreEqual(testFixture.DisplayName, copyOftestFixture.DisplayName);
            Assert.AreEqual(testFixture.Order, copyOftestFixture.Order);
            Assert.AreEqual(testFixture.Description, copyOftestFixture.Description);
            Assert.AreEqual(testFixture.IsMuted, copyOftestFixture.IsMuted);
            Assert.IsTrue(testFixture.Tags.SequenceEqual(copyOftestFixture.Tags));
            Assert.AreEqual(testFixture.Group, copyOftestFixture.Group);          
            Assert.AreNotEqual(testFixture.ScriptFile, copyOftestFixture.ScriptFile);      
            Assert.AreNotEqual(testFixture.TestFixtureEntity, copyOftestFixture.TestFixtureEntity);
            Assert.IsFalse(testFixture.Tests.SequenceEqual(copyOftestFixture.Tests));

        }
    }
}
