using System;
using System.IO;
using System.Reflection;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public class CompilationResult : IDisposable
    {       
        public string OutputAssemblyName { get; }

        public MemoryStream AssemblyStream { get; private set; }

        public MemoryStream PdbStream { get; private set; }       
        
        public CompilationResult(string outputAssemblyName, MemoryStream assemblyStream, MemoryStream pdbStream)
        {           
            this.OutputAssemblyName = $"{outputAssemblyName}.dll";
            this.AssemblyStream = assemblyStream;
            this.PdbStream = pdbStream;
        }

        private Assembly assembly;
        public Assembly LoadAndGetInMemoryAssembly()
        {         
            if(assembly == null)
            {
                assembly = Assembly.Load(AssemblyStream.ToArray(), PdbStream?.ToArray());
            }
            return assembly;
        }

        public void SaveAssemblyToDisk(string saveAtLocation)
        {
            if (!Directory.Exists(saveAtLocation))
            {
                throw new DirectoryNotFoundException($"{saveAtLocation} doesn't exist");
            }

            string targetDll = Path.Combine(saveAtLocation, OutputAssemblyName);
            string targetPdb = Path.Combine(saveAtLocation, $"{Path.GetFileNameWithoutExtension(OutputAssemblyName)}.pdb");         
            File.WriteAllBytes(targetDll, AssemblyStream.ToArray());
            if(PdbStream != null)
            {
                File.WriteAllBytes(targetPdb, PdbStream.ToArray());
            }
        }       

        ~CompilationResult()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            AssemblyStream?.Dispose();
            PdbStream?.Dispose();
            AssemblyStream = null;
            PdbStream = null;
            assembly = null;
            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }

        }
    }
}
