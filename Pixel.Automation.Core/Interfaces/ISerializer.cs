using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ISerializer
    {
        /// <summary>
        /// Serialize the data and save to file at the given path
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the data to be serialized</typeparam>
        /// <param name="path">File where the serialized data will be saved</param>
        /// <param name="model">Instance of type T i.e. object to be serialized</param>
        /// <param name="knownTypes">Collection of known types during serialization process</param>
        void Serialize<T>(string path, T model, List<Type> knownTypes = null);

        /// <summary>
        /// Desirialize data from a given file 
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> to which data will be desirialized</typeparam>
        /// <param name="path">File which contains the serialized data</param>
        /// <param name="knownTypes">Collection of known types during desirialization process</param>
        /// <returns>Desirialized data as instance of <see cref="Type"/> T </returns>
        T Deserialize<T>(string path, List<Type> knownTypes = null) where T : new();

        /// <summary>
        /// Deserialize provided string content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        T DeserializeContent<T>(string content, List<Type> knownTypes = null) where T : new();
    }
}
