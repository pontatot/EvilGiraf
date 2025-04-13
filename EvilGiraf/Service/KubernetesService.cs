using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class KubernetesService(IDeploymentService deploymentService, INamespaceService namespaceService, IServiceService serviceService, IGitBuildService gitBuildService) : IKubernetesService
{
    public async Task Deploy(Application app, int timeoutSeconds = 600)
    {
        await namespaceService.CreateIfNotExistsNamespace(app.Id.ToNamespace());
        
        string imageLink;
        
        if (app.Type == ApplicationType.Git)
        {
            if (await gitBuildService.BuildAndPushFromGitAsync(app, timeoutSeconds) is {} link)
                imageLink = link;
            else
            {
                Console.WriteLine($"Failed to build and push image for app {app.Id}");
                return;
            }
        }
        else imageLink = app.Link;

        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
        {
            await deploymentService.CreateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                imageLink, app.Port));

            if(app.Port is not null)
            {
                await serviceService.CreateService(new ServiceModel(app.Name, app.Id.ToNamespace(),
                    app.Port.Value));
            }
        }
        else
        {
            await deploymentService.UpdateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                imageLink, app.Port));

            if (app.Port is not null)
            {
                await serviceService.CreateIfNotExistsService(new ServiceModel(app.Name, app.Id.ToNamespace(),
                    app.Port.Value));
            }
        }
    }
    
    public async Task Undeploy(Application app)
    {
        await namespaceService.DeleteNamespace(app.Id.ToNamespace());
    }
}