using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class PrefabsRepositoryClient : IPrefabRepositoryClient
{
    private readonly ILogger logger = Log.ForContext<PrefabsRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    public PrefabsRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    public Task<Prefab> GetByIdAsync(string prefabId, string version)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Prefab>> GetPrefabs(string applicationId, DateTime laterThan)
    {
        throw new NotImplementedException();
    }

  

    public Task<Prefab> AddPrefabToScreenAsync(Prefab prefab, string screenId)
    {
        throw new NotImplementedException();
    }

    public Task DeletePrefabAsync(Prefab prefabProject)
    {
        throw new NotImplementedException();
    }

   

    public Task<Prefab> UpdatePrefabAsync(Prefab prefab)
    {
        throw new NotImplementedException();
    }
}
