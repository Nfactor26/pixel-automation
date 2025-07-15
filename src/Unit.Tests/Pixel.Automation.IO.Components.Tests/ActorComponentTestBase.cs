using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.IO.Components.Tests;

public class ActorComponentTestBase
{
    protected string tempDirectory = string.Empty;
    protected IEntityManager entityManager = Substitute.For<IEntityManager>();
    protected IArgumentProcessor argumentProcessor = Substitute.For<IArgumentProcessor>();

    [SetUp]
    public void Setup()
    {
        tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(tempDirectory);       
     
        entityManager.GetArgumentProcessor().Returns(argumentProcessor);
        argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(a => a.ArgAt<InArgument<string>>(0).DefaultValue);
        argumentProcessor.GetValueAsync<bool>(Arg.Any<InArgument<bool>>()).Returns(a => a.ArgAt<InArgument<bool>>(0).DefaultValue);
        argumentProcessor.GetValueAsync<FileAttributes>(Arg.Any<InArgument<FileAttributes>>()).Returns(a => a.ArgAt<InArgument<FileAttributes>>(0).DefaultValue);
        argumentProcessor.GetValueAsync<EnumerationOptions>(Arg.Any<InArgument<EnumerationOptions>>()).Returns(a => a.ArgAt<InArgument<EnumerationOptions>>(0).DefaultValue);
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Remove read-only attribute from all files in tempDirectory to avoid UnauthorizedAccessException
        if (!string.IsNullOrEmpty(tempDirectory) && Directory.Exists(tempDirectory))
        {
            foreach (var file in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
            {
                try { File.SetAttributes(file, FileAttributes.Normal); } catch { }
            }
            try { Directory.Delete(tempDirectory, true); } catch { }
        }
        argumentProcessor.ClearReceivedCalls();
    }
}
