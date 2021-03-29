﻿using Dawn;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Pixel.Automation.Core
{
    public abstract class FileSystem : IFileSystem
    {
        protected readonly ISerializer serializer;

        protected readonly ApplicationSettings applicationSettings;

        public string ReferencesFile => Path.Combine(this.DataModelDirectory, "AssemblyReferences.dat");

        public string WorkingDirectory { get; protected set; }

        public string ScriptsDirectory { get; protected set; }

        public string TempDirectory { get; protected set; }

        public string DataModelDirectory { get; protected set; }

        public string ReferencesDirectory { get; protected set; }      

        public FileSystem(ISerializer serializer, ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        }

        protected void Initialize()
        {
            if(String.IsNullOrEmpty(this.WorkingDirectory))
            {
                throw new InvalidOperationException("WorkingDirectory is not initialized");
            }

            this.ScriptsDirectory = Path.Combine(this.WorkingDirectory, "Scripts");
            this.TempDirectory = Path.Combine(this.WorkingDirectory, "Temp");
            this.DataModelDirectory = Path.Combine(this.WorkingDirectory, "DataModel");
            this.ReferencesDirectory = Path.Combine(this.WorkingDirectory, "References");

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
       
        private AssemblyReferences editorReferences;

        public string[] GetAssemblyReferences()
        {
            if (File.Exists(ReferencesFile))
            {
                this.editorReferences = serializer.Deserialize<AssemblyReferences>(ReferencesFile, null);
            }
            else
            {
                this.editorReferences = new AssemblyReferences();
                this.editorReferences.GetReferencesOrDefault();
                serializer.Serialize<AssemblyReferences>(ReferencesFile, this.editorReferences);

            }
            return this.editorReferences.GetReferencesOrDefault();
        }

        public void UpdateAssemblyReferences(IEnumerable<string> references)
        {
            if(this.editorReferences == null)
            {
                GetAssemblyReferences();
            }
            this.editorReferences.AddReferences(references);
            serializer.Serialize<AssemblyReferences>(ReferencesFile, this.editorReferences);
        }       

        public T LoadFile<T>(string fileName) where T: new()
        {
            return this.serializer.Deserialize<T>(fileName);
        }

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

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public string GetRelativePath(string path)
        {
            return Path.GetRelativePath(this.WorkingDirectory, path);
        }
    }

    public abstract class VersionedFileSystem : FileSystem, IVersionedFileSystem
    {
        public VersionInfo ActiveVersion { get; protected set; }

        public abstract void SwitchToVersion(VersionInfo versionInfo);

        public VersionedFileSystem(ISerializer  serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {
           
        }
    }
}