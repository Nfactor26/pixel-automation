using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

/// <summary>
/// Contract for managing docker compose files repository
/// </summary>
public interface IComposeFilesRepository
{
    /// <summary>
    /// Check if file with given name exists
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<bool> CheckFileExistsAsync(string fileName);

    /// <summary>
    /// Get the names of all the available files
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<string> GetAllFileNamesAsync();

    /// <summary>
    /// Get all the avaialble files
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<DataFile> GetAllFilesAsync();

    /// <summary>
    /// Get file with given name
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<DataFile> GetFileAsync(string fileName);

    /// <summary>
    /// Add a new file
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileContents"></param>
    /// <returns></returns>
    Task AddOrUpdateFileAsync(string fileName, byte[] fileContents);

    /// <summary>
    /// Delete an existing file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task DeleteFileAsync(string fileName);
  
}