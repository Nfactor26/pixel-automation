using Newtonsoft.Json;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pixel.Automation.Core.Utilities
{
    public class JsonSerializer : ISerializer
    {
        public T Deserialize<T>(string path, List<Type> knownTypes = null) where T : new()
        {
            try
            {
                Log.Debug("Json - Deserialize enter.");

                var jsonValue = File.ReadAllText(path);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                };
                T myObject = JsonConvert.DeserializeObject<T>(jsonValue, settings);
                return myObject;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                Debug.Assert(false);

            }
            finally
            {
                Log.Debug("Json - Deserialize exit.");
            }
            return default(T);
        }

        public void Serialize<T>(string path, T model, List<Type> knownTypes = null)
        {
            Log.Debug("Json - Serialize enter.");

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                };
                var jsonValue = JsonConvert.SerializeObject(model, typeof(T), Formatting.Indented, settings);
                using (StreamWriter s = new StreamWriter(path, false))
                {
                    s.Write(jsonValue);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                Debug.Assert(false);
            }

            Log.Debug("Json - Serialize end.");

        }
    }
}
