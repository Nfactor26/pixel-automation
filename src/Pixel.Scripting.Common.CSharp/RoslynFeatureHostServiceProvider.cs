using System.Collections.Immutable;
using System.Composition;
using System.Reflection;

namespace Pixel.Scripting.Common.CSharp
{
    public interface IHostServicesProvider
    {
        ImmutableArray<Assembly> Assemblies { get; }
    }

    [Shared]
    [Export(typeof(IHostServicesProvider))]
    [Export(typeof(RoslynFeaturesHostServicesProvider))]
    public class RoslynFeaturesHostServicesProvider : IHostServicesProvider
    {
        public ImmutableArray<Assembly> Assemblies { get; }

        [ImportingConstructor]
        public RoslynFeaturesHostServicesProvider(IAssemblyLoader loader)
        {
            var builder = ImmutableArray.CreateBuilder<Assembly>();
            builder.AddRange(loader.Load(Configuration.RoslynWorkspaces, Configuration.RoslynCSharpWorkspaces, Configuration.RoslynFeatures, Configuration.RoslynCSharpFeatures));
            Assemblies = builder.ToImmutable();
        }
    }
}
