using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Pixel.Automation.RunTime
{
    public class KnownTypeProvider : ITypeProvider
    {
        private readonly ILogger logger = Log.ForContext<KnownTypeProvider>();    
        private readonly List<Type> knownTypes = new ();

        #region constructor

        public KnownTypeProvider()
        {   
            LoadAvailableTypes();
        }

        #endregion constructor

        #region public methods

        void LoadAvailableTypes()
        {
            this.knownTypes.Clear();
            foreach (var path in Directory.EnumerateFiles(".", "*.Components.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, path));
                    LoadTypesFromAssembly(assembly);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }
            }
        }

        public void LoadTypesFromAssembly(Assembly assembly)
        {           
            foreach (Type t in assembly.DefinedTypes)
            {
                if (t.IsPublic && !t.IsAbstract)
                {
                    if(!this.knownTypes.Contains(t))
                    {
                        this.knownTypes.Add(t);
                    }                    
                }
            }               
        }      

        public List<Type> GetKnownTypes()
        {
            return this.knownTypes;
        }

        #endregion public methods
       
    }
}
