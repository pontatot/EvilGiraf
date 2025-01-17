using EvilGiraf.Model;
using k8s.Models;

namespace EvilGiraf.Interface;

public interface IDeploymentService
{
    public Task<V1Deployment> CreateDeployment(DeploymentModel model);
}