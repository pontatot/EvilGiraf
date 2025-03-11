using k8s.Models;

namespace EvilGiraf.Model;

public record DeploymentModel(string Name, string Namespace, int Replicas, string Image, int[] Ports);

public static class DeploymentModelExtensions
{
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
                                Ports = model.Ports.Select(x => new V1ContainerPort(x)).ToList()
                            }
                        }
                    }
                }
            }
        };
    }
}
