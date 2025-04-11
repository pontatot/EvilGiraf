using EvilGiraf.Model;
using k8s.Models;

namespace EvilGiraf.Interface;

public interface IServiceService
{
    public Task<V1Service> CreateService(ServiceModel model);
    
    public Task<V1Service?> ReadService(string name, string @namespace);
    
    public Task<V1Service?> UpdateService(ServiceModel model);
    
    public Task<V1Service?> DeleteService(string name, string @namespace);

    public Task<V1Service> CreateIfNotExistsService(ServiceModel model);
}