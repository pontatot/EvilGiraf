using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model.Kubernetes;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class ServiceService(IKubernetes client) : IServiceService
{
    public async Task<V1Service> CreateService(ServiceModel model)
    {
        return await client.CoreV1.CreateNamespacedServiceAsync(model.ToService(), model.Namespace);
    }

    public async Task<V1Service?> ReadService(string name, string @namespace)
    {
        try
        {
            return await client.CoreV1.ReadNamespacedServiceAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Service?> UpdateService(ServiceModel model)
    {
        try
        {
            return await client.CoreV1.ReplaceNamespacedServiceAsync(model.ToService(), model.Name, model.Namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Service?> DeleteService(string name, string @namespace)
    {
        try
        {
            return await client.CoreV1.DeleteNamespacedServiceAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Service?> CreateOrReplaceService(ServiceModel model)
    {
        if (await ReadService(model.Name, model.Namespace) is null)
            return await CreateService(model);
        return await UpdateService(model);
    }
}