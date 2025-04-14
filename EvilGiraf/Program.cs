using System.Diagnostics.CodeAnalysis;
using EvilGiraf.Authentication;
using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Service;
using EvilGiraf.Service.Kubernetes;
using k8s;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace EvilGiraf;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApiKeyAuth(options =>
        {
            options.ApiKey = builder.Configuration["ApiKey"]!;
        });
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "EvilGiraf API", Version = "v1" });
    
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Description = "API Key authentication using the 'X-API-Key' header"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        builder.Services.AddSingleton<IKubernetes>(_ =>
        {
            var config = KubernetesClientConfiguration.IsInCluster() ?
                KubernetesClientConfiguration.InClusterConfig() :
                KubernetesClientConfiguration.BuildConfigFromConfigFile();
            return new Kubernetes(config);
        });
        builder.Services.AddDbContext<DatabaseService>(opt =>
            opt.UseNpgsql(
                builder.Configuration.GetSection("Postgres")
                .Get<NpgsqlConnectionStringBuilder>()?.ConnectionString));
        builder.Services.AddScoped<IDeploymentService, DeploymentService>();
        builder.Services.AddScoped<INamespaceService, NamespaceService>();
        builder.Services.AddScoped<IApplicationService, ApplicationService>();
        builder.Services.AddScoped<IKubernetesService, KubernetesService>();
        builder.Services.AddScoped<IServiceService, ServiceService>();
        builder.Services.AddScoped<IGitBuildService, GitBuildService>();
        builder.Services.AddScoped<IIngressService, IngressService>();
        builder.Services.AddScoped<IConfigMapService, ConfigMapService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
