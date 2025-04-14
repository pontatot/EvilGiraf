using k8s.Models;

namespace EvilGiraf.Model.Kubernetes;

public record ServiceModel(string Name, string Namespace, int Port);

public static class ServiceModelExtensions
{   
    public static V1Service ToService(this ServiceModel model)
    {
        return new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            },
            Spec = new V1ServiceSpec
            {
                Type = "ClusterIP",
                Ports = new List<V1ServicePort>{ new(model.Port)},
                Selector = new Dictionary<string, string>
                {
                    { "app", model.Name }
                }
            }
        };
    }
}