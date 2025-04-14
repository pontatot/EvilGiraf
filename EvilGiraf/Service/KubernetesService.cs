using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;
using EvilGiraf.Model.Kubernetes;

namespace EvilGiraf.Service;

public class KubernetesService(IDeploymentService deploymentService, INamespaceService namespaceService, IServiceService serviceService, IGitBuildService gitBuildService, IIngressService ingressService, IConfigMapService configMapService) : IKubernetesService
{
    public async Task Deploy(Application app, int timeoutSeconds = 600)
    {
        await namespaceService.CreateIfNotExistsNamespace(app.Id.ToNamespace());
        
        var imageLink = await GetImageLink(app, timeoutSeconds);
        if (string.IsNullOrEmpty(imageLink))
        {
            Console.WriteLine($"Failed to get image link for app {app.Id}");
            return;
        }

        var existed = await HandleDeployment(app, imageLink);
        await HandleServiceAndIngress(app, existed);
        await HandleConfigmaps(app);
    }
    
    private async Task<string?> GetImageLink(Application app, int timeoutSeconds)
    {
        if (app.Type == ApplicationType.Git)
        {
            return await gitBuildService.BuildAndPushFromGitAsync(app, timeoutSeconds);
        }
        
        return app.Link;
    }
    
    private async Task<bool> HandleDeployment(Application app, string imageLink)
    {
        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        var deploymentModel = new DeploymentModel(app.Name, app.Id.ToNamespace(), 1, imageLink, app.Port, app.Variables is null || app.Variables.Count == 0 ? null : app.Name);
        
        if (deployment is null)
        {
            await deploymentService.CreateDeployment(deploymentModel);
            return false;
        }
        await deploymentService.UpdateDeployment(deploymentModel);
        return true;
    }
    
    private async Task HandleServiceAndIngress(Application app, bool existed)
    {
        if (app.Port is null)
            return;
            
        var serviceModel = new ServiceModel(app.Name, app.Id.ToNamespace(), app.Port.Value);
        
        if (existed)
        {
            await serviceService.CreateIfNotExistsService(serviceModel);
            
            if (app.DomainName is not null)
            {
                var ingressModel = new IngressModel(app.Name, app.Id.ToNamespace(), app.DomainName, app.Port.Value, "/");
                await ingressService.CreateIfNotExistsIngress(ingressModel);
            }
        }
        else
        {
            
            await serviceService.CreateService(serviceModel);
            
            if (app.DomainName is not null)
            {
                var ingressModel = new IngressModel(app.Name, app.Id.ToNamespace(), app.DomainName, app.Port.Value, "/");
                await ingressService.CreateIngress(ingressModel);
            }
        }
    }
    
    private async Task HandleConfigmaps(Application app)
    {
        if (app.Variables is null || app.Variables.Count == 0)
        {
            await configMapService.DeleteConfigMap(app.Name, app.Id.ToNamespace());
            return;
        }
            
        var configModel = new ConfigMapModel(app.Name, app.Id.ToNamespace(), app.Variables);
        
        await configMapService.CreateOrReplaceConfigMap(configModel);
            
    }
    
    public async Task Undeploy(Application app)
    {
        await namespaceService.DeleteNamespace(app.Id.ToNamespace());
    }
}