using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Respository.Interfaces;
using Pixel.Persistence.Services.Api.Jobs;
using Pixel.Persistence.Services.Api.Services;
using Quartz;
using Serilog;

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
            services.AddSingleton<RetentionPolicy>(sp => sp.GetRequiredService<IOptions<RetentionPolicy>>().Value);
            services.AddSingleton<IJobManager, JobManager>();
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
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
           
            services.AddControllers();
            services.AddRazorPages();

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
            });
       
            services.AddHostedService<StatisticsProcessorService>();
            services.AddHostedService<QuartzJobBuilderService>();
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
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
