using k8s.Models;

namespace EvilGiraf.Model;

public class ServiceModel
{
    public required string Name { get; set; }
    
    public required string Namespace { get; set; }
    
    public required string Type { get; set; }
    
    public required int[] Ports { get; set; }
    
    public required string TargetPort { get; set; }
    
    public required string Protocol { get; set; }
    
    public required string Selector { get; set; }
    
    public V1Service ToService()
    {
        return new V1Service
        {
            Metadata = new V1ObjectMeta
            {
                Name = Name,
                NamespaceProperty = Namespace
            },
            Spec = new V1ServiceSpec
            {
                Type = Type,
                Ports = Ports.Select(p => new V1ServicePort
                {
                    Port = p,
                    TargetPort = TargetPort,
                    Protocol = Protocol
                }).ToList(),
                Selector = new Dictionary<string, string>
                {
                    { "app", Selector }
                }
            }
        };
    }
}