﻿using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Extensions;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

public class ReferencesRepository : IReferencesRepository
{
    private readonly ILogger logger;
    private readonly IMongoCollection<ProjectReferences> referencesCollection;
 
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public ReferencesRepository(ILogger<ReferencesRepository> logger, IMongoDbSettings dbSettings)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        referencesCollection = database.GetCollection<ProjectReferences>(dbSettings.ProjectReferencesCollectionName);
    }

    /// <inheritdoc/>  
    public async Task<ProjectReferences> GetProjectReferences(string projectId, string projectVersion)
    {
       return await this.referencesCollection.FindFirstOrDefaultAsync(x=> x.ProjectId == projectId && x.ProjectVersion == projectVersion);
    }

    /// <inheritdoc/>  
    public async Task AddProjectReferences(string projectId, string projectVersion, ProjectReferences projectReferences)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion);
        if((await this.referencesCollection.FindFirstOrDefaultAsync(x => x.ProjectId.Equals(projectId) && x.ProjectVersion.Equals(projectVersion))) != null)
        {
            throw new InvalidOperationException($"PrefabReferences for project : {projectId} and version : {projectVersion} alredy exists");
        }
        projectReferences.ProjectId = projectId;
        projectReferences.ProjectVersion = projectVersion;      
        await this.referencesCollection.InsertOneAsync(projectReferences);
        logger.LogInformation("ProjectReferences was added to version : '{0}' of project : '{1}'", projectVersion, projectId);
    }

    /// <inheritdoc/>  
    public async Task<bool> IsControlInUse(string controlId)
    {
        var filter = Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences,
               Builders<ControlReference>.Filter.Eq(x => x.ControlId, controlId));
        long count = await this.referencesCollection.CountDocumentsAsync(filter);
        return count > 0;
    }

    /// <inheritdoc/>  
    public async Task<bool> HasControlReference(string projectId, string projectVersion, ControlReference controlReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion)
                      & Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences, Builders<ControlReference>.Filter.Eq(x => x.ControlId, controlReference.ControlId));
        var controlReferences = (await this.referencesCollection.FindAsync<List<ControlReference>>(filter, new FindOptions<ProjectReferences, List<ControlReference>>()
        {
            Projection = Builders<ProjectReferences>.Projection.Expression(u => u.ControlReferences)
        })).ToList().FirstOrDefault();
        return controlReferences?.Contains(controlReference) ?? false;
    }

    /// <inheritdoc/>  
    public async Task AddControlReference(string projectId, string projectVersion, ControlReference controlReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion);
        var push = Builders<ProjectReferences>.Update.Push(t => t.ControlReferences, controlReference);
        await this.referencesCollection.UpdateOneAsync(filter, push);
        logger.LogInformation("Control reference {@0} was added to version : '{1}' of  project : {2}", controlReference, projectVersion, projectId);
    }

    /// <inheritdoc/>  
    public async Task UpdateControlReference(string projectId, string projectVersion, ControlReference controlReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion)
                      & Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences, Builders<ControlReference>.Filter.Eq(x => x.ControlId, controlReference.ControlId));
        var update = Builders<ProjectReferences>.Update.Set(x => x.ControlReferences.FirstMatchingElement().Version, controlReference.Version);
        await this.referencesCollection.UpdateOneAsync(filter, update);
        logger.LogInformation("Control reference {@0} was updated for version : '{1}' of  project : {2}", controlReference, projectVersion, projectId);

    }

    /// <inheritdoc/>  
    public async Task<bool> IsPrefabInUse(string prefabId)
    {
        var filter = Builders<ProjectReferences>.Filter.ElemMatch(x => x.PrefabReferences,
               Builders<PrefabReference>.Filter.Eq(x => x.PrefabId, prefabId));
        long count = await this.referencesCollection.CountDocumentsAsync(filter);
        return count > 0;
    }

    /// <inheritdoc/>  
    public async Task<bool> HasPrefabReference(string projectId, string projectVersion, PrefabReference prefabReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion)
                      & Builders<ProjectReferences>.Filter.ElemMatch(x => x.PrefabReferences, Builders<PrefabReference>.Filter.Eq(x => x.PrefabId, prefabReference.PrefabId));
        var prefabReferences = (await this.referencesCollection.FindAsync<List<PrefabReference>>(filter, new FindOptions<ProjectReferences, List<PrefabReference>>()
        {
            Projection = Builders<ProjectReferences>.Projection.Expression(u => u.PrefabReferences)
        })).ToList().FirstOrDefault();
        return prefabReferences?.Contains(prefabReference) ?? false;
    }

    /// <inheritdoc/>  
    public async Task AddPrefabReference(string projectId, string projectVersion, PrefabReference prefabReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion);
        var push = Builders<ProjectReferences>.Update.Push(t => t.PrefabReferences, prefabReference);
        await this.referencesCollection.UpdateOneAsync(filter, push);
        logger.LogInformation("Prefab reference {@0} was added to version : '{1}' of  project : {2}", prefabReference, projectVersion, projectId);
    }

    /// <inheritdoc/>  
    public async Task UpdatePrefabReference(string projectId, string projectVersion, PrefabReference prefabReference)
    {
        var filter = Builders<ProjectReferences>.Filter.Eq(x => x.ProjectId, projectId) & Builders<ProjectReferences>.Filter.Eq(x => x.ProjectVersion, projectVersion)
                       & Builders<ProjectReferences>.Filter.ElemMatch(x => x.PrefabReferences, Builders<PrefabReference>.Filter.Eq(x => x.PrefabId, prefabReference.PrefabId));
        var update = Builders<ProjectReferences>.Update.Set(x => x.PrefabReferences.FirstMatchingElement().Version, prefabReference.Version);
        await this.referencesCollection.UpdateOneAsync(filter, update);
        logger.LogInformation("Prefab reference {@0} was updated for version : '{1}' of  project : {2}", prefabReference, projectVersion, projectId);
    }

    /// <inheritdoc/>  
    public async Task SetEditorReferences(string projectId, string projectVersion, EditorReferences editorReferences)
    {
        var projectReferences = await this.GetProjectReferences(projectId, projectVersion);
        await this.referencesCollection.UpdateField<ProjectReferences, string, EditorReferences>(projectReferences, x => x.EditorReferences, editorReferences);
    }   
}
