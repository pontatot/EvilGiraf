using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class KubernetesService(IDeploymentService deploymentService, INamespaceService namespaceService, IServiceService serviceService) : IKubernetesService
{
    public async Task Deploy(Application app)
    {
        await namespaceService.CreateIfNotExistsNamespace(app.Id.ToNamespace());
        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
        {
            await deploymentService.CreateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, app.Ports));

            if(app.Ports.Length > 0)
            {
                await serviceService.CreateService(new ServiceModel(app.Name, app.Id.ToNamespace(), "ClusterIP",
                    app.Ports, "TCP", app.Name));
            }
        }
        else
        {
            await deploymentService.UpdateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, app.Ports));

            if (app.Ports.Length > 0)
            {
                await serviceService.CreateIfNotExistsService(new ServiceModel(app.Name, app.Id.ToNamespace(), "ClusterIP",
                    app.Ports, "TCP", app.Name));
            }
        }
    }
}