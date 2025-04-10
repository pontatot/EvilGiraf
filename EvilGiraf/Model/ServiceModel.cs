using k8s.Models;

namespace EvilGiraf.Model;

public record ServiceModel(string Name, string Namespace, string Type, int[] Ports, string Protocol, string Selector);

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
                Type = model.Type,
                Ports = model.Ports.Select(p => new V1ServicePort
                {
                    Port = p,
                    TargetPort = p,
                    Protocol = model.Protocol
                }).ToList(),
                Selector = new Dictionary<string, string>
                {
                    { "app", model.Selector }
                }
            }
        };
    }
}