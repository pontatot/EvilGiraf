using EvilGiraf.Model.Kubernetes;
using k8s.Models;

namespace EvilGiraf.Interface.Kubernetes;

public interface IIngressService
{
    public Task<V1Ingress> CreateIngress(IngressModel model);
    
    public Task<V1Ingress?> ReadIngress(string name, string @namespace);
    
    public Task<V1Ingress?> UpdateIngress(IngressModel model);
    
    public Task<V1Status?> DeleteIngress(string name, string @namespace);

    public Task<V1Ingress?> CreateOrReplaceIngress(IngressModel model);
}