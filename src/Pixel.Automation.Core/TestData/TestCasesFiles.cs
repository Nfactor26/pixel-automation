using Dawn;
using System.IO;

namespace Pixel.Automation.Core.TestData;

/// <summary>
/// Each test case has essentially these three files. There can be additional scripts associated with components belonging to process.
/// </summary>
/// <param name="TestFile"></param>
/// <param name="ProcessFile"></param>
/// <param name="ScriptFile"></param>
public class TestCaseFiles
{
    public string TestDirectory { get; private set; }

    public string TestFile { get; private set; }

    public string ProcessFile { get; private set; }

    public string ScriptFile { get; private set; }

    public TestCaseFiles(TestCase testCase, string workingDirectory)
    {
        Guard.Argument(testCase).NotNull();
        Guard.Argument(workingDirectory).NotNull().NotEmpty().NotWhiteSpace();
        TestDirectory = Path.Combine(workingDirectory, testCase.FixtureId, testCase.TestCaseId);
        if (!Directory.Exists(TestDirectory))
        {
            Directory.CreateDirectory(TestDirectory);
        }
        TestFile = Path.Combine(TestDirectory, $"{testCase.TestCaseId}.test");
        ProcessFile = Path.Combine(TestDirectory, $"{testCase.TestCaseId}.proc");
        ScriptFile = Path.Combine(TestDirectory, $"{testCase.TestCaseId}.csx");
    }
}
