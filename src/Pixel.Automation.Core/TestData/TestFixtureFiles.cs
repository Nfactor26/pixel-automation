using Dawn;
using System.IO;

namespace Pixel.Automation.Core.TestData;

/// <summary>
/// Each Test fixture has essentially  three files. There can be additional scripts associated with components belonging to process.
/// </summary>
public class TestFixtureFiles
{
    public string FixtureDirectory { get; private set; }

    public string FixtureFile { get; private set; }

    public string ProcessFile { get; private set; }

    public string ScriptFile { get; private set; }

    public TestFixtureFiles(TestFixture testFixture, string workingDirectory)
    {
        Guard.Argument(testFixture).NotNull();
        Guard.Argument(workingDirectory).NotNull().NotEmpty().NotWhiteSpace();
        FixtureDirectory = Path.Combine(workingDirectory, testFixture.FixtureId);
        if(!Directory.Exists(FixtureDirectory))
        {
            Directory.CreateDirectory(FixtureDirectory);
        }
        FixtureFile = Path.Combine(FixtureDirectory, $"{testFixture.FixtureId}.fixture");
        ProcessFile = Path.Combine(FixtureDirectory, $"{testFixture.FixtureId}.proc");
        ScriptFile = Path.Combine(FixtureDirectory, $"{testFixture.FixtureId}.csx");
    }
}


