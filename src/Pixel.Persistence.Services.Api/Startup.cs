using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Respository.Interfaces;
using Pixel.Persistence.Services.Api.Hubs;
using Pixel.Persistence.Services.Api.Jobs;
using Pixel.Persistence.Services.Api.Services;
using Quartz;
using Serilog;
using System;

namespace Pixel.Persistence.Services.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService("pixel-persistence");
                })
                .WithTracing(builder =>
                {
                    string[] enabledSources = Configuration.GetSection("OpenTelemetry:Sources").Get<string[]>();
                    double.TryParse(Configuration["OpenTelemetry:Sampling:Ratio"], out double samplingProbablity);

                    builder.AddSource(enabledSources);
                    builder.AddAspNetCoreInstrumentation();                 
                    builder.AddHttpClientInstrumentation();
                    builder.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingProbablity)));
                    string otlpTraceEndPoint = Configuration["OpenTelemetry:Trace:EndPoint"];
                    if (!string.IsNullOrEmpty(otlpTraceEndPoint))
                    {
                        Enum.TryParse<OtlpExportProtocol>(Configuration["OpenTelemetry:TraceExporter:OtlpExportProtocol"] ?? "HttpProtobuf", out OtlpExportProtocol exportProtocol);
                        Enum.TryParse<ExportProcessorType>(Configuration["OpenTelemetry:TraceExporter:ExportProcessorType"] ?? "Batch", out ExportProcessorType processorType);
                        builder.AddOtlpExporter(e =>
                        {
                            e.Endpoint = new Uri(otlpTraceEndPoint);
                            e.Protocol = exportProtocol;
                            e.ExportProcessorType = processorType;
                        });
                    }
                    string exporterConsole = Configuration["OpenTelemetry:TraceExporter:Console"];
                    if (!string.IsNullOrEmpty(exporterConsole) && bool.Parse(exporterConsole))
                    {
                        builder.AddConsoleExporter();
                    }
                });
                

            //To forward the scheme from the proxy in non - IIS scenarios
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });           
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));
            services.Configure<RetentionPolicy>(Configuration.GetSection(nameof(RetentionPolicy)));
            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);         
            services.AddSingleton<IJobManager, JobManager>();
            services.AddSingleton<IAgentManager, AgentManager>();            
            services.AddTransient<ITestSessionRepository, TestSessionRespository>();
            services.AddTransient<ITestResultsRepository, TestResultsRepository>();
            services.AddTransient<ITestStatisticsRepository, TestStatisticsRepository>();
            services.AddTransient<IProjectStatisticsRepository, ProjectStatisticsRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IControlRepository, ControlRepository>();
            services.AddTransient<IProjectsRepository, ProjectsRepository>();
            services.AddTransient<IReferencesRepository, ReferencesRepository>();          
            services.AddTransient<IProjectFilesRepository, ProjectFilesRepository>();
            services.AddTransient<IPrefabFilesRepository, PrefabFilesRepository>();
            services.AddTransient<ITestFixtureRepository, TestFixtureRepository>();
            services.AddTransient<ITestCaseRepository, TestCaseRepository>();
            services.AddTransient<ITestDataRepository, TestDataRepository>();           
            services.AddTransient<IPrefabsRepository, PrefabsRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
            services.AddTransient<ITemplateHandlerRepository, TemplateHandlerRepository>();
            services.AddTransient<IComposeFilesRepository, ComposeFilesRepository>();

            services.AddControllers();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                c.UseOneOfForPolymorphism();
            });

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseInMemoryStore();
            });

            services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
                q.StartDelay = TimeSpan.FromMinutes(1);
            });
       
            services.AddHostedService<StatisticsProcessorService>();
            services.AddHostedService<QuartzJobBuilderService>();
            services.AddHostedService<AddHandlersService>();
            services.AddHostedService<PurgeDataService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            app.UsePathBase("/persistence");
            app.UseSerilogRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pixel Persistence V1");
            });

            //app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();        

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<AgentsHub>("agents");
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
