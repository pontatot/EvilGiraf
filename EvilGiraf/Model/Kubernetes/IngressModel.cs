using k8s.Models;

namespace EvilGiraf.Model.Kubernetes;

public record IngressModel(
    string Name,
    string Namespace,
    string Host,
    int Port,
    string Path
);

public static class IngressModelExtensions
{
    public static V1Ingress ToIngress(this IngressModel model)
    {
        return new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name,
                NamespaceProperty = model.Namespace
            },
            Spec = new V1IngressSpec
            {
                Rules = new List<V1IngressRule>
                {
                    new()
                    {
                        Host = model.Host,
                        Http = new V1HTTPIngressRuleValue
                        {
                            Paths = new List<V1HTTPIngressPath>
                            {
                                new()
                                {
                                    Path = model.Path,
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = model.Name,
                                            Port = new V1ServiceBackendPort
                                            {
                                                Number = model.Port
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}