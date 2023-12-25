using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Extensions;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ProjectsRepository : IProjectsRepository
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<AutomationProject> projectsCollection;
        private readonly ITestFixtureRepository fixturesRepository;       
        private readonly ITestCaseRepository testCaseRepository;
        private readonly IReferencesRepository referencesRepository;
        private readonly ITestDataRepository testDataRepository;
        private readonly IProjectFilesRepository projectFilesRepository;
        
        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();      

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbSettings"></param>
        public ProjectsRepository(ILogger<ProjectsRepository> logger, IMongoDbSettings dbSettings,
            ITestFixtureRepository fixturesRepository, ITestCaseRepository testCaseRepository, IReferencesRepository referencesRepository,
            ITestDataRepository testDataRepository, IProjectFilesRepository projectFilesRepository)
        {
            Guard.Argument(dbSettings, nameof(dbSettings)).NotNull();
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.fixturesRepository = Guard.Argument(fixturesRepository, nameof(fixturesRepository)).NotNull().Value;
            this.testCaseRepository = Guard.Argument(testCaseRepository, nameof(testCaseRepository)).NotNull().Value;
            this.referencesRepository = Guard.Argument(referencesRepository, nameof(referencesRepository)).NotNull().Value;
            this.testDataRepository = Guard.Argument(testDataRepository, nameof(testDataRepository)).NotNull().Value;
            this.projectFilesRepository = Guard.Argument(projectFilesRepository, nameof(projectFilesRepository)).NotNull().Value;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            projectsCollection = database.GetCollection<AutomationProject>(dbSettings.AutomationProjectCollectionName);
        }

        /// <inheritdoc/>   
        public async Task<AutomationProject> FindByIdAsync(string projectId, CancellationToken cancellationToken)
        {
            return await projectsCollection.FindFirstOrDefaultAsync(x => x.ProjectId.Equals(projectId), cancellationToken);
        }

        /// <inheritdoc/>  
        public async Task<AutomationProject> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await projectsCollection.FindFirstOrDefaultAsync(x => x.Name.Equals(name), cancellationToken);
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<AutomationProject>> FindAllAsync(CancellationToken cancellationToken)
        {
            var projects = await projectsCollection.FindAsync(Builders<AutomationProject>.Filter.Empty, 
                null, cancellationToken);
            return projects.ToList();
        }

        /// <inheritdoc/>  
        public async Task AddProjectAsync(AutomationProject automationProject, CancellationToken cancellationToken)
        {
            Guard.Argument(automationProject).NotNull();
            var exists = (await FindByIdAsync(automationProject.ProjectId, cancellationToken)) != null;
            if(exists)
            {
                throw new InvalidOperationException($"Project with Id : {automationProject.Id} already exists");
            }
            await projectsCollection.InsertOneAsync(automationProject, InsertOneOptions, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Automation project {0} was added.", automationProject);
        }
               

        /// <summary>
        /// Create a copy of 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="newVersion"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddProjectVersionAsync(string projectId, VersionInfo newVersion, VersionInfo cloneFrom, CancellationToken cancellationToken)
        {
            var filter = Builders<AutomationProject>.Filter.Eq(x => x.ProjectId, projectId)
                            & Builders<AutomationProject>.Filter.ElemMatch(x => x.AvailableVersions, Builders<VersionInfo>.Filter.Eq(x => x.Version, newVersion.Version));
            var projectVersions = (await this.projectsCollection.FindAsync<List<VersionInfo>>(filter, new FindOptions<AutomationProject, List<VersionInfo>>()
            {
                Projection = Builders<AutomationProject>.Projection.Expression(u => u.AvailableVersions)
            })).ToList().FirstOrDefault();
            if (projectVersions?.Contains(newVersion) ?? false)
            {
                throw new InvalidOperationException($"Version {newVersion} already exists for project {projectId}");
            }

            var projectReference = await this.referencesRepository.GetProjectReferences(projectId, cloneFrom.Version.ToString());
            projectReference.Id = string.Empty;
            await this.referencesRepository.AddProjectReferences(projectId, newVersion.ToString(), projectReference);


            DateTime laterThan = DateTime.MinValue.ToUniversalTime();
            var fixtures = await this.fixturesRepository.GetFixturesAsync(projectId, cloneFrom.ToString(), laterThan, cancellationToken);
            foreach(var fixture in fixtures)
            {
                fixture.Id = string.Empty;
            }
            await this.fixturesRepository.AddFixturesAsync(projectId, newVersion.ToString(), fixtures, cancellationToken);

            var tests = await this.testCaseRepository.GetTestCasesAsync(projectId, cloneFrom.ToString(), laterThan, cancellationToken);
            foreach(var test in tests)
            {
                test.Id = string.Empty;
            }
            await this.testCaseRepository.AddTestCasesAsync(projectId, newVersion.ToString(), tests, cancellationToken);

            var dataSources = await this.testDataRepository.GetDataSourcesAsync(projectId, cloneFrom.ToString(), laterThan, cancellationToken);
            foreach(var dataSource in dataSources)
            {
                dataSource.Id = string.Empty;
            }
            await this.testDataRepository.AddDataSourcesAsync(projectId, newVersion.ToString(), dataSources, cancellationToken);

            await foreach (var file in this.projectFilesRepository.GetFilesAsync(projectId, cloneFrom.ToString(), new string[] { }))
            {
                //when creating a copy from previous version, we don't want dll and pdb files to be brough in to new version.                
                if (Path.GetExtension(file.FileName).Equals(".dll") || Path.GetExtension(file.FileName).Equals(".pdb"))
                {
                    continue;
                }
                await this.projectFilesRepository.AddOrUpdateFileAsync(projectId, newVersion.ToString(), file);
            }
            
            filter = Builders<AutomationProject>.Filter.Eq(x => x.ProjectId, projectId);
            var push = Builders<AutomationProject>.Update.Push(t => t.AvailableVersions, newVersion);
            await this.projectsCollection.UpdateOneAsync(filter, push);
        }

        /// <inheritdoc/>  
        public async Task UpdateProjectVersionAsync(string projectId, VersionInfo projectVersion, CancellationToken cancellationToken)
        {
            var filter = Builders<AutomationProject>.Filter.Eq(x => x.ProjectId, projectId)
                             & Builders<AutomationProject>.Filter.ElemMatch(x => x.AvailableVersions, Builders<VersionInfo>.Filter.Eq(x => x.Version, projectVersion.Version));
            var projectVersions = (await this.projectsCollection.FindAsync<List<VersionInfo>>(filter, new FindOptions<AutomationProject, List<VersionInfo>>()
            {
                Projection = Builders<AutomationProject>.Projection.Expression(u => u.AvailableVersions)
            }))
            .ToList().FirstOrDefault();

            if (projectVersions?.Contains(projectVersion) ?? false)
            {
                if (!projectVersion.IsActive && projectVersion.PublishedOn is null)
                {
                    projectVersion.PublishedOn = DateTime.UtcNow;
                }
                var update = Builders<AutomationProject>.Update.Set(x => x.AvailableVersions.FirstMatchingElement(), projectVersion);
                await this.projectsCollection.UpdateOneAsync(filter, update);
                logger.LogInformation("Project version {0} was updated for project : {1}", projectVersion, projectId);
                return;
            }
            throw new InvalidOperationException($"Version {projectVersion} doesn't exist on project {projectId}");
        }
    }
}

