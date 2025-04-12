using EvilGiraf.Interface;
using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class IngressService(IKubernetes client) : IIngressService
{
    public async Task<V1Ingress> CreateIngress(IngressModel model)
    {
        return await client.NetworkingV1.CreateNamespacedIngressAsync(model.ToIngress(), model.Namespace);   
    }

    public async Task<V1Ingress?> ReadIngress(string name, string @namespace)
    {
        try
        {
            return await client.NetworkingV1.ReadNamespacedIngressAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Ingress?> UpdateIngress(IngressModel model)
    {
        try
        {
            return await client.NetworkingV1.ReplaceNamespacedIngressAsync(model.ToIngress(), model.Name, model.Namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Status?> DeleteIngress(string name, string @namespace)
    {
        try
        {
            return await client.NetworkingV1.DeleteNamespacedIngressAsync(name, @namespace);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<V1Ingress> CreateIfNotExistsIngress(IngressModel model)
    {
        var ingress = await ReadIngress(model.Name, model.Namespace);
        if (ingress != null)
            return ingress;
        return await CreateIngress(model);
    }
}