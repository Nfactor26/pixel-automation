using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

/// <summary>
/// Contract to manage template handlers
/// </summary>
public interface ITemplateHandlerRepository
{
    /// <summary>
    /// Find template handler by Id
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TemplateHandler> FindByIdAsync(string Id, CancellationToken cancellationToken);

    /// <summary>
    /// Find template handler by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TemplateHandler> FindByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Get all template handlers
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TemplateHandler>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get template handlers matching specified criterias on request
    /// </summary>
    /// <param name="queryParameter"></param>
    /// <returns></returns>
    Task<IEnumerable<TemplateHandler>> GetHandlersAsync(GetHandlersRequest queryParameter);

    /// <summary>
    /// Add a new template handler
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddHandlerAsync(TemplateHandler handler, CancellationToken cancellationToken);

    /// <summary>
    /// Update details of an existing template handler
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateHandlerAsync(TemplateHandler handler, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an existing template handler
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteHandlerAsync(string Id, CancellationToken cancellationToken);

}
