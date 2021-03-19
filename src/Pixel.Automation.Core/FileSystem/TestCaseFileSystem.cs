using Pixel.Automation.Core.Interfaces;
using System.IO;

namespace Pixel.Automation.Core
{
    public class TestCaseFileSystem : FileSystem, ITestCaseFileSystem
    {
        public string FixtureDirectory { get; private set; }

        public string FixtureFile { get; private set; }


        public string FixtureProcessFile { get; private set; }

        public string FixtureScript { get; private set; }

        public TestCaseFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {

        }

        public void Initialize(string projectWorkingDirectory, string testFixtureId)
        {
            this.WorkingDirectory = projectWorkingDirectory;
            this.FixtureDirectory = Path.Combine(this.WorkingDirectory, "TestCases", testFixtureId);
            this.FixtureFile = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.fixture");
            this.FixtureProcessFile = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.proc");
            this.FixtureScript = Path.Combine(this.FixtureDirectory, $"{testFixtureId}.csx");
            this.ScriptsDirectory = this.FixtureDirectory;
            
            if (!Directory.Exists(FixtureDirectory))
            {
                Directory.CreateDirectory(FixtureDirectory);
            }           
        }

        public string GetTestCaseFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.test");
        }

        public string GetTestProcessFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.proc"); ;
        }

        public string GetTestScriptFile(string testCaseId)
        {
            return Path.Combine(FixtureDirectory, $"{testCaseId}.csx"); ;
        }

      
        public void DeleteTestCase(string testCaseId)
        {
            File.Delete(GetTestCaseFile(testCaseId));
            File.Delete(GetTestProcessFile(testCaseId));
            File.Delete(GetTestScriptFile(testCaseId));
        }
    }
}