using System.Net;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace EvilGiraf.Service;

public class DeploymentService : IDeploymentService
{
    private readonly IKubernetes _client;
    
    public DeploymentService(IKubernetes client)
    {
        _client = client;
    }
    
    public async Task<V1Deployment> CreateDeployment(DeploymentModel model)
    {
        var deployment = new V1Deployment
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
        return await _client.AppsV1.CreateNamespacedDeploymentAsync(deployment, model.Namespace);
    }
    
    public async Task<V1Deployment> ReadDeployment(string name, string @namespace)
    {
        try
        {
            var deployment = await _client.AppsV1.ReadNamespacedDeploymentAsync(name, @namespace);
            return deployment;
        }
        catch (HttpOperationException e)
        {
            if (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Deployment '{name}' not found in namespace '{@namespace}'");
            }
            throw;
        }
    }

    public async Task<V1Status> DeleteDeployment(string name, string @namespace)
    {
        return await _client.AppsV1.DeleteNamespacedDeploymentAsync(name, @namespace);
    }

    public async Task<V1Deployment> UpdateDeployment(DeploymentModel model)
    {
        var deployment = new V1Deployment
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
        return await _client.AppsV1.ReplaceNamespacedDeploymentAsync(deployment, model.Name, model.Namespace);
    }
}