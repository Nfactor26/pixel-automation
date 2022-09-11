using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
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
           
        }

        #endregion constructor

        #region public methods

        public void LoadTypesFromAssembly(Assembly assembly)
        {
            try
            {
                foreach (Type t in assembly.DefinedTypes)
                {
                    if (t.IsPublic && !t.IsAbstract)
                    {
                        if (!this.knownTypes.Contains(t))
                        {
                            this.knownTypes.Add(t);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An exception occured while trying to load types from assembly {0}", assembly.FullName);
                throw;
            }           
        }      

        public List<Type> GetKnownTypes()
        {
            return this.knownTypes;
        }

        #endregion public methods
       
    }
}
