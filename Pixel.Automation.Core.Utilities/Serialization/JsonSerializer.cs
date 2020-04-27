﻿using Newtonsoft.Json;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.Core.Utilities
{
    public class JsonSerializer : ISerializer
    {
        private readonly ILogger logger = Log.ForContext<JsonSerializer>();
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
        };

        public T Deserialize<T>(string path, List<Type> knownTypes = null) where T : new()
        {
            if(File.Exists(path))
            {
                var jsonContent = File.ReadAllText(path);
                return DeserializeContent<T>(jsonContent, knownTypes);
            }
            throw new FileNotFoundException($"File {path} doesn't exist");
        }

        public T DeserializeContent<T>(string content, List<Type> knownTypes = null) where T : new()
        {
            try
            {               
                T myObject = JsonConvert.DeserializeObject<T>(content, settings);
                return myObject;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }           
            return default;
        }

        public void Serialize<T>(string path, T model, List<Type> knownTypes = null)
        {
            try
            {                
                var jsonValue = JsonConvert.SerializeObject(model, typeof(T), Formatting.Indented, settings);
                using (StreamWriter s = new StreamWriter(path, false))
                {
                    s.Write(jsonValue);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);             
            }        
        }
    }
}
