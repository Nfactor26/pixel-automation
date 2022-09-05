using Pixel.Automation.Core.Interfaces;
using System.IO;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Implementation of <see cref="ITestCaseFileSystem"/>
    /// </summary>
    public class TestCaseFileSystem : ITestCaseFileSystem
    {
        /// <InheritDoc/>       
        public string WorkingDirectory { get; private set; }

        /// <InheritDoc/>      
        public string FixtureDirectory { get; private set; }

        /// <InheritDoc/>      
        public string FixtureFile { get; private set; }

        /// <InheritDoc/>      
        public string FixtureProcessFile { get; private set; }

        /// <InheritDoc/>      
        public string FixtureScript { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public TestCaseFileSystem()
        {

        }

        /// <InheritDoc/>      
        public void Initialize(string projectWorkingDirectory, string testFixtureId)
        {
            this.WorkingDirectory = projectWorkingDirectory;
            this.FixtureDirectory = Path.Combine(this.WorkingDirectory, Constants.TestCasesDirectory, testFixtureId);
            this.FixtureFile = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.fixture");
            this.FixtureProcessFile = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.proc");
            this.FixtureScript = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.csx");
                      
            if (!Directory.Exists(FixtureDirectory))
            {
                Directory.CreateDirectory(FixtureDirectory);
            }           
        }

        /// <InheritDoc/>      
        public string GetTestCaseFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.test");
        }

        /// <InheritDoc/>      
        public string GetTestProcessFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.proc"); ;
        }

        /// <InheritDoc/>      
        public string GetTestScriptFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.csx"); ;
        }

        /// <InheritDoc/>      
        public void DeleteTestCase(string testCaseId)
        {
            File.Delete(GetTestCaseFile(testCaseId));
            File.Delete(GetTestProcessFile(testCaseId));
            File.Delete(GetTestScriptFile(testCaseId));
        }
    }
}