using Pixel.Automation.Core.Arguments;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IServiceResolver : IDisposable
    {
        T Get<T>(string key = null);      
       
        IEnumerable<T> GetAll<T>();

        void RegisterDefault<T>(T instance) where T : class;      
    }
}
