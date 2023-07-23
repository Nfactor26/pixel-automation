using Microsoft.AspNetCore.Components.Forms;
using Pixel.Automation.Web.Portal.Helpers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services;

public interface IComposeFileService
{
    /// <summary>
    /// Get all the available compose file names
    /// </summary>
    /// <returns></returns>
    Task<string[]> GetComposeFileNamesAsync();

    /// <summary>
    /// Upload a compose file that can be used with a template handler later
    /// </summary>
    /// <param name="composeFile"></param>
    /// <returns></returns>
    Task<OperationResult> AddComposeFileAsync(IBrowserFile composeFile);   
}

public class ComposeFileService : IComposeFileService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="httpClient"></param>
    public ComposeFileService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<OperationResult> AddComposeFileAsync(IBrowserFile composeFile)
    {
        var fileContent = new StreamContent(composeFile.OpenReadStream());
        using (var content = new MultipartFormDataContent())
        {
            content.Add(fileContent, "composeFile", composeFile.Name);
            var result = await this.httpClient.PostAsync("api/composefiles", content);
            return await OperationResult.FromResponseAsync(result);
        }
    }

    /// <inheritdoc/>
    public async Task<string[]> GetComposeFileNamesAsync()
    {
        return await this.httpClient.GetFromJsonAsync<string[]>("api/composefiles/names");
    }
}

