using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Pixel.Script.Editor.Services.CSharp.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public class CachingCodeFixProviderForProjects
    {
        private readonly ConcurrentDictionary<ProjectId, IEnumerable<CodeFixProvider>> _cache = new ConcurrentDictionary<ProjectId, IEnumerable<CodeFixProvider>>(); 

        public IEnumerable<CodeFixProvider> GetAllCodeFixesForProject(ProjectId projectId)
        {
            if (_cache.ContainsKey(projectId))
                return _cache[projectId];

            return Enumerable.Empty<CodeFixProvider>();
        }

        public void LoadFrom(ProjectInfo project)
        {
            var codeFixes = project.AnalyzerReferences
                .OfType<AnalyzerFileReference>()
                .SelectMany(analyzerFileReference => analyzerFileReference.GetAssembly().DefinedTypes)
                .Where(x => x.IsSubclassOf(typeof(CodeFixProvider)))
                .Select(x =>
                {
                    try
                    {
                        var attribute = x.GetCustomAttribute<ExportCodeFixProviderAttribute>();

                        if (attribute?.Languages != null && attribute.Languages.Contains(project.Language))
                        {
                            return x.AsType().CreateInstance<CodeFixProvider>();
                        }

                        //_logger.LogInformation($"Skipping code fix provider '{x.AsType()}' because it's language doesn't match '{project.Language}'.");

                        return null;
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError($"Creating instance of code fix provider '{x.AsType()}' failed, error: {ex}");

                        return null;
                    }
                })
                .Where(x => x != null)
                .ToImmutableArray();

            _cache.AddOrUpdate(project.Id, codeFixes, (_, __) => codeFixes);
        }
    }
}
