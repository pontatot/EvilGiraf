using EvilGiraf.Interface;
using EvilGiraf.Service;
using k8s;

namespace EvilGiraf;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IKubernetes>(_ =>
        {
            var config = KubernetesClientConfiguration.IsInCluster() ?
                KubernetesClientConfiguration.InClusterConfig() :
                KubernetesClientConfiguration.BuildConfigFromConfigFile();
            return new Kubernetes(config);
        });
        builder.Services.AddSingleton<IDeploymentService, DeploymentService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
