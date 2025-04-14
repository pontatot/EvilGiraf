using EvilGiraf.Model.Kubernetes;
using k8s.Models;

namespace EvilGiraf.Interface.Kubernetes;

public interface IServiceService
{
    public Task<V1Service> CreateService(ServiceModel model);
    
    public Task<V1Service?> ReadService(string name, string @namespace);
    
    public Task<V1Service?> UpdateService(ServiceModel model);
    
    public Task<V1Service?> DeleteService(string name, string @namespace);

    public Task<V1Service?> CreateOrReplaceService(ServiceModel model);
}