using Dawn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Services;

public class AddHandlersService : IHostedService
{
    private readonly IServiceProvider serviceProvider;   
    private readonly ILogger<AddHandlersService> logger;

    IEnumerable<TemplateHandler> handlers = new List<TemplateHandler>()
    {
        new WindowsTemplateHandler()
        {
            Name = "windows-server",
            Description = "Pixel runner standalone for windows"
        },
        new LinuxTemplateHandler()
        {
            Name = "linux-server",
            Description = "Pixel runner standalone for linux"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-pixel-run",
            Description = "Pixel runner standalone for docker containers",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"}
            },
            DockerComposeFileName = "docker-pixel-run.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-webdriver-chrome",
            Description = "Pixel runner with selenium chrome standalone",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"},
                { "selenium-standalone-image", " selenium/standalone-chrome:latest" },
                { "grid-address", "http://chrome-standalone:4444" }
            },
            DockerComposeFileName = "webdriver-chrome-standalone.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-webdriver-firefox",
            Description = "Pixel runner with selenium firefox standalone",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"},
                { "selenium-standalone-image", " selenium/standalone-firefox:latest" },
                { "grid-address", "http://firefox-standalone:4444" }
            },
            DockerComposeFileName = "webdriver-firefox-standalone.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-webdriver-edge",
            Description = "Pixel runner with selenium edge standalone",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"},
                { "selenium-standalone-image", " selenium/standalone-edge:latest" },
                { "grid-address", "http://edge-standalone:4444" }
            },
            DockerComposeFileName = "webdriver-edge-standalone.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-playwright-chrome",
            Description = "Pixel runner standalone with playwright chrome ",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"}
            },
            DockerComposeFileName = "playwright-chrome.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-playwright-firefox",
            Description = "Pixel runner standalone with playwright firefox ",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"}
            },
            DockerComposeFileName = "playwright-firefox.yml"
        },
        new DockerTemplateHandler()
        {
            Name = "docker-playwright-edge",
            Description = "Pixel runner standalone with playwright edge ",
            Parameters = new()
            {
                { "pixel-run-image", "pixel-test-runner:latest"}
            },
            DockerComposeFileName = "playwright-edge.yml"
        }
    };

    public AddHandlersService(ILogger<AddHandlersService> logger, IServiceProvider serviceProvider)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        this.serviceProvider = Guard.Argument(serviceProvider, nameof(serviceProvider)).NotNull().Value;        
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();

        var handlersRepository = scope.ServiceProvider.GetRequiredService<ITemplateHandlerRepository>();
        var composeFileRepository = scope.ServiceProvider.GetRequiredService<IComposeFilesRepository>();

        var availableHandlers = await handlersRepository.GetAllAsync(CancellationToken.None);
        foreach(var templateHandler in handlers)
        {
            if(!availableHandlers.Contains(templateHandler))
            {               
                if(templateHandler is DockerTemplateHandler dockerTemplateHandler)
                {
                    if(!(await composeFileRepository.CheckFileExistsAsync(dockerTemplateHandler.DockerComposeFileName)))
                    {
                        string fileLocation = Path.Combine(Environment.CurrentDirectory, "ComposeFiles", dockerTemplateHandler.DockerComposeFileName);
                        await composeFileRepository.AddOrUpdateFileAsync(dockerTemplateHandler.DockerComposeFileName, File.ReadAllBytes(fileLocation));
                        logger.LogInformation("Compose file : {0} was added", dockerTemplateHandler.DockerComposeFileName);                     
                    }                  
                }
                await handlersRepository.AddHandlerAsync(templateHandler, CancellationToken.None);
                logger.LogInformation("Template Handler : {0} was added", templateHandler.Name);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
