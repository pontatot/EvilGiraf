using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class DeploymentService(IKubernetes client) : IDeploymentService
{
    public async Task<V1Deployment> CreateDeployment(DeploymentModel model)
    {
        return await client.AppsV1.CreateNamespacedDeploymentAsync(model.ToDeployment(), model.Namespace);
    }
    
    public async Task<V1Deployment?> ReadDeployment(string name, string @namespace)
    {
        try
        {
            var deployment = await client.AppsV1.ReadNamespacedDeploymentAsync(name, @namespace);
            return deployment;
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Status?> DeleteDeployment(string name, string @namespace)
    {
        try
        {
            return await client.AppsV1.DeleteNamespacedDeploymentAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Deployment?> UpdateDeployment(DeploymentModel model)
    {
        try
        {
            return await client.AppsV1.ReplaceNamespacedDeploymentAsync(model.ToDeployment(), model.Name, model.Namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}