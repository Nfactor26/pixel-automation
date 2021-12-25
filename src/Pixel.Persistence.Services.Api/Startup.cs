using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Services;
using Serilog;
using System.Security.Claims;

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
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));
            services.Configure<RetentionPolicy>(Configuration.GetSection(nameof(RetentionPolicy)));
            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
            services.AddSingleton<RetentionPolicy>(sp => sp.GetRequiredService<IOptions<RetentionPolicy>>().Value);

            services.AddTransient<ITestSessionRepository, TestSessionRespository>();
            services.AddTransient<ITestResultsRepository, TestResultsRepository>();
            services.AddTransient<ITestStatisticsRepository, TestStatisticsRepository>();
            services.AddTransient<IProjectStatisticsRepository, ProjectStatisticsRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IControlRepository, ControlRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IPrefabRepository, PrefabRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
                      
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
            .AddOAuth2Introspection(options =>
            {               
                options.Authority = Configuration["INTROSPECTION_AUTHORITY"];
                options.ClientId = "pixel-persistence-api";
                options.ClientSecret = "pixel-secret";                
            });
          
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy(Policies.WriteProcessDataPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(Roles.EditorUserRole);
                });
                options.AddPolicy(Policies.ReadProcessDataPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        return context.User.IsInRole(Roles.EditorUserRole) || context.User.HasClaim("sub", "pixel-test-runner");
                    });
                });
                options.AddPolicy(Policies.WriteTestDataPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        return context.User.IsInRole(Roles.EditorUserRole) || context.User.HasClaim("sub", "pixel-test-runner");
                    });
                });
                options.AddPolicy(Policies.ReadTestDataPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(Roles.DashboardUserRole, Roles.EditorUserRole);
                });
            });

            services.AddControllers();
            services.AddRazorPages();

            services.AddSwaggerGen();
            services.AddHostedService<StatisticsProcessorService>();
            services.AddHostedService<PurgeFileService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
