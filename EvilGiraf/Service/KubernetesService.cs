using EvilGiraf.Interface;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class KubernetesService(IDeploymentService deploymentService) : IKubernetesService
{
    public async Task Deploy(Application app)
    {
        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
        {
            await deploymentService.CreateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, []));
        }
        else
        {
            await deploymentService.UpdateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, []));
        }
    }
}