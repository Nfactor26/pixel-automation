using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// ITestSessionRepository is used to manage <see cref="TestSession"/> stored in database
    /// </summary>
    public interface ITestSessionRepository
    {
        /// <summary>
        /// Get <see cref="TestSession"/> with a given sessionId
        /// </summary>
        /// <param name="sessionId">SessionId of the <see cref="TestSession"/> to be retreived</param>
        /// <returns></returns>
        Task<TestSession> GetTestSessionAsync(string sessionId);

        /// <summary>
        /// Get all the available <see cref="TestSession"/> matching specified search criteria in <see cref="TestSessionRequest"/>
        /// </summary>
        /// <returns>Collection of <see cref="TestSession"/> available in database matching specified search criteria</returns>
        Task<IEnumerable<TestSession>> GetTestSessionsAsync(TestSessionRequest queryParameter);

        /// <summary>
        /// Get the count of records matching specified search criteria
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        Task<long> GetCountAsync(TestSessionRequest queryParameter);

        /// <summary>
        /// Get the sessionId's of the <see cref="TestSession"/> which are not yet processed for statistics
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetUnprocessedSessionsAsync();

        /// <summary>
        /// Mark session as Processed
        /// </summary>
        /// <param name="sessionId">SessionId of the Session to be updated</param>
        /// <returns></returns>
        Task MarkSessionProcessedAsync(string sessionId);

        /// <summary>
        /// Add a new <see cref="TestSession"/> to database
        /// </summary>
        /// <param name="testSession"></param>
        /// <returns></returns>
        Task AddTestSessionAsync(TestSession testSession);
      
        /// <summary>
        /// Update the details of a <see cref="TestSession"/>
        /// </summary>
        /// <param name="sessionId">SessionId of the TestSession</param>
        /// <param name="testSession">Update <see cref="TeseSession"/> content</param>
        /// <returns></returns>
        Task UpdateTestSessionAsync(string sessionId, TestSession testSession);

        /// <summary>
        /// Delete a <see cref="TestSession"/> with given sessionId
        /// </summary>
        /// <param name="sessionId">SessionId of the <see cref="TestSession"/> to be deleted</param>
        /// <returns></returns>
        Task<bool>  TryDeleteTestSessionAsync(string sessionId);
       
    }
}
