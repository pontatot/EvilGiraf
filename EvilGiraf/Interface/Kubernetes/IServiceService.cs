using EvilGiraf.Model;
using k8s.Models;

namespace EvilGiraf.Interface;

public interface IServiceService
{
    Task<V1Service> CreateService(ServiceModel model);
    
    Task<V1Service?> ReadService(string name, string @namespace);
    
    Task<V1Service?> UpdateService(ServiceModel model);
    
    Task<V1Service?> DeleteService(string name, string @namespace);
}