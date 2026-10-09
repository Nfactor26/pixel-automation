using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class PrefabsRepositoryClientV2 : IPrefabsRepositoryClientV2
{
    private readonly ILogger logger = Log.ForContext<TestsRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    public PrefabsRepositoryClientV2(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    public Task<Prefab> AddPrefabAsync(string projectId, string projectVersion, Prefab prefab)
    {
        throw new NotImplementedException();
    }

    public Task DeletePrefabAsync(string projectId, string projectVersion, string prefabId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Prefab>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan)
    {
        throw new NotImplementedException();
    }

    public Task<Prefab> GetByIdAsync(string projectId, string projectVersion, string prefabId)
    {
        throw new NotImplementedException();
    }

    public Task<TestCase> GetByNameAsync(string projectId, string projectVersion, string displayName)
    {
        throw new NotImplementedException();
    }

    public Task<Prefab> UpdatePrefabAsync(string projectId, string projectVersion, Prefab prefab)
    {
        throw new NotImplementedException();
    }
}
