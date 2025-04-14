using EvilGiraf.Model.Kubernetes;
using k8s.Models;

namespace EvilGiraf.Interface.Kubernetes;

public interface IConfigMapService
{
    Task<V1ConfigMap> CreateConfigMap(ConfigMapModel model);
    Task<V1ConfigMap?> ReadConfigMap(string name, string @namespace);
    Task<V1ConfigMap?> UpdateConfigMap(ConfigMapModel model);
    Task<V1Status?> DeleteConfigMap(string name, string @namespace);
    Task<V1ConfigMap?> CreateOrReplaceConfigMap(ConfigMapModel model);
}