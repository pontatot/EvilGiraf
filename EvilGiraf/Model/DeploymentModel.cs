using k8s.Models;

namespace EvilGiraf.Model;

public record DeploymentModel(string Name, string Namespace, int Replicas, string Image, int? Port);

public static class DeploymentModelExtensions
{
    public static string ToNamespace(this int id) => $"evilgiraf-{id}";
    
    public static V1Deployment ToDeployment(this DeploymentModel model)
    {
        return new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Name = model.Name
            },
            Spec = new V1DeploymentSpec
            {
                Replicas = model.Replicas,
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        { "app", model.Name }
                    }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            { "app", model.Name }
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new()
                            {
                                Name = model.Name,
                                Image = model.Image,
                                Ports = model.Port is null ?
                                    new List<V1ContainerPort>() :
                                    new List<V1ContainerPort>{new(model.Port.Value)}
                            }
                        }
                    }
                }
            }
        };
    }
}
