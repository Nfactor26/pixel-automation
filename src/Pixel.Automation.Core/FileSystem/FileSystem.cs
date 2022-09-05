using Dawn;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Implementation of <see cref="IFileSystem"/>
    /// </summary>
    public abstract class FileSystem : IFileSystem
    {
        protected readonly ISerializer serializer;

        protected readonly ApplicationSettings applicationSettings;

        /// <InheritDoc/>
        public string WorkingDirectory { get; protected set; }

        /// <InheritDoc/>      
        public string ScriptsDirectory { get; protected set; }

        /// <InheritDoc/>
        public string TempDirectory { get; protected set; }

        /// <InheritDoc/>
        public string DataModelDirectory { get; protected set; }

        /// <InheritDoc/>  
        public string ReferencesDirectory { get; protected set; }

        /// <InheritDoc/>
        public string ControlReferencesFile { get; protected set; }

        /// <InheritDoc/> 
        public AssemblyReferenceManager ReferenceManager { get; internal protected set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public FileSystem(ISerializer serializer, ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        }

        /// <InheritDoc/>
        protected void Initialize()
        {
            if(String.IsNullOrEmpty(this.WorkingDirectory))
            {
                throw new InvalidOperationException("WorkingDirectory is not initialized");
            }

            this.ScriptsDirectory = Path.Combine(this.WorkingDirectory, Constants.ScriptsDirectory);
            this.TempDirectory = Path.Combine(this.WorkingDirectory, Constants.TemporaryDirectory);
            this.DataModelDirectory = Path.Combine(this.WorkingDirectory, Constants.DataModelDirectory);
            this.ReferencesDirectory = Path.Combine(this.WorkingDirectory, Constants.ReferencesDirectory);
            this.ControlReferencesFile = Path.Combine(this.WorkingDirectory, Constants.ControlRefrencesFileName);

            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }

            if (!Directory.Exists(ScriptsDirectory))
            {
                Directory.CreateDirectory(ScriptsDirectory);
            }

            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            if (!Directory.Exists(DataModelDirectory))
            {
                Directory.CreateDirectory(DataModelDirectory);
            }

            if (!Directory.Exists(ReferencesDirectory))
            {
                Directory.CreateDirectory(ReferencesDirectory);
            }
        }

        /// <InheritDoc/>
        public T LoadFile<T>(string fileName) where T: new()
        {
            return this.serializer.Deserialize<T>(fileName);
        }

        /// <InheritDoc/>
        public IEnumerable<T> LoadFiles<T>(string directory) where T : new()
        {
            var fileDescription = TypeDescriptor.GetAttributes(typeof(T))[typeof(FileDescriptionAttribute)] as FileDescriptionAttribute;
            if (fileDescription != null)
            {
               var locatedFiles = Directory.GetFiles(directory, $"*.{fileDescription.Extension}");
               foreach(var file in locatedFiles)
                {
                    yield return this.serializer.Deserialize<T>(file);
                }
            }
            yield break;
        }

        /// <InheritDoc/>
        public void SaveToFile<T>(T model, string directory) where T : new()
        {
            var fileDescription = TypeDescriptor.GetAttributes(typeof(T))[typeof(FileDescriptionAttribute)] as FileDescriptionAttribute;
            if (fileDescription != null)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                this.serializer.Serialize<T>(Path.Combine(directory, $"{model}.{fileDescription.Extension}"),
                    model);
            }
        }

        /// <InheritDoc/>
        public void SaveToFile<T>(T model, string directory, string fileName) where T : new()
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string targetFile = Path.Combine(directory, fileName);
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            this.serializer.Serialize<T>(targetFile, model);           
        }

        /// <InheritDoc/>
        public void CreateOrReplaceFile(string directory, string fileName, string content)
        {
            var targetFile = Path.Combine(directory, fileName);
            if(File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            using(var fs = File.CreateText(targetFile))
            {
                fs.Write(content ?? string.Empty);
            }
        }

        /// <InheritDoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <InheritDoc/>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <InheritDoc/>
        public string GetRelativePath(string path)
        {
            return Path.GetRelativePath(this.WorkingDirectory, path);
        }
    }

    /// <summary>
    /// Implementation of <see cref="IVersionedFileSystem"/>
    /// </summary>
    public abstract class VersionedFileSystem : FileSystem, IVersionedFileSystem
    {
        /// <InheritDoc/>
        public VersionInfo ActiveVersion { get; protected set; }

        /// <InheritDoc/>
        public abstract void SwitchToVersion(VersionInfo versionInfo);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public VersionedFileSystem(ISerializer  serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {
           
        }
    }
}
