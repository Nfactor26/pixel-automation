﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Scripting.Components.Arguments;
using System;

namespace Pixel.Automation.Scripting.Components.Tests
{
    public class ArgumentProcessorTests
    {
        /// <summary>
        /// Validate that ArgumentProcessor can be constructed
        /// </summary>
        [Test]
        public void CanBeInitialized()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            var argumentProcessor = new ArgumentProcessor(scriptEngine);

            Assert.IsNotNull(argumentProcessor);
        }


        /// <summary>
        /// Validate that constructor throws a ArgumentNullException when null is passed for IScriptEngine argument.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionWhenInitializedWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => { new ArgumentProcessor(null); });
        }
    }
}
