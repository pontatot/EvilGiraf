using EvilGiraf.Model.Kubernetes;
using k8s.Models;

namespace EvilGiraf.Interface.Kubernetes;

public interface IDeploymentService
{
    public Task<V1Deployment> CreateDeployment(DeploymentModel model);
    
    public Task<V1Deployment?> ReadDeployment(string name, string @namespace);
    
    public Task<V1Deployment?> UpdateDeployment(DeploymentModel model);
    
    public Task<V1Status?> DeleteDeployment(string name, string @namespace);
    
    public Task<V1Deployment?> CreateOrReplaceDeployment(DeploymentModel model);
}