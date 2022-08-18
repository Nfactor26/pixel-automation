using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Pixel.Scripting.Common.CSharp.Diagnostics
{
    public enum DiagnosticsUpdatedKind
    {       
        DiagnosticsRemoved,     
        DiagnosticsCreated,
    }

    public class DiagnosticsUpdatedArgs : EventArgs
    {
        /// <summary>
        /// The identity of update group. 
        /// </summary>
        public object Id { get; }

        /// <summary>
        /// <see cref="Workspace"/> this update is associated with.
        /// </summary>
        public Workspace Workspace { get; }

        /// <summary>
        /// <see cref="ProjectId"/> this update is associated with, or <see langword="null"/>.
        /// </summary>
        public ProjectId? ProjectId { get; }

        /// <summary>
        /// <see cref="DocumentId"/> this update is associated with, or <see langword="null"/>.
        /// </summary>
        public DocumentId? DocumentId { get; }

        public Solution? Solution { get; }

        private readonly IEnumerable<Diagnostic> diagnostics;

        public DiagnosticsUpdatedKind Kind { get; }

        private DiagnosticsUpdatedArgs(object id, Workspace workspace, Solution? solution, ProjectId? projectId, DocumentId? documentId, IEnumerable<Diagnostic> diagnostics, DiagnosticsUpdatedKind kind)
        {
            this.Id = id;
            this.Workspace = workspace;
            this.Solution = solution;
            this.ProjectId = projectId;
            this.DocumentId = documentId;
            this.diagnostics = diagnostics;
            this.Kind = kind;
        }
      
        public IEnumerable<Diagnostic> GetAllDiagnosticsRegardlessOfPushPullSetting()  => diagnostics;

        public static DiagnosticsUpdatedArgs DiagnosticsCreated(object id, Workspace workspace, Solution? solution, ProjectId? projectId, DocumentId? documentId, IEnumerable<Diagnostic> diagnostics)
        {
            return new DiagnosticsUpdatedArgs(id, workspace, solution, projectId, documentId, diagnostics, DiagnosticsUpdatedKind.DiagnosticsCreated);
        }

        public static DiagnosticsUpdatedArgs DiagnosticsRemoved(object id, Workspace workspace, Solution? solution, ProjectId? projectId, DocumentId? documentId)
        {
            return new DiagnosticsUpdatedArgs(id, workspace, solution, projectId, documentId, Enumerable.Empty<Diagnostic>(), DiagnosticsUpdatedKind.DiagnosticsRemoved);
        }
    }
}
