using Pixel.Automation.Core.Interfaces;
using System.IO;

namespace Pixel.Automation.Core
{
    public class TestCaseFileSystem : FileSystem, ITestCaseFileSystem
    {
        public string TestDirectory { get; private set; }

        public string TestCaseFile { get; private set; }

        public string TestProcessFile { get; private set; }


        public TestCaseFileSystem(ISerializer serializer) : base(serializer)
        {

        }

        public void Initialize(string projectWorkingDirectory, string testCaseId)
        {
            this.WorkingDirectory = projectWorkingDirectory;
            this.TestDirectory = Path.Combine(this.WorkingDirectory, "TestCases", testCaseId);
            this.TestCaseFile = Path.Combine(TestDirectory, "TestCase.test");
            this.TestProcessFile = Path.Combine(TestDirectory, "TestAutomation.proc");

            base.Initialize();

            this.ScriptsDirectory = this.TestDirectory;

        }        
    }
}