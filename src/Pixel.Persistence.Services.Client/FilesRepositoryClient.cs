﻿using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using RestSharp;
using Serilog;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class ProjectFilesRepositoryClient : FilesRepositoryClient, IProjectFilesRepositoryClient
{
    public ProjectFilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer) 
        : base(clientFactory, serializer)
    {
    }

    protected override string GetBaseUrl()
    {
        return "projectfiles";
    }
}

public class PrefabFilesRepositoryClient : FilesRepositoryClient, IPrefabFilesRepositoryClient
{
    public PrefabFilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        : base(clientFactory, serializer)
    {
    }

    protected override string GetBaseUrl()
    {
        return "prefabfiles";
    }
}

public abstract class FilesRepositoryClient : IFilesRepositoryClient
{
    protected readonly ILogger logger = Log.ForContext<PrefabsRepositoryClient>();
    protected readonly IRestClientFactory clientFactory;
    protected readonly ISerializer serializer;
    protected readonly string baseUrl;


    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="clientFactory"></param>
    /// <param name="serializer"></param>
    public FilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
        this.baseUrl = GetBaseUrl();
    }

    /// <summary>
    /// Get the base url for the api endpoint
    /// </summary>
    /// <returns></returns>
    protected abstract string GetBaseUrl();

    /// <inheritdoc/>  
    public async Task<byte[]> DownloadProjectDataFilesWithTags(string projectId, string projectVersion, string[] tags)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(tags, nameof(tags)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/tags");
        foreach (var tag in tags)
        {
            restRequest.AddQueryParameter("tag", tag);
        }
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        return result.RawBytes;
    }

    /// <inheritdoc/>  
    public async Task<byte[]> DownloadProjectDataFilesOfType(string projectId, string projectVersion, string fileExtension)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileExtension, nameof(fileExtension)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/type/{fileExtension}");      
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        return result.RawBytes;
    }

    /// <inheritdoc/>  
    public async Task<ProjectDataFile> DownProjectDataFile(string projectId, string projectVersion, string fileName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/name/{fileName}");
        var client = this.clientFactory.GetOrCreateClient();
        return await client.GetAsync<ProjectDataFile>(restRequest);     
    }

    /// <inheritdoc/>  
    public async Task AddProjectDataFile(ProjectDataFile file, string filePath)
    {
        Guard.Argument(file, nameof(file)).NotNull();
        Guard.Argument(filePath).NotNull().NotEmpty();

        var projectFileRequest = new AddProjectFileRequest(file.ProjectId, file.ProjectVersion, file.Tag, file.FileName, file.FilePath);      
        RestRequest restRequest = new RestRequest(baseUrl) { Method = Method.Post };      
        restRequest.AddParameter(nameof(AddProjectFileRequest), serializer.Serialize<AddProjectFileRequest>(projectFileRequest), ParameterType.RequestBody);
        restRequest.AddFile("file", filePath);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteAsync(restRequest);
        result.EnsureSuccess();
    }
    
    /// <inheritdoc/>  
    public async Task DeleteProjectDataFile(string projectId, string projectVersion, string fileName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        var deleteFileRequest = new DeleteProjectFileRequest(projectId, projectVersion, fileName);       

        RestRequest restRequest = new RestRequest(baseUrl);
        restRequest.AddParameter(nameof(DeleteProjectFileRequest), serializer.Serialize<DeleteProjectFileRequest>(deleteFileRequest), ParameterType.RequestBody);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.DeleteAsync(restRequest);
        result.EnsureSuccess();       
    }
}
