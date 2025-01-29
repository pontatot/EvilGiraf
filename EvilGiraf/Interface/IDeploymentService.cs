using EvilGiraf.Model;
using k8s.Models;

namespace EvilGiraf.Interface;

public interface IDeploymentService
{
    public Task<V1Deployment> CreateDeployment(DeploymentModel model);
    public Task<V1Deployment> ReadDeployment(string name, string @namespace);
    public Task<V1Status> DeleteDeployment(string name, string @namespace);
}