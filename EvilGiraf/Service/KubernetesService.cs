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

        await HandleDeployment(app, imageLink);
        await HandleServiceAndIngress(app);
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
    
    private async Task HandleDeployment(Application app, string imageLink)
    {
        var deploymentModel = new DeploymentModel(app.Name, app.Id.ToNamespace(), 1, imageLink, app.Port,
            app.Variables is null || app.Variables.Count == 0 ? null : app.Name);
        await deploymentService.CreateOrReplaceDeployment(deploymentModel);
    }
    
    private async Task HandleServiceAndIngress(Application app)
    {
        if (app.Port is null)
        {
            await serviceService.DeleteService(app.Name, app.Id.ToNamespace());
            await ingressService.DeleteIngress(app.Name, app.Id.ToNamespace());
            return;
        }
            
        var serviceModel = new ServiceModel(app.Name, app.Id.ToNamespace(), app.Port.Value);
        
        await serviceService.CreateOrReplaceService(serviceModel);
        if (app.DomainName is not null)
        {
            var ingressModel = new IngressModel(app.Name, app.Id.ToNamespace(), app.DomainName, app.Port.Value, "/");
            await ingressService.CreateOrReplaceIngress(ingressModel);
        }
        else
            await ingressService.DeleteIngress(app.Name, app.Id.ToNamespace());
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