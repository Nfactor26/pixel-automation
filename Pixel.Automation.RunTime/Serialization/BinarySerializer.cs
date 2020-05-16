using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pixel.Automation.RunTime.Serialization
{
    public class BinarySerializer : ISerializer
    {
        public T Deserialize<T>(string path, List<Type> knownTypes = null) where T : new()
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    T myObject = (T)formatter.Deserialize(stream);
                    return myObject;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            return default;
        }

        public T DeserializeContent<T>(string content, List<Type> knownTypes = null) where T : new()
        {
            throw new NotImplementedException();
        }

        public void Serialize<T>(string path, T model, List<Type> knownTypes = null)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, model);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

        }
    }
}
