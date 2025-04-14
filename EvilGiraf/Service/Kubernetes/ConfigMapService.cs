using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model.Kubernetes;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class ConfigMapService(IKubernetes client) : IConfigMapService
{
    public async Task<V1ConfigMap> CreateConfigMap(ConfigMapModel model)
    {
        return await client.CoreV1.CreateNamespacedConfigMapAsync(model.ToConfigMap(), model.Namespace);
    }

    public async Task<V1ConfigMap?> ReadConfigMap(string name, string @namespace)
    {
        try
        {
            return await client.CoreV1.ReadNamespacedConfigMapAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1ConfigMap?> UpdateConfigMap(ConfigMapModel model)
    {
        try
        {
            return await client.CoreV1.ReplaceNamespacedConfigMapAsync(model.ToConfigMap(), model.Name, model.Namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Status?> DeleteConfigMap(string name, string @namespace)
    {
        try
        {
            return await client.CoreV1.DeleteNamespacedConfigMapAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1ConfigMap?> CreateOrReplaceConfigMap(ConfigMapModel model)
    {
        if (await ReadConfigMap(model.Name, model.Namespace) is null)
            return await CreateConfigMap(model);
        return await UpdateConfigMap(model);
    }
}