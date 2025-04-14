using EvilGiraf.Interface.Kubernetes;
using EvilGiraf.Model.Kubernetes;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service.Kubernetes;

public class IngressService(IKubernetes client, IConfiguration configuration) : IIngressService
{
    public async Task<V1Ingress> CreateIngress(IngressModel model)
    {
        var ingress = model.ToIngress();
        if (configuration.GetSection("IngressAnnotations").Get<Dictionary<string, string>>()?.Count > 0)
        {
            ingress = (model with
            {
                Annotations = configuration.GetSection("IngressAnnotations").Get<Dictionary<string, string>>()
            }).ToIngress();
        }
        return await client.NetworkingV1.CreateNamespacedIngressAsync(ingress, model.Namespace);   
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

    public async Task<V1Ingress?> CreateOrReplaceIngress(IngressModel model)
    {
        if (await ReadIngress(model.Name, model.Namespace) is null)
            return await CreateIngress(model);
        return await UpdateIngress(model);
    }
}