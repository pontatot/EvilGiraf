using EvilGiraf.Interface.Kubernetes;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class NamespaceService(IKubernetes client) : INamespaceService
{
    public async Task<V1Namespace> CreateNamespace(string name)
    {
        return await client.CoreV1.CreateNamespaceAsync(new V1Namespace { Metadata = new V1ObjectMeta { Name = name } });
    }

    public async Task<V1Namespace?> ReadNamespace(string name)
    {
        try
        {
            return await client.CoreV1.ReadNamespaceAsync(name);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Status?> DeleteNamespace(string name)
    {
        try
        {
            return await client.CoreV1.DeleteNamespaceAsync(name);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Namespace> CreateIfNotExistsNamespace(string name)
    {
        if (await ReadNamespace(name) is { } @namespace)
            return @namespace;
        return await CreateNamespace(name);
    }
}  