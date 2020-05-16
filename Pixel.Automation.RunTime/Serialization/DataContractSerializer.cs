using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Pixel.Automation.RunTime.Serialization
{   
    public  class XmlSerializer : ISerializer
    {
        private readonly XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates };
       
        /// <summary>
        /// Serialize a model 
        /// </summary>
        /// <typeparam name="T">Type of model</typeparam>
        /// <param name="path">Relative path of xml where data will be serialized</param>
        /// <param name="model">Model to serialize</param>
        /// <exception cref="BusinessModels.XmlSerializationException">ModelSerializationException is thrown when serialization fails</exception>
        public void Serialize<T>(string path, T model, List<Type> knownTypes=null)
        {
            try
            {              
                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    var ds = new DataContractSerializer(typeof(T), knownTypes);
                    ds.WriteObject(writer, model);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }


        /// <summary>
        /// Desirialize data of given Type
        /// </summary>
        /// <typeparam name="T">Type of model</typeparam>
        /// <param name="path">Relative Path of  file ; Relative to application</param>
        /// <returns>Deserialized data</returns>
        /// <exception cref="BusinessExceptions.ModelDeserializationException">ModelDeserializationException is thrown if deserialization is failed</exception>
        public T Deserialize<T>(string path, List<Type> knownTypes = null) where T : new()
        {
            try
            {
                T model;
                using (Stream s = File.OpenRead(path))
                {
                    var ds = new DataContractSerializer(typeof(T), knownTypes);
                    model = (T)ds.ReadObject(s);
                    return model;
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
    }
}

